using System;
using PCZamanlayici;
class SchedulerValidation {
 static int Main(){try{var backend=new WindowsDailyScheduler();backend.ValidateOnly();Console.WriteLine("PASS: Windows Task Scheduler accepted daily XML using TASK_VALIDATE_ONLY; no task was registered or run.");var existing=backend.Read();Console.WriteLine(existing==null?"No active daily plan for this user.":"An existing daily plan was read without changing it.");return 0;}catch(Exception e){Console.WriteLine("FAIL: "+e);return 1;}}
}
