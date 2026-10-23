using System;
using System.IO;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace PCZamanlayici {
 public sealed class DailyState { public DailySpec Spec; public bool Enabled; }
 public interface IDailyScheduler { DailyState Read(); void Save(DailySpec spec); void Remove(); }
 public sealed class DemoDailyScheduler:IDailyScheduler {
  public string SavedXml;public int Saves,Removes;
  const string Sid="S-1-5-21-demo";const string Command=@"C:\Windows\System32\shutdown.exe";
  public DailyState Read(){return SavedXml==null?null:new DailyState{Spec=DailyPlan.ReadXml(SavedXml,Sid,Command,DateTime.Now),Enabled=true};}
  public void Save(DailySpec spec){SavedXml=DailyPlan.Xml(spec,Sid,Command);Saves++;}
  public void Remove(){SavedXml=null;Removes++;}
 }
 public sealed class WindowsDailyScheduler:IDailyScheduler {
  const string FolderPath=@"\PCZamanlayici";
  readonly string sid=WindowsIdentity.GetCurrent().User.Value;
  readonly string command=Path.Combine(Environment.SystemDirectory,"shutdown.exe");
  readonly string testSuffix="";
  public WindowsDailyScheduler() { }
  internal WindowsDailyScheduler(string harmlessTestExecutable){
   if(Path.GetFileName(harmlessTestExecutable)!="SchedulerNoop.exe" || !File.Exists(harmlessTestExecutable))throw new ArgumentException("Only the inert integration-test executable is allowed.");
   command=Path.GetFullPath(harmlessTestExecutable);testSuffix="-INTEGRATION-TEST";
  }
  string Name {get{return "Daily-"+sid+testSuffix;}}
  dynamic Service(){dynamic service=Activator.CreateInstance(Type.GetTypeFromProgID("Schedule.Service"));service.Connect();return service;}
  bool Missing(Exception e){return e.HResult==unchecked((int)0x80070002) || e.HResult==unchecked((int)0x80070003);}
  dynamic Folder(dynamic service,bool create){
   try{return service.GetFolder(FolderPath);}catch(Exception e){if(!Missing(e))throw;if(!create)return null;return service.GetFolder(@"\").CreateFolder("PCZamanlayici",null);}
  }
  dynamic Existing(dynamic folder){
   if(folder==null)return null;
   try{return folder.GetTask(Name);}catch(Exception e){if(Missing(e))return null;throw;}
  }
  DailyState Parse(dynamic task){return new DailyState{Spec=DailyPlan.ReadXml((string)task.Xml,sid,command,DateTime.Now),Enabled=(bool)task.Enabled};}
  public DailyState Read(){dynamic service=Service();dynamic folder=Folder(service,false);dynamic task=Existing(folder);return task==null?null:Parse(task);}
  public void Save(DailySpec spec){
   dynamic service=Service();dynamic folder=Folder(service,true);dynamic old=Existing(folder);
   if(old!=null)Parse(old); // Never replace an unrelated definition with the same name.
   string xml=DailyPlan.Xml(spec,sid,command);
   folder.RegisterTask(Name,xml,6,sid,null,3,null);
   dynamic check=Existing(folder);if(check==null)throw new InvalidOperationException("Günlük plan Windows'a kaydedilemedi.");
   DailyState restored=Parse(check);
   if(!restored.Enabled || restored.Spec.Time!=spec.Time || restored.Spec.Action!=spec.Action)throw new InvalidOperationException("Windows günlük plan doğrulaması başarısız.");
  }
  public void Remove(){
   dynamic service=Service();dynamic folder=Folder(service,false);dynamic task=Existing(folder);
   if(task==null)return;Parse(task);folder.DeleteTask(Name,0);
   if(Existing(folder)!=null)throw new InvalidOperationException("Günlük plan silinemedi.");
  }
  public void ValidateOnly(){
   dynamic service=Service();dynamic root=service.GetFolder(@"\");
   string validationName="PCZamanlayici-Validation-"+Guid.NewGuid().ToString("N");
   string xml=DailyPlan.Xml(DailyPlan.Create(DateTime.Now,new TimeSpan(23,59,0),PowerAction.Shutdown),sid,command);
   // TASK_VALIDATE_ONLY=1 validates XML but never creates or runs a task.
   root.RegisterTask(validationName,xml,1,sid,null,3,null);
   try{dynamic unexpected=root.GetTask(validationName);throw new InvalidOperationException("Validation unexpectedly registered a task; abort.");}
   catch(Exception e){if(!Missing(e))throw;}
  }
 }
}
