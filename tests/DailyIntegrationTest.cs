using System;
using System.IO;
using PCZamanlayici;
class DailyIntegrationTest {
 static int Main(){
  var test=new WindowsDailyScheduler(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"SchedulerNoop.exe"));
  bool created=false;
  try{
   if(test.Read()!=null)throw new Exception("Preexisting test task; refusing to replace it.");
   created=true;
   test.Save(DailyPlan.Create(DateTime.Now,new TimeSpan(23,59,0),PowerAction.Shutdown));
   var restored=new WindowsDailyScheduler(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"SchedulerNoop.exe")).Read();
   if(restored==null || restored.Spec.Time!=new TimeSpan(23,59,0))throw new Exception("Persistent restore failed.");
   Console.WriteLine("PASS real Task Scheduler registration and restore using an INERT executable.");
   test.Save(DailyPlan.Create(DateTime.Now,new TimeSpan(22,15,0),PowerAction.Restart));
   if(test.Read().Spec.Action!=PowerAction.Restart || test.Read().Spec.Time!=new TimeSpan(22,15,0))throw new Exception("Update failed.");
   Console.WriteLine("PASS real daily task update; INERT executable only.");
   test.Remove();created=false;if(test.Read()!=null)throw new Exception("Removal failed.");
   Console.WriteLine("PASS test task deleted and absence read back. No shutdown/restart task was registered, and no task was run.");return 0;
  }catch(Exception e){Console.WriteLine("FAIL "+e);return 1;}
  finally{if(created){try{test.Remove();Console.WriteLine("Test task cleanup completed.");}catch(Exception e){Console.WriteLine("CLEANUP FAILED: "+e.Message);}}}
 }
}
