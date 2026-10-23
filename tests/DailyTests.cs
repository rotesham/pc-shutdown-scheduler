using System;
using System.Xml.Linq;
using PCZamanlayici;
class DailyTests {
 static int passed,failed;static readonly DateTime Now=new DateTime(2026,1,1,18,0,0);
 const string Sid="S-1-5-21-1234";const string Command=@"C:\Windows\System32\shutdown.exe";
 static void Test(string n,Action t){try{t();Console.WriteLine("PASS "+n);passed++;}catch(Exception e){Console.WriteLine("FAIL "+n+": "+e.Message);failed++;}}
 static void Equal(object a,object b){if(!a.Equals(b))throw new Exception("Expected "+b+", got "+a);}
 static void Reject(Action a){try{a();}catch(ArgumentException){return;}throw new Exception("Unsafe definition accepted");}
 static DailySpec Spec(){return DailyPlan.Create(Now,new TimeSpan(23,59,0),PowerAction.Shutdown);}
 static int Main(){
  Test("daily today",()=>Equal(Spec().NextTarget,new DateTime(2026,1,1,23,59,0)));
  Test("warning one minute before",()=>Equal(Spec().TriggerAt,new DateTime(2026,1,1,23,58,0)));
  Test("past rolls tomorrow",()=>Equal(DailyPlan.Create(new DateTime(2026,1,1,23,59,1),new TimeSpan(23,59,0),PowerAction.Restart).NextTarget,new DateTime(2026,1,2,23,59,0)));
  Test("too near rolls tomorrow",()=>Equal(DailyPlan.Create(new DateTime(2026,1,1,23,58,30),new TimeSpan(23,59,0),PowerAction.Shutdown).NextTarget,new DateTime(2026,1,2,23,59,0)));
  Test("midnight warning previous day",()=>Equal(DailyPlan.Create(Now,TimeSpan.Zero,PowerAction.Shutdown).TriggerAt,new DateTime(2026,1,1,23,59,0)));
  Test("negative time rejected",()=>Reject(()=>DailyPlan.Create(Now,TimeSpan.FromMinutes(-1),PowerAction.Shutdown)));
  Test("24h rejected",()=>Reject(()=>DailyPlan.Create(Now,TimeSpan.FromDays(1),PowerAction.Shutdown)));
  Test("seconds rejected",()=>Reject(()=>DailyPlan.Create(Now,new TimeSpan(23,59,1),PowerAction.Shutdown)));
  Test("invalid action rejected",()=>Reject(()=>DailyPlan.Create(Now,new TimeSpan(23,59,0),(PowerAction)99)));
  Test("daily XML interval",()=>{var x=XDocument.Parse(DailyPlan.Xml(Spec(),Sid,Command));XNamespace n="http://schemas.microsoft.com/windows/2004/02/mit/task";Equal(x.Root.Element(n+"Triggers").Element(n+"CalendarTrigger").Element(n+"ScheduleByDay").Element(n+"DaysInterval").Value,"1");});
  Test("daily XML grace",()=>{string x=DailyPlan.Xml(Spec(),Sid,Command);if(!x.Contains("/s /t 60"))throw new Exception("Missing safety countdown");});
  Test("missed trigger not replayed",()=>{string x=DailyPlan.Xml(Spec(),Sid,Command);if(!x.Contains("<StartWhenAvailable>false</StartWhenAvailable>"))throw new Exception("Catch-up enabled");});
  Test("no wake",()=>{string x=DailyPlan.Xml(Spec(),Sid,Command);if(!x.Contains("<WakeToRun>false</WakeToRun>"))throw new Exception("Wake enabled");});
  Test("restore persists daily time",()=>Equal(DailyPlan.ReadXml(DailyPlan.Xml(Spec(),Sid,Command),Sid,Command,Now).Time,new TimeSpan(23,59,0)));
  Test("restore restart",()=>{var s=DailyPlan.Create(Now,new TimeSpan(7,30,0),PowerAction.Restart);Equal(DailyPlan.ReadXml(DailyPlan.Xml(s,Sid,Command),Sid,Command,Now).Action,PowerAction.Restart);});
  Test("foreign owner rejected",()=>Reject(()=>DailyPlan.ReadXml(DailyPlan.Xml(Spec(),Sid,Command),"S-1-5-21-5678",Command,Now)));
  Test("tampered command rejected",()=>Reject(()=>DailyPlan.ReadXml(DailyPlan.Xml(Spec(),Sid,Command).Replace("shutdown.exe","other.exe"),Sid,Command,Now)));
  Test("XML escaping",()=>{string cmd=@"C:\a&b\shutdown.exe";Equal(DailyPlan.ReadXml(DailyPlan.Xml(Spec(),Sid,cmd),Sid,cmd,Now).Time,Spec().Time);});
  Test("tampered trigger rejected",()=>Reject(()=>DailyPlan.ReadXml(DailyPlan.Xml(Spec(),Sid,Command).Replace("2026-01-01T23:58:00","2026-01-01T20:00:00"),Sid,Command,Now)));
  Test("catch-up tampering rejected",()=>Reject(()=>DailyPlan.ReadXml(DailyPlan.Xml(Spec(),Sid,Command).Replace("<StartWhenAvailable>false</StartWhenAvailable>","<StartWhenAvailable>true</StartWhenAvailable>"),Sid,Command,Now)));
  Test("Windows omits default flags",()=>{string x=DailyPlan.Xml(Spec(),Sid,Command).Replace("<Enabled>true</Enabled>","").Replace("<StartWhenAvailable>false</StartWhenAvailable>","").Replace("<WakeToRun>false</WakeToRun>","");Equal(DailyPlan.ReadXml(x,Sid,Command,Now).Time,Spec().Time);});
  Test("future start remains first eligible target",()=>{
   string x=DailyPlan.Xml(Spec(),Sid,Command).Replace("2026-01-01T23:58:00","2026-02-05T23:58:00");
   var restored=DailyPlan.ReadXml(x,Sid,Command,Now);Equal(restored.NextTarget,new DateTime(2026,2,5,23,59,0));Equal(restored.TriggerAt,new DateTime(2026,2,5,23,58,0));
  });
  Test("historical daily start restores after days and reboots",()=>{
   string x=DailyPlan.Xml(Spec(),Sid,Command).Replace("2026-01-01T23:58:00","2025-11-10T23:58:00");
   Equal(DailyPlan.ReadXml(x,Sid,Command,Now.AddDays(8)).NextTarget,new DateTime(2026,1,9,23,59,0));
   Equal(DailyPlan.ReadXml(x,Sid,Command,new DateTime(2026,1,9,23,58,0)).NextTarget,new DateTime(2026,1,10,23,59,0));
   Equal(DailyPlan.ReadXml(x,Sid,Command,new DateTime(2026,1,9,23,58,30)).NextTarget,new DateTime(2026,1,10,23,59,0));
  });
  Test("future midnight start preserves previous-day warning",()=>{
   string x=DailyPlan.Xml(DailyPlan.Create(Now,TimeSpan.Zero,PowerAction.Shutdown),Sid,Command).Replace("2026-01-01T23:59:00","2026-02-05T23:59:00");
   var restored=DailyPlan.ReadXml(x,Sid,Command,Now);Equal(restored.NextTarget,new DateTime(2026,2,6,0,0,0));Equal(restored.TriggerAt,new DateTime(2026,2,5,23,59,0));
  });
  Test("UTC start boundary rejected rather than treated as local time",()=>Reject(()=>DailyPlan.ReadXml(DailyPlan.Xml(Spec(),Sid,Command).Replace("2026-01-01T23:58:00","2026-01-01T23:58:00Z"),Sid,Command,Now)));
  Test("unsupported trigger behavior rejected",()=>{
   XNamespace n="http://schemas.microsoft.com/windows/2004/02/mit/task";
   foreach(var extra in new[]{new XElement(n+"Repetition",new XElement(n+"Interval","PT5M")),new XElement(n+"EndBoundary","2025-12-31T23:59:00"),new XElement(n+"RandomDelay","PT5M"),new XElement(n+"StartBoundary","2026-01-02T23:58:00")}){
    var x=XDocument.Parse(DailyPlan.Xml(Spec(),Sid,Command));x.Root.Element(n+"Triggers").Element(n+"CalendarTrigger").Add(extra);
    Reject(()=>DailyPlan.ReadXml(x.ToString(),Sid,Command,Now));
   }
  });
  Console.WriteLine("RESULT "+passed+" passed; "+failed+" failed. No task registered or power command executed.");return failed==0?0:1;
 }
}
