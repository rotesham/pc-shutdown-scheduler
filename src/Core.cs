using System;
namespace PCZamanlayici {
 public enum PowerAction { Shutdown, Restart }
 public static class Plan {
  public static int At(DateTime now,DateTime target) {
   double seconds=(target-now).TotalSeconds;
   if(seconds<30 || seconds>31536000) throw new ArgumentException("Tarih en az 30 saniye ileride, en fazla 365 gun sonra olmali.");
   return (int)Math.Ceiling(seconds);
  }
  public static string Arguments(PowerAction action,int delay) {
   if(action!=PowerAction.Shutdown && action!=PowerAction.Restart) throw new ArgumentException("Gecersiz islem.");
   if(delay<30 || delay>31536000) throw new ArgumentException("Gecersiz sure.");
   return (action==PowerAction.Shutdown?"/s":"/r")+" /t "+delay.ToString(System.Globalization.CultureInfo.InvariantCulture);
  }
  public static int After(DateTime now,int hours,int minutes,int seconds) {
   if(hours<0 || hours>8760 || minutes<0 || minutes>59 || seconds<0 || seconds>59) throw new ArgumentException("Sure degerleri gecersiz.");
   long total=(long)hours*3600+minutes*60+seconds;
   if(total<30 || total>31536000) throw new ArgumentException("Sure en az 30 saniye, en fazla 365 gun olabilir.");
   return (int)total;
  }
 }
}
