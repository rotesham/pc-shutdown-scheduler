using System;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
namespace PCZamanlayici {
 public sealed class DailySpec {
  public TimeSpan Time; public PowerAction Action; public DateTime NextTarget;
  public DateTime TriggerAt { get { return NextTarget.AddSeconds(-60); } }
 }
 public static class DailyPlan {
  public static string Xml(DailySpec spec,string userSid,string command){
   if(spec==null || string.IsNullOrWhiteSpace(userSid) || string.IsNullOrWhiteSpace(command))throw new ArgumentException("Günlük görev bilgileri eksik.");
   XNamespace n="http://schemas.microsoft.com/windows/2004/02/mit/task";
   var x=new XElement(n+"Task",new XAttribute("version","1.2"),
    new XElement(n+"RegistrationInfo",new XElement(n+"Author","PC Zamanlayici"),new XElement(n+"Description","Her gün "+spec.Time.ToString(@"hh\:mm")+"; 60 saniye önceden uyarı. Kaçırılan çalışma tekrarlanmaz.")),
    new XElement(n+"Triggers",new XElement(n+"CalendarTrigger",new XElement(n+"StartBoundary",spec.TriggerAt.ToString("yyyy-MM-ddTHH:mm:ss",CultureInfo.InvariantCulture)),new XElement(n+"Enabled","true"),new XElement(n+"ScheduleByDay",new XElement(n+"DaysInterval","1")))),
    new XElement(n+"Principals",new XElement(n+"Principal",new XAttribute("id","Author"),new XElement(n+"UserId",userSid),new XElement(n+"LogonType","InteractiveToken"),new XElement(n+"RunLevel","LeastPrivilege"))),
    new XElement(n+"Settings",new XElement(n+"MultipleInstancesPolicy","IgnoreNew"),new XElement(n+"DisallowStartIfOnBatteries","false"),new XElement(n+"StopIfGoingOnBatteries","false"),new XElement(n+"StartWhenAvailable","false"),new XElement(n+"AllowStartOnDemand","false"),new XElement(n+"Enabled","true"),new XElement(n+"Hidden","true"),new XElement(n+"WakeToRun","false"),new XElement(n+"ExecutionTimeLimit","PT2M")),
    new XElement(n+"Data","PCZamanlayici-v2|"+spec.Time.ToString(@"hh\:mm")+"|"+((int)spec.Action).ToString(CultureInfo.InvariantCulture)),
    new XElement(n+"Actions",new XAttribute("Context","Author"),new XElement(n+"Exec",new XElement(n+"Command",command),new XElement(n+"Arguments",Plan.Arguments(spec.Action,60)))));
   return x.ToString(SaveOptions.DisableFormatting);
  }
  public static DailySpec ReadXml(string xml,string userSid,string command,DateTime now){
   try{
    XNamespace n="http://schemas.microsoft.com/windows/2004/02/mit/task";var r=XDocument.Parse(xml).Root;
    string[] data=r.Element(n+"Data").Value.Split('|');
    if(data.Length!=3 || data[0]!="PCZamanlayici-v2")throw new ArgumentException("Bu görev uygulamaya ait değil.");
    var time=TimeSpan.ParseExact(data[1],@"hh\:mm",CultureInfo.InvariantCulture);
    var spec=Create(now,time,(PowerAction)int.Parse(data[2],CultureInfo.InvariantCulture));
    var p=r.Element(n+"Principals").Element(n+"Principal");
    if(p.Element(n+"UserId").Value!=userSid || p.Element(n+"LogonType").Value!="InteractiveToken")throw new ArgumentException("Günlük görev başka kullanıcıya ait.");
    var acts=r.Element(n+"Actions");var exec=acts.Element(n+"Exec");
    if(acts.Elements().Count()!=1 || exec==null || !string.Equals(exec.Element(n+"Command").Value,command,StringComparison.OrdinalIgnoreCase) || exec.Element(n+"Arguments").Value!=Plan.Arguments(spec.Action,60))throw new ArgumentException("Günlük görev eylemi beklenenle uyuşmuyor.");
    var triggers=r.Element(n+"Triggers");var t=triggers.Element(n+"CalendarTrigger");
    if(triggers.Elements().Count()!=1 || t==null || t.Element(n+"ScheduleByDay").Element(n+"DaysInterval").Value!="1")throw new ArgumentException("Günlük görev tetikleyicisi uyuşmuyor.");
    // Only the local, once-per-day trigger shape emitted by this application is supported.
    if(t.Elements().Any(e=>e.Name!=n+"StartBoundary" && e.Name!=n+"Enabled" && e.Name!=n+"ScheduleByDay") || t.Elements().GroupBy(e=>e.Name).Any(g=>g.Count()!=1))throw new ArgumentException("Günlük görev tetikleyicisi desteklenmiyor.");
    var schedule=t.Element(n+"ScheduleByDay");
    if(schedule.Elements().Count()!=1 || schedule.Element(n+"DaysInterval")==null)throw new ArgumentException("Günlük görev tekrarı desteklenmiyor.");
    DateTime actualStart=DateTime.Parse(t.Element(n+"StartBoundary").Value,CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind);
    if(actualStart.Kind!=DateTimeKind.Unspecified || actualStart.TimeOfDay!=spec.TriggerAt.TimeOfDay || (t.Element(n+"Enabled")!=null && t.Element(n+"Enabled").Value!="true"))throw new ArgumentException("Günlük görev saati değiştirilmiş.");
    // Historical boundaries recur normally; a future boundary must not be moved earlier.
    if(actualStart>spec.TriggerAt)spec.NextTarget=actualStart.AddSeconds(60);
    var settings=r.Element(n+"Settings");
    if((settings.Element(n+"StartWhenAvailable")!=null && settings.Element(n+"StartWhenAvailable").Value!="false") || (settings.Element(n+"WakeToRun")!=null && settings.Element(n+"WakeToRun").Value!="false"))throw new ArgumentException("Günlük görev güvenlik ayarları değiştirilmiş.");
    return spec;
   }catch(ArgumentException){throw;}catch(Exception e){throw new ArgumentException("Günlük görev okunamadı: "+e.Message);}
  }
  public static DailySpec Create(DateTime now,TimeSpan time,PowerAction action){
   if(time<TimeSpan.Zero || time>=TimeSpan.FromDays(1) || time.Seconds!=0 || time.Milliseconds!=0)throw new ArgumentException("Günlük saat 00:00–23:59 arasında olmalı.");
   if(action!=PowerAction.Shutdown && action!=PowerAction.Restart)throw new ArgumentException("Geçersiz işlem.");
   DateTime target=now.Date.Add(time);
   if(target.AddSeconds(-60)<=now)target=target.AddDays(1);
   return new DailySpec{Time=time,Action=action,NextTarget=target};
  }
 }
}
