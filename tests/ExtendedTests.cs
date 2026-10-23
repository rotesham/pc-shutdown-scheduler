using System;
using PCZamanlayici;
class ExtendedTests {
 static int passed,failed;
 static void Test(string name,Action test){try{test();Console.WriteLine("PASS "+name);passed++;}catch(Exception e){Console.WriteLine("FAIL "+name+": "+e.Message);failed++;}}
 static void Equal(object actual,object expected){if(!actual.Equals(expected))throw new Exception("Expected "+expected+", got "+actual);}
 static void Reject(Action a){try{a();}catch(ArgumentException){return;}throw new Exception("Invalid plan accepted");}
 static int Main(){
  DateTime now=new DateTime(2026,1,1,23,30,0);
  Test("relative delay",()=>Equal(Plan.After(now,1,30,5),5405));
  Test("zero rejected",()=>Reject(()=>Plan.After(now,0,0,0)));
  Test("under 30s rejected",()=>Reject(()=>Plan.After(now,0,0,29)));
  Test("30s allowed",()=>Equal(Plan.After(now,0,0,30),30));
  Test("negative rejected",()=>Reject(()=>Plan.After(now,-1,0,0)));
  Test("bad minutes rejected",()=>Reject(()=>Plan.After(now,0,60,0)));
  Test("365 days allowed",()=>Equal(Plan.After(now,8760,0,0),31536000));
  Test("above limit rejected",()=>Reject(()=>Plan.After(now,8760,0,1)));
  Test("midnight target",()=>Equal(Plan.At(now,new DateTime(2026,1,2,0,15,0)),2700));
  Test("past target rejected",()=>Reject(()=>Plan.At(now,now.AddSeconds(-1))));
  Test("fraction rounded safely",()=>Equal(Plan.At(now,now.AddMilliseconds(30500)),31));
  Test("same time rejected",()=>Reject(()=>Plan.At(now,now)));
  Test("far target rejected",()=>Reject(()=>Plan.At(now,now.AddDays(366))));
  Test("shutdown arguments",()=>Equal(Plan.Arguments(PowerAction.Shutdown,3600),"/s /t 3600"));
  Test("restart arguments",()=>Equal(Plan.Arguments(PowerAction.Restart,120),"/r /t 120"));
  Test("zero command rejected",()=>Reject(()=>Plan.Arguments(PowerAction.Shutdown,0)));
  Test("invalid action rejected",()=>Reject(()=>Plan.Arguments((PowerAction)99,60)));
  Console.WriteLine("RESULT "+passed+" passed; "+failed+" failed. No OS power command executed.");return failed==0?0:1;
 }
}
