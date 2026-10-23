using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using PCZamanlayici;
class SmokeTests {
 static int passed,failed;
 static void Test(string name,Action test){try{test();Console.WriteLine("PASS "+name);passed++;}catch(Exception e){Console.WriteLine("FAIL "+name+": "+e.GetBaseException().GetType().Name+": "+e.GetBaseException().Message);failed++;}finally{Environment.ExitCode=0;}}
 static T Field<T>(MainForm form,string name){return (T)typeof(MainForm).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(form);}
 static void Invoke(MainForm form,string name,params object[] args){typeof(MainForm).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(form,args);}
 static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
 static void Run(string output){
  Environment.ExitCode=0;
  using(var form=new MainForm(new DemoBackend(),true,false,new DemoDailyScheduler(),output)){
   form.Show();
   typeof(MainForm).GetMethod("RunSmoke",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(form,null);
  }
 }
 [STAThread] static int Main(){
  Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
  string root=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"smoke-tests-"+Guid.NewGuid().ToString("N"));
  Directory.CreateDirectory(root);
  try{
   Test("first run displays English title and controls",()=>{
    using(var form=new MainForm(new DemoBackend(),true,false,new DemoDailyScheduler(),Path.Combine(root,"language-default"))){
     if(!form.Text.StartsWith("PC Shutdown Scheduler"))throw new Exception("Expected English display title");
     var action=(ComboBox)typeof(MainForm).GetField("action",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(form);
     if(action.Text!="Shut down computer")throw new Exception("Expected English action");
    }
   });
   Test("language switch persists and preserves active plan without backend actions",()=>{
    string output=Path.Combine(root,"languages");var backend=new DemoBackend();var daily=new DemoDailyScheduler();
    using(var form=new MainForm(backend,true,false,daily,output)){
     var language=Field<ComboBox>(form,"language");var action=Field<ComboBox>(form,"action");
     Require(language.Items[0].ToString()=="English" && language.Items[1].ToString()=="Türkçe","Clear selector labels");
     Field<CheckBox>(form,"saved").Checked=true;Invoke(form,"Schedule",true);
     var target=Field<DateTime?>(form,"plannedAt");action.SelectedIndex=1;
     language.SelectedIndex=1;
     Require(form.Text.StartsWith("PC Zamanlayıcı") && action.Text=="Yeniden başlat","Turkish controls");
     Require(Field<Label>(form,"countdown").Text.StartsWith("Kalan: "),"Turkish countdown");
     Require(Field<Label>(form,"status").Text.EndsWith("Bilgisayarı kapat"),"Active plan status retains scheduled action, not edited selection");
     Require(Field<Label>(form,"preview").Text.StartsWith("Hedef: "),"Turkish target preview");
     language.SelectedIndex=0;
     Require(Field<Label>(form,"countdown").Text.StartsWith("Remaining: "),"English countdown");
     Require(Field<Label>(form,"status").Text.EndsWith("Shut down computer"),"English active plan status");
     Require(backend.Calls==1 && daily.Saves==0 && daily.Removes==0 && Field<DateTime?>(form,"plannedAt")==target,"Language cannot alter plans");
     language.SelectedIndex=1;
    }
    using(var reopened=new MainForm(new DemoBackend(),true,false,new DemoDailyScheduler(),output))Require(Field<ComboBox>(reopened,"language").SelectedIndex==1,"Local preference restored");
    File.WriteAllText(Path.Combine(output,"PCZamanlayici","language.txt"),"unsupported");
    using(var reopened=new MainForm(new DemoBackend(),true,false,new DemoDailyScheduler(),output))Require(Field<ComboBox>(reopened,"language").SelectedIndex==0,"Invalid preference defaults English");
   });
   Test("dynamic validation daily labels and backend errors translate both ways",()=>{
    using(var form=new MainForm(new DemoBackend(),true,false,new DemoDailyScheduler(),Path.Combine(root,"dynamic"))){
     var language=Field<ComboBox>(form,"language");
     Field<NumericUpDown>(form,"hours").Value=0;
     Require(Field<Label>(form,"preview").Text.StartsWith("The delay must"),"English validation");
     language.SelectedIndex=1;Require(Field<Label>(form,"preview").Text.StartsWith("Sure en az"),"Turkish validation");
     Field<RadioButton>(form,"daily").Checked=true;
     Require(Field<Label>(form,"preview").Text.Contains("İlk hedef:"),"Turkish daily preview");
     Field<CheckBox>(form,"saved").Checked=true;Invoke(form,"ScheduleDaily",true);
     language.SelectedIndex=0;
     Require(Field<Label>(form,"preview").Text.Contains("First target:"),"English daily preview");
     Require(Field<Label>(form,"dailyInfo").Text.StartsWith("Every day ") && Field<Label>(form,"status").Text.Contains("Daily alarm saved"),"English daily labels");
     Invoke(form,"ShowFailure",new BackendResult(1115,"ignored"),false,true);
     Require(Field<Label>(form,"status").Text.StartsWith("Windows already"),"English known error");
     language.SelectedIndex=1;Require(Field<Label>(form,"status").Text.StartsWith("Windows'ta zaten"),"Live error translation");
     string native="C:\\Türkçe\\app.exe /literal Her gün";
     Require(UiText.Error(native,false)==native,"Unknown OS text paths and arguments remain untouched");
     Invoke(form,"ShowFailure",new BackendResult(5,native),false,true);
     language.SelectedIndex=0;
     Require(Field<Label>(form,"status").Text.StartsWith("Windows did not complete") && Field<Label>(form,"status").Text.EndsWith(native),"Localized error context, untouched native detail");
    }
   });
   Test("smoke creates fresh nested output directory",()=>{
    string output=Path.Combine(root,"fresh","output");
    if(Directory.Exists(output))throw new Exception("Output must not exist before smoke");
    Run(output);
    if(Environment.ExitCode!=0 || !File.ReadAllText(Path.Combine(output,"smoke-result.txt")).StartsWith("PASS:") || new FileInfo(Path.Combine(output,"ui-preview-v2.png")).Length==0)throw new Exception("Missing smoke output");
    Require(File.ReadAllText(Path.Combine(output,"smoke-result.txt")).Contains("English/Turkish switches preserve active plans and task XML"),"Smoke must verify bilingual switching without side effects");
   });
   Test("unusable output directory returns failure without escaping",()=>{
    string output=Path.Combine(root,"not-a-directory");File.WriteAllText(output,"blocked");
    Run(output);if(Environment.ExitCode!=1)throw new Exception("Expected failure exit code");
   });
   Test("unwritable report returns failure without a second exception",()=>{
    string output=Path.Combine(root,"blocked-report");Directory.CreateDirectory(Path.Combine(output,"smoke-result.txt"));
    Run(output);if(Environment.ExitCode!=1)throw new Exception("Expected failure exit code");
   });
  }finally{Directory.Delete(root,true);}
  Console.WriteLine("RESULT "+passed+" passed; "+failed+" failed. Fake backends only; isolated output removed.");return failed==0?0:1;
 }
}
