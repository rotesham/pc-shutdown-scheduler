using System;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace PCZamanlayici {
 public sealed class BackendResult {
  public int Code; public string Detail;
  public BackendResult(int code,string detail){Code=code;Detail=detail;}
 }
 public interface IBackend { BackendResult Run(string arguments); }
 public sealed class WindowsBackend:IBackend {
  public BackendResult Run(string arguments){
   var start=new ProcessStartInfo(Path.Combine(Environment.SystemDirectory,"shutdown.exe"),arguments);
   start.UseShellExecute=false;start.CreateNoWindow=true;start.RedirectStandardOutput=true;start.RedirectStandardError=true;
   using(var process=Process.Start(start)){
    if(!process.WaitForExit(10000)) return new BackendResult(-1,"Windows yanıtı zaman aşımına uğradı. İşlem durumunu kontrol edin.");
    string output=process.StandardOutput.ReadToEnd()+process.StandardError.ReadToEnd();
    return new BackendResult(process.ExitCode,output.Trim());
   }
  }
 }
 public sealed class DemoBackend:IBackend {
  public int Calls;public string LastArguments="";
  public BackendResult Run(string arguments){Calls++;LastArguments=arguments;return new BackendResult(0,"DEMO: Windows güç komutu çalıştırılmadı.");}
 }
 // Exact, presentation-only resources. Scheduler XML, command arguments, ownership
 // identifiers and unknown Windows error details are never translated.
 public static class UiText {
  static readonly System.Collections.Generic.Dictionary<string,string> english=new System.Collections.Generic.Dictionary<string,string>(StringComparer.Ordinal){
   {"PC Zamanlayıcı","PC Shutdown Scheduler"},
   {" — GÜVENLİ DEMO"," — SAFE DEMO"},
   {"DEMO • İşlemler simüle edilir; bilgisayar kapanmaz.","DEMO • Simulated actions; the computer will not shut down."},
   {"Bilgisayarını istediğin zamanda kapat veya yeniden başlat.","Shut down or restart your computer at your chosen time."},
   {"01  ZAMAN SEÇ","01  CHOOSE TIME"},
   {"Süre sonra","After a delay"},
   {"Tarih ve saatte","At a date and time"},
   {"Her gün","Every day"},
   {"Saat","Hours"},
   {"Dakika","Minutes"},
   {"Saniye","Seconds"},
   {"Tarih/saat bilgisayarın yerel saatine göredir. En az 30 saniye ileri seç.","Uses the computer's local time. Choose at least 30 seconds ahead."},
   {"02  İŞLEM SEÇ","02  CHOOSE ACTION"},
   {"Bilgisayarı kapat","Shut down computer"},
   {"Yeniden başlat","Restart"},
   {"ÖNCE ÇALIŞMALARINI KAYDET","SAVE YOUR WORK FIRST"},
   {"Süre dolunca açık uygulamalar zorla kapanabilir. Kaydedilmemiş dosyalar\nkaybolabilir. Pencereyi kapatmak, Windows'a verilmiş planı iptal etmez.","Open applications may be forced to close. Unsaved files may be lost.\nClosing this window does not cancel a plan already sent to Windows."},
   {"Veri kaybı riskini anladım; işlemi onaylıyorum.","I understand the risk of data loss and confirm this action."},
   {"DEMO PLANLA","SCHEDULE DEMO"},
   {"Planı başlat","Start plan"},
   {"Bekleyen işlemi iptal et","Cancel pending action"},
   {"Güvenli demo. Gerçek kapatma / yeniden başlatma yapılmaz.","Safe demo. No real shutdown or restart will occur."},
   {"Hazır. Henüz bu oturumda bir plan oluşturulmadı.","Ready. No plan has been created in this session."},
   {"03  KAYITLI GÜNLÜK ALARM","03  SAVED DAILY ALARM"},
   {"Günlük plan okunuyor...","Reading daily plan..."},
   {"Günlük planı kaldır","Remove daily plan"},
   {"Günlük planı kaydet","Save daily plan"},
   {"Her gün tekrarlar. Windows uyarısı hedef saatten 1 dakika önce başlar.","Repeats daily. Windows starts its warning 1 minute before the target."},
   {"Her gün ","Every day "},
   {" • İlk hedef: "," • First target: "},
   {"Hedef: ","Target: "},
   {"Kalan: ","Remaining: "},
   {" gün "," days "},
   {"Hedef zamana ulaşıldı.","Target time reached."},
   {"Windows planını iptal etmek için yukarıdaki düğmeyi kullan.","Use the button above to cancel the Windows plan."},
   {"Önce çalışmalarını kaydettiğini onayla.","First confirm that you have saved your work."},
   {"DEMO: Gerçek komut çalışmayacak.\n\n","DEMO: No real command will run.\n\n"},
   {" tarihinde bilgisayar "," — the computer will "},
   {"kapatılacak.","shut down."},
   {"yeniden başlatılacak.","restart."},
   {"\n\nKaydedilmemiş dosyalar kaybolabilir. Onaylıyor musun?","\n\nUnsaved files may be lost. Do you confirm?"},
   {"Planı onayla","Confirm plan"},
   {"DEMO PLAN: ","DEMO PLAN: "},
   {"Windows planı oluşturuldu: ","Windows plan created: "},
   {"Plan oluşturulamadı","Could not create plan"},
   {"Windows'taki bekleyen kapatma / yeniden başlatma planı iptal edilsin mi?\n\nBaşka bir uygulamanın oluşturduğu bekleyen plan da iptal edilebilir.","Cancel the pending Windows shutdown / restart plan?\n\nThis may also cancel a pending plan created by another application."},
   {"DEMO plan iptal edildi.","DEMO plan cancelled."},
   {"Bekleyen işlem iptal edildi. Kayıtlı günlük alarm varsa silinmedi.","Pending action cancelled. Any saved daily alarm was not removed."},
   {"İptal edilemedi","Could not cancel"},
   {"Windows'ta zaten bekleyen bir plan var. Önce 'Bekleyen işlemi iptal et' düğmesini kullan.","Windows already has a pending plan. Use 'Cancel pending action' first."},
   {"Windows'ta iptal edilecek bekleyen plan bulunamadı.","No pending Windows plan was found to cancel."},
   {"Windows işlemi tamamlamadı (kod ","Windows did not complete the action (code "},
   {"Windows yanıtı","Windows response"},
   {"Kayıtlı günlük alarm yok.\nHer gün seçeneğiyle yeni bir alarm oluşturabilirsin.","No daily alarm saved.\nChoose Every day to create one."},
   {"DEVRE DIŞI: ","DISABLED: "},
   {"Kapat","Shut down"},
   {"\nUygulama kapalıyken de geçerli. Oturum açık olmalı.","\nWorks with the app closed. You must be signed in."},
   {"Günlük alarm okunamadı: ","Could not read daily alarm: "},
   {"Önce veri kaybı riskini onayla.","First acknowledge the risk of data loss."},
   {"DEMO: Windows görevi kaydedilmeyecek.\n\n","DEMO: No Windows task will be saved.\n\n"},
   {"\nİlk hedef: ","\nFirst target: "},
   {"\n\nHedeften 1 dakika önce Windows geri sayımı başlar.\nBilgisayar açık ve kullanıcı oturumu açık olmalı. Kaçırılan çalışma sonradan yapılmaz.\n\nBu günlük plan her gün tekrarlanır; açık dosyalar kaybolabilir. Var olan günlük alarmın yerini alır. Onaylıyor musun?","\n\nThe Windows countdown starts 1 minute before the target.\nThe computer must be on and you must be signed in. Missed runs are not replayed.\n\nThis plan repeats every day; unsaved files may be lost. It replaces the existing daily alarm. Do you confirm?"},
   {"Her gün tekrarlayan planı onayla","Confirm repeating daily plan"},
   {"Günlük alarm kaydı doğrulanamadı.","Could not verify the saved daily alarm."},
   {"Günlük alarm kaydedildi. Her gün ","Daily alarm saved. Every day "},
   {"; ilk hedef ","; first target "},
   {"Günlük plan kaydedilemedi","Could not save daily plan"},
   {"Kayıtlı günlük alarm kalıcı olarak kaldırılsın mı?\n\nBaşlamış bir Windows geri sayımı varsa bu düğme onu iptal etmez. Bunun için ayrıca 'Bekleyen işlemi iptal et' düğmesini kullan.","Permanently remove the saved daily alarm?\n\nThis does not cancel an already started Windows countdown. Use 'Cancel pending action' separately for that."},
   {"Günlük alarmı kaldır","Remove daily alarm"},
   {"Günlük alarm silinemedi.","Could not delete the daily alarm."},
   {"Günlük alarm kaldırıldı. Başlamış geri sayım varsa ayrıca iptal et.","Daily alarm removed. Cancel any already started countdown separately."},
   {"Günlük alarm kaldırılamadı","Could not remove daily alarm"},
   {"Demo penceresi kapatılacak. Gerçek Windows planı yok.","The demo window will close. There is no real Windows plan."},
   {"Pencereyi kapatmak Windows'taki planı iptal etmez. Bekleyen işlemi iptal etmek için önce 'Bekleyen işlemi iptal et' düğmesini kullanın.\n\nPencere kapatılsın mı?","Closing this window does not cancel the Windows plan. To cancel it, use 'Cancel pending action' first.\n\nClose this window?"},
   {"Plan devam ediyor","Plan remains active"},
   {"Windows yanıtı zaman aşımına uğradı. İşlem durumunu kontrol edin.","Windows response timed out. Check the action's state."},
   {"DEMO: Windows güç komutu çalıştırılmadı.","DEMO: No Windows power command was executed."},
   {"Tarih en az 30 saniye ileride, en fazla 365 gun sonra olmali.","Choose a date at least 30 seconds and at most 365 days ahead."},
   {"Gecersiz islem.","Invalid action."},
   {"Geçersiz işlem.","Invalid action."},
   {"Gecersiz sure.","Invalid delay."},
   {"Sure degerleri gecersiz.","Invalid duration values."},
   {"Sure en az 30 saniye, en fazla 365 gun olabilir.","The delay must be at least 30 seconds and at most 365 days."},
   {"Günlük saat 00:00–23:59 arasında olmalı.","The daily time must be between 00:00 and 23:59."},
   {"Günlük görev bilgileri eksik.","Daily task details are missing."},
   {"Bu görev uygulamaya ait değil.","This task is not owned by this application."},
   {"Günlük görev başka kullanıcıya ait.","The daily task belongs to another user."},
   {"Günlük görev eylemi beklenenle uyuşmuyor.","The daily task action does not match the expected action."},
   {"Günlük görev tetikleyicisi uyuşmuyor.","The daily task trigger does not match."},
   {"Günlük görev tetikleyicisi desteklenmiyor.","The daily task trigger is not supported."},
   {"Günlük görev tekrarı desteklenmiyor.","The daily task recurrence is not supported."},
   {"Günlük görev saati değiştirilmiş.","The daily task time was modified."},
   {"Günlük görev güvenlik ayarları değiştirilmiş.","The daily task safety settings were modified."},
   {"Günlük plan Windows'a kaydedilemedi.","Could not save the daily plan to Windows."},
   {"Windows günlük plan doğrulaması başarısız.","Windows daily plan verification failed."},
   {"Günlük plan silinemedi.","Could not delete the daily plan."},
   {"Günlük görev okunamadı: ","Could not read the daily task: "},
   {"Dil ayarı kaydedilemedi: ","Could not save language preference: "},
  };
  public static string Source(string text){if(english.ContainsKey(text))return text;foreach(var item in english)if(item.Value==text)return item.Key;return text;}
  public static bool IsResource(string text){return english.ContainsKey(text)||english.ContainsValue(text);}
  public static string Get(string key,bool turkish){string value;return !turkish&&english.TryGetValue(key,out value)?value:key;}
  public static string Error(string message,bool turkish){
   const string wrapper="Günlük görev okunamadı: ";
   if(message!=null&&message.StartsWith(wrapper,StringComparison.Ordinal))return Get(wrapper,turkish)+message.Substring(wrapper.Length);
   return Get(message??"",turkish);
  }
 }
 public sealed class MainForm:Form {
  readonly System.Collections.Generic.Dictionary<Control,string> localizedControls=new System.Collections.Generic.Dictionary<Control,string>();
  ComboBox language; bool turkish,localizing; string languageFile;
  Func<string> statusText,dailyInfoText;
  string T(string key){return UiText.Get(key,turkish);}
  string Error(string message){return UiText.Error(message,turkish);}
  void SetStatus(Func<string> render){statusText=render;status.Text=render();}
  void SetDailyInfo(Func<string> render){dailyInfoText=render;dailyInfo.Text=render();}
  void Remember(Control parent){foreach(Control c in parent.Controls){if(c!=action && c!=language && c!=status && c!=countdown && c!=preview && c!=dailyInfo && c!=clock && !(c is DateTimePicker) && UiText.IsResource(c.Text))localizedControls[c]=UiText.Source(c.Text);Remember(c);}}
  void ApplyLanguage(){
   localizing=true;
   foreach(var item in localizedControls)item.Key.Text=T(item.Value);
   Text=T("PC Zamanlayıcı")+(demo?T(" — GÜVENLİ DEMO"):"");
   int selected=action.SelectedIndex;action.Items.Clear();action.Items.AddRange(new object[]{T("Bilgisayarı kapat"),T("Yeniden başlat")});action.SelectedIndex=selected;
   start.Text=demo?T("DEMO PLANLA"):daily.Checked?T("Günlük planı kaydet"):T("Planı başlat");
   timeNote.Text=daily.Checked?T("Her gün tekrarlar. Windows uyarısı hedef saatten 1 dakika önce başlar."):T("Tarih/saat bilgisayarın yerel saatine göredir. En az 30 saniye ileri seç.");
   localizing=false;if(statusText!=null)status.Text=statusText();if(dailyInfoText!=null)dailyInfo.Text=dailyInfoText();UpdatePreview();UpdateClock();
  }
  readonly IBackend backend;readonly bool demo;readonly IDailyScheduler recurring;
  readonly Color navy=Color.FromArgb(21,37,62),blue=Color.FromArgb(38,99,235),muted=Color.FromArgb(88,104,125);
  RadioButton after,at,daily;
  NumericUpDown hours,minutes,seconds;
  DateTimePicker date,dailyTime;
  ComboBox action;
  Label preview,status,countdown,clock,timeNote,dailyInfo;
  DailyState dailyState;
  CheckBox saved;
  Button start,cancel,removeDaily;
  System.Windows.Forms.Timer timer;
  DateTime? plannedAt;
  bool smoke;readonly string smokeOutputDirectory;
  public MainForm(IBackend backend,bool demo,bool smoke,IDailyScheduler recurring,string smokeOutputDirectory=null){
   this.backend=backend;this.demo=demo;this.smoke=smoke;this.recurring=recurring;
   this.smokeOutputDirectory=smokeOutputDirectory??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"PCZamanlayici");
   languageFile=Path.Combine(smokeOutputDirectory==null?Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData):smokeOutputDirectory,"PCZamanlayici","language.txt");
   try{turkish=File.Exists(languageFile)&&File.ReadAllText(languageFile).Trim()=="tr";}catch(IOException){}catch(UnauthorizedAccessException){}
   Text=T("PC Zamanlayıcı")+(demo?T(" — GÜVENLİ DEMO"):"");
   Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);
   ClientSize=new Size(720,838);MinimumSize=new Size(736,877);MaximumSize=new Size(736,877);
   StartPosition=FormStartPosition.CenterScreen;BackColor=Color.FromArgb(240,244,249);
   Font=new Font("Segoe UI",10);AutoScaleMode=AutoScaleMode.Dpi;MaximizeBox=false;
   Build();Remember(this);RefreshDaily();UpdatePreview();
   timer=new System.Windows.Forms.Timer();timer.Interval=1000;timer.Tick+=delegate{UpdateClock();};timer.Start();
   FormClosed+=delegate{timer.Dispose();};
   FormClosing+=delegate(object sender,FormClosingEventArgs e){
    if(plannedAt.HasValue && !smoke && DateTime.Now<plannedAt.Value){
     string text=demo?T("Demo penceresi kapatılacak. Gerçek Windows planı yok."):T("Pencereyi kapatmak Windows'taki planı iptal etmez. Bekleyen işlemi iptal etmek için önce 'Bekleyen işlemi iptal et' düğmesini kullanın.\n\nPencere kapatılsın mı?");
     if(MessageBox.Show(this,text,T("Plan devam ediyor"),MessageBoxButtons.YesNo,MessageBoxIcon.Information,MessageBoxDefaultButton.Button2)!=DialogResult.Yes)e.Cancel=true;
    }
   };
   if(smoke) Shown+=delegate{BeginInvoke(new Action(RunSmoke));};
  }
  Label L(Control parent,string text,int x,int y,int w,int h,float size,Color color,bool bold){
   var l=new Label();l.Text=text;l.SetBounds(x,y,w,h);l.ForeColor=color;l.Font=new Font("Segoe UI",size,bold?FontStyle.Bold:FontStyle.Regular);parent.Controls.Add(l);return l;
  }
  Panel Card(int y,int height){var p=new Panel();p.SetBounds(24,y,672,height);p.BackColor=Color.White;Controls.Add(p);return p;}
  NumericUpDown N(Panel p,int x,int value,int max){var n=new NumericUpDown();n.Minimum=0;n.Maximum=max;n.Value=value;n.SetBounds(x,75,160,32);n.Font=new Font("Segoe UI",13);n.ValueChanged+=delegate{UpdatePreview();};p.Controls.Add(n);return n;}
  Button B(Control parent,string text,int x,int y,int w,Color background,Color foreground){var b=new Button();b.Text=text;b.SetBounds(x,y,w,42);b.FlatStyle=FlatStyle.Flat;b.FlatAppearance.BorderSize=0;b.BackColor=background;b.ForeColor=foreground;b.Font=new Font("Segoe UI",11,FontStyle.Bold);b.Cursor=Cursors.Hand;parent.Controls.Add(b);return b;}
  void Build(){
   var header=new Panel();header.SetBounds(0,0,720,104);header.BackColor=navy;Controls.Add(header);
   L(header,T("PC Zamanlayıcı"),24,18,465,37,21,Color.White,true);
   L(header,demo?T("DEMO • İşlemler simüle edilir; bilgisayar kapanmaz."):T("Bilgisayarını istediğin zamanda kapat veya yeniden başlat."),26,65,650,25,10,Color.FromArgb(187,205,229),false);
   language=new ComboBox();language.DropDownStyle=ComboBoxStyle.DropDownList;language.Items.AddRange(new object[]{"English","Türkçe"});language.SetBounds(505,22,185,30);language.AccessibleName="Language / Dil";language.SelectedIndex=turkish?1:0;header.Controls.Add(language);
   language.SelectedIndexChanged+=delegate{
    turkish=language.SelectedIndex==1;ApplyLanguage();
    try{Directory.CreateDirectory(Path.GetDirectoryName(languageFile));File.WriteAllText(languageFile,turkish?"tr":"en");}
    catch(Exception e){SetStatus(delegate{return T("Dil ayarı kaydedilemedi: ")+Error(e.Message);});}
   };
   var timing=Card(120,209);
   L(timing,T("01  ZAMAN SEÇ"),18,12,620,24,10,muted,true);
   after=new RadioButton();after.Text=T("Süre sonra");after.Checked=true;after.SetBounds(18,41,194,28);timing.Controls.Add(after);
   at=new RadioButton();at.Text=T("Tarih ve saatte");at.SetBounds(238,41,225,28);timing.Controls.Add(at);
   daily=new RadioButton();daily.Text=T("Her gün");daily.SetBounds(495,41,155,28);timing.Controls.Add(daily);
   hours=N(timing,18,1,8760);minutes=N(timing,194,0,59);seconds=N(timing,370,0,59);
   L(timing,T("Saat"),18,113,160,22,9,muted,false);L(timing,T("Dakika"),194,113,160,22,9,muted,false);L(timing,T("Saniye"),370,113,160,22,9,muted,false);
   date=new DateTimePicker();date.Format=DateTimePickerFormat.Custom;date.CustomFormat="dd.MM.yyyy   HH:mm:ss";date.Value=DateTime.Now.AddHours(1);date.SetBounds(18,148,512,32);date.Font=new Font("Segoe UI",12);date.Enabled=false;timing.Controls.Add(date);date.ValueChanged+=delegate{UpdatePreview();};
   dailyTime=new DateTimePicker();dailyTime.Format=DateTimePickerFormat.Custom;dailyTime.CustomFormat="HH:mm";dailyTime.ShowUpDown=true;dailyTime.Value=DateTime.Today.AddHours(23).AddMinutes(59);dailyTime.SetBounds(18,148,512,32);dailyTime.Font=new Font("Segoe UI",12);dailyTime.Visible=false;timing.Controls.Add(dailyTime);dailyTime.ValueChanged+=delegate{UpdatePreview();};
   timeNote=L(timing,T("Tarih/saat bilgisayarın yerel saatine göredir. En az 30 saniye ileri seç."),18,184,640,21,9,muted,false);
   after.CheckedChanged+=delegate{ChangeMode();};at.CheckedChanged+=delegate{ChangeMode();};daily.CheckedChanged+=delegate{ChangeMode();};
   var operation=Card(341,113);L(operation,T("02  İŞLEM SEÇ"),18,12,600,24,10,muted,true);
   action=new ComboBox();action.DropDownStyle=ComboBoxStyle.DropDownList;action.Items.AddRange(new object[]{T("Bilgisayarı kapat"),T("Yeniden başlat")});action.SelectedIndex=0;action.SetBounds(18,45,512,34);action.Font=new Font("Segoe UI",12);operation.Controls.Add(action);action.SelectedIndexChanged+=delegate{UpdatePreview();};
   preview=L(operation,"",18,83,640,26,9,muted,false);
   var confirm=Card(466,117);
   L(confirm,T("ÖNCE ÇALIŞMALARINI KAYDET"),18,10,630,23,10,Color.FromArgb(159,75,15),true);
   L(confirm,T("Süre dolunca açık uygulamalar zorla kapanabilir. Kaydedilmemiş dosyalar\nkaybolabilir. Pencereyi kapatmak, Windows'a verilmiş planı iptal etmez."),18,36,635,43,9,muted,false);
   saved=new CheckBox();saved.Text=T("Veri kaybı riskini anladım; işlemi onaylıyorum.");saved.SetBounds(18,83,635,26);saved.CheckedChanged+=delegate{start.Enabled=saved.Checked&&(daily.Checked||!plannedAt.HasValue);};confirm.Controls.Add(saved);
   start=B(this,demo?T("DEMO PLANLA"):T("Planı başlat"),24,595,326,blue,Color.White);start.Enabled=false;start.Click+=delegate{Schedule(false);};
   cancel=B(this,T("Bekleyen işlemi iptal et"),370,595,326,Color.FromArgb(222,230,241),navy);cancel.Click+=delegate{Cancel(false);};
   status=L(this,demo?T("Güvenli demo. Gerçek kapatma / yeniden başlatma yapılmaz."):T("Hazır. Henüz bu oturumda bir plan oluşturulmadı."),25,648,668,28,9,muted,false);
   SetStatus(delegate{return demo?T("Güvenli demo. Gerçek kapatma / yeniden başlatma yapılmaz."):T("Hazır. Henüz bu oturumda bir plan oluşturulmadı.");});
   countdown=L(this,"",25,677,425,30,13,navy,true);
   clock=L(this,"",450,681,246,24,9,muted,false);clock.TextAlign=ContentAlignment.MiddleRight;
   var dailyCard=Card(716,107);L(dailyCard,T("03  KAYITLI GÜNLÜK ALARM"),18,9,620,24,10,muted,true);
   dailyInfo=L(dailyCard,T("Günlük plan okunuyor..."),18,39,425,58,9,muted,false);
   removeDaily=B(dailyCard,T("Günlük planı kaldır"),452,43,200,Color.FromArgb(251,229,226),Color.FromArgb(156,50,44));removeDaily.Enabled=false;removeDaily.Click+=delegate{RemoveDaily(false);};
   UpdateClock();
  }
  void ChangeMode(){hours.Enabled=minutes.Enabled=seconds.Enabled=after.Checked;date.Enabled=at.Checked;date.Visible=!daily.Checked;dailyTime.Visible=daily.Checked;if(daily.Checked&&dailyState!=null)action.SelectedIndex=(int)dailyState.Spec.Action;if(start!=null){start.Text=demo?T("DEMO PLANLA"):daily.Checked?T("Günlük planı kaydet"):T("Planı başlat");start.Enabled=saved.Checked&&(daily.Checked||!plannedAt.HasValue);}timeNote.Text=daily.Checked?T("Her gün tekrarlar. Windows uyarısı hedef saatten 1 dakika önce başlar."):T("Tarih/saat bilgisayarın yerel saatine göredir. En az 30 saniye ileri seç.");UpdatePreview();}
  DailySpec SelectedDaily(){return DailyPlan.Create(DateTime.Now,new TimeSpan(dailyTime.Value.Hour,dailyTime.Value.Minute,0),action.SelectedIndex==0?PowerAction.Shutdown:PowerAction.Restart);}
  int SelectedDelay(DateTime now){return after.Checked?Plan.After(now,(int)hours.Value,(int)minutes.Value,(int)seconds.Value):Plan.At(now,date.Value);}
  void UpdatePreview(){
   if(localizing || preview==null || after==null || action==null)return;
   try{if(daily.Checked){var s=SelectedDaily();preview.Text=T("Her gün ")+s.Time.ToString(@"hh\:mm")+T(" • İlk hedef: ")+s.NextTarget.ToString("dd.MM HH:mm");}else{DateTime now=DateTime.Now;int delay=SelectedDelay(now);preview.Text=T("Hedef: ")+now.AddSeconds(delay).ToString("dd.MM.yyyy HH:mm:ss")+"  •  "+action.Text;}preview.ForeColor=muted;}
   catch(ArgumentException e){preview.Text=Error(e.Message);preview.ForeColor=Color.FromArgb(174,62,47);}
  }
  void UpdateClock(){
   if(clock==null)return;clock.Text=DateTime.Now.ToString("dd.MM.yyyy  HH:mm:ss");
   if(plannedAt.HasValue){
    TimeSpan remain=plannedAt.Value-DateTime.Now;
    countdown.Text=remain.TotalSeconds>0?T("Kalan: ")+(remain.Days>0?remain.Days+T(" gün "):"")+remain.ToString(@"hh\:mm\:ss"):T("Hedef zamana ulaşıldı.");
   }else{countdown.Text=T("Windows planını iptal etmek için yukarıdaki düğmeyi kullan.");if(Math.Abs(countdown.Font.Size-8.5f)>0.1f)countdown.Font=new Font("Segoe UI",8.5f);UpdatePreview();}
  }
  bool Ask(string text,string title){return MessageBox.Show(this,text,title,MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2)==DialogResult.Yes;}
  void Schedule(bool automated){
   if(daily.Checked){ScheduleDaily(automated);return;}
   if(!saved.Checked){SetStatus(delegate{return T("Önce çalışmalarını kaydettiğini onayla.");});return;}
   if(automated && !demo)throw new InvalidOperationException("Automated scheduling is allowed only with DemoBackend.");
   try{
    DateTime now=DateTime.Now;int delay=SelectedDelay(now);DateTime target=now.AddSeconds(delay);
    var selected=action.SelectedIndex==0?PowerAction.Shutdown:PowerAction.Restart;
    string question=(demo?T("DEMO: Gerçek komut çalışmayacak.\n\n"):"")+target.ToString("dd.MM.yyyy HH:mm:ss")+T(" tarihinde bilgisayar ")+(selected==PowerAction.Shutdown?T("kapatılacak."):T("yeniden başlatılacak."))+T("\n\nKaydedilmemiş dosyalar kaybolabilir. Onaylıyor musun?");
    if(!automated && !Ask(question,T("Planı onayla")))return;
    // Recalculate absolute targets after the user has spent time confirming.
    delay=SelectedDelay(DateTime.Now);target=DateTime.Now.AddSeconds(delay);
    var result=backend.Run(Plan.Arguments(selected,delay));
    if(result.Code!=0){ShowFailure(result,false,automated);return;}
    plannedAt=target;start.Enabled=false;saved.Checked=false;
    SetStatus(delegate{return (demo?T("DEMO PLAN: "):T("Windows planı oluşturuldu: "))+target.ToString("dd.MM.yyyy HH:mm:ss")+" — "+T(selected==PowerAction.Shutdown?"Bilgisayarı kapat":"Yeniden başlat");});
    status.ForeColor=Color.FromArgb(22,126,89);countdown.Font=new Font("Segoe UI",13,FontStyle.Bold);UpdateClock();
   }catch(Exception e){SetStatus(delegate{return Error(e.Message);});status.ForeColor=Color.FromArgb(174,62,47);if(!automated)MessageBox.Show(this,Error(e.Message),T("Plan oluşturulamadı"),MessageBoxButtons.OK,MessageBoxIcon.Warning);}
  }
  void Cancel(bool automated){
   if(automated && !demo)throw new InvalidOperationException("Automated cancellation is allowed only with DemoBackend.");
   if(!automated && !Ask((demo?T("DEMO: Gerçek komut çalışmayacak.\n\n"):"")+T("Windows'taki bekleyen kapatma / yeniden başlatma planı iptal edilsin mi?\n\nBaşka bir uygulamanın oluşturduğu bekleyen plan da iptal edilebilir."),T("Bekleyen işlemi iptal et")))return;
   try{var result=backend.Run("/a");if(result.Code!=0){ShowFailure(result,true,automated);return;}plannedAt=null;start.Enabled=saved.Checked;SetStatus(delegate{return demo?T("DEMO plan iptal edildi."):T("Bekleyen işlem iptal edildi. Kayıtlı günlük alarm varsa silinmedi.");});status.ForeColor=muted;UpdateClock();}
   catch(Exception e){SetStatus(delegate{return Error(e.Message);});if(!automated)MessageBox.Show(this,Error(e.Message),T("İptal edilemedi"),MessageBoxButtons.OK,MessageBoxIcon.Warning);}
  }
  void ShowFailure(BackendResult result,bool abort,bool automated){
   Func<string> message;
   if(result.Code==1115)message=delegate{return T("Windows'ta zaten bekleyen bir plan var. Önce 'Bekleyen işlemi iptal et' düğmesini kullan.");};
   else if(abort && result.Code==1116){plannedAt=null;start.Enabled=saved.Checked;message=delegate{return T("Windows'ta iptal edilecek bekleyen plan bulunamadı.");};}
   else message=delegate{return T("Windows işlemi tamamlamadı (kod ")+result.Code+"). "+Error(result.Detail);};
   SetStatus(message);status.ForeColor=Color.FromArgb(174,62,47);if(!automated)MessageBox.Show(this,message(),T("Windows yanıtı"),MessageBoxButtons.OK,MessageBoxIcon.Information);
  }
  void RefreshDaily(){
   try{
    dailyState=recurring.Read();removeDaily.Enabled=dailyState!=null;
    if(dailyState==null){SetDailyInfo(delegate{return T("Kayıtlı günlük alarm yok.\nHer gün seçeneğiyle yeni bir alarm oluşturabilirsin.");});return;}
    var s=dailyState.Spec;dailyTime.Value=DateTime.Today.Add(s.Time);
    SetDailyInfo(delegate{return (dailyState.Enabled?T("Her gün "):T("DEVRE DIŞI: "))+s.Time.ToString(@"hh\:mm")+" — "+(s.Action==PowerAction.Shutdown?T("Kapat"):T("Yeniden başlat"))+T("\nUygulama kapalıyken de geçerli. Oturum açık olmalı.");});
   }catch(Exception e){dailyState=null;removeDaily.Enabled=false;SetDailyInfo(delegate{return T("Günlük alarm okunamadı: ")+Error(e.Message);});}
  }
  void ScheduleDaily(bool automated){
   if(automated && (!demo || !(recurring is DemoDailyScheduler)))throw new InvalidOperationException("Automated daily scheduling requires the fake backend.");
   if(!saved.Checked){SetStatus(delegate{return T("Önce veri kaybı riskini onayla.");});return;}
   try{
    var spec=SelectedDaily();
    string question=(demo?T("DEMO: Windows görevi kaydedilmeyecek.\n\n"):"")+T("Her gün ")+spec.Time.ToString(@"hh\:mm")+" — "+(spec.Action==PowerAction.Shutdown?T("Bilgisayarı kapat"):T("Yeniden başlat"))+T("\nİlk hedef: ")+spec.NextTarget.ToString("dd.MM.yyyy HH:mm")+T("\n\nHedeften 1 dakika önce Windows geri sayımı başlar.\nBilgisayar açık ve kullanıcı oturumu açık olmalı. Kaçırılan çalışma sonradan yapılmaz.\n\nBu günlük plan her gün tekrarlanır; açık dosyalar kaybolabilir. Var olan günlük alarmın yerini alır. Onaylıyor musun?");
    if(!automated && !Ask(question,T("Her gün tekrarlayan planı onayla")))return;
    spec=SelectedDaily();recurring.Save(spec);RefreshDaily();
    if(dailyState==null || dailyState.Spec.Time!=spec.Time || dailyState.Spec.Action!=spec.Action)throw new InvalidOperationException("Günlük alarm kaydı doğrulanamadı.");
    saved.Checked=false;SetStatus(delegate{return (demo?"DEMO: ":"")+T("Günlük alarm kaydedildi. Her gün ")+spec.Time.ToString(@"hh\:mm")+T("; ilk hedef ")+spec.NextTarget.ToString("dd.MM HH:mm")+".";});status.ForeColor=Color.FromArgb(22,126,89);
   }catch(Exception e){SetStatus(delegate{return Error(e.Message);});status.ForeColor=Color.FromArgb(174,62,47);if(!automated)MessageBox.Show(this,Error(e.Message),T("Günlük plan kaydedilemedi"),MessageBoxButtons.OK,MessageBoxIcon.Warning);}
  }
  void RemoveDaily(bool automated){
   if(automated && (!demo || !(recurring is DemoDailyScheduler)))throw new InvalidOperationException("Automated daily deletion requires the fake backend.");
   string question=T("Kayıtlı günlük alarm kalıcı olarak kaldırılsın mı?\n\nBaşlamış bir Windows geri sayımı varsa bu düğme onu iptal etmez. Bunun için ayrıca 'Bekleyen işlemi iptal et' düğmesini kullan.");
   if(!automated && !Ask(question,T("Günlük alarmı kaldır")))return;
   try{recurring.Remove();RefreshDaily();if(dailyState!=null)throw new InvalidOperationException("Günlük alarm silinemedi.");SetStatus(delegate{return T("Günlük alarm kaldırıldı. Başlamış geri sayım varsa ayrıca iptal et.");});status.ForeColor=muted;}
   catch(Exception e){SetStatus(delegate{return Error(e.Message);});if(!automated)MessageBox.Show(this,Error(e.Message),T("Günlük alarm kaldırılamadı"),MessageBoxButtons.OK,MessageBoxIcon.Warning);}
  }
  void RunSmoke(){
   try{
    Directory.CreateDirectory(smokeOutputDirectory);
    var fake=backend as DemoBackend;if(fake==null)throw new Exception("Unsafe test backend");
    if(start.Enabled)throw new Exception("Confirmation required by default");
    saved.Checked=true;Schedule(true);if(fake.Calls!=1 || !fake.LastArguments.StartsWith("/s /t "))throw new Exception("Relative shutdown flow failed");
    var activeTarget=plannedAt;string arguments=fake.LastArguments;
    language.SelectedIndex=1;
    if(fake.Calls!=1 || plannedAt!=activeTarget || fake.LastArguments!=arguments || !countdown.Text.StartsWith("Kalan: ") || !preview.Text.StartsWith("Hedef: "))throw new Exception("Turkish switch altered active plan or labels");
    language.SelectedIndex=0;
    if(fake.Calls!=1 || plannedAt!=activeTarget || !countdown.Text.StartsWith("Remaining: "))throw new Exception("English switch altered active plan or countdown");
    Cancel(true);if(fake.LastArguments!="/a" || plannedAt.HasValue)throw new Exception("Cancellation flow failed");
    at.Checked=true;date.Value=DateTime.Now.AddHours(2);action.SelectedIndex=1;saved.Checked=true;Schedule(true);
    if(fake.Calls!=3 || !fake.LastArguments.StartsWith("/r /t "))throw new Exception("Absolute restart flow failed");
    Cancel(true);if(fake.Calls!=4)throw new Exception("Second cancellation failed");
    var dailyFake=recurring as DemoDailyScheduler;if(dailyFake==null)throw new Exception("Unsafe daily test backend");
    daily.Checked=true;dailyTime.Value=DateTime.Today.AddHours(23).AddMinutes(59);action.SelectedIndex=0;saved.Checked=true;Schedule(true);
    if(dailyFake.Saves!=1 || dailyFake.Read()==null || !dailyInfo.Text.Contains("23:59"))throw new Exception("Daily alarm save flow failed");
    action.SelectedIndex=1;saved.Checked=true;Schedule(true);if(dailyFake.Saves!=2 || dailyFake.Read().Spec.Action!=PowerAction.Restart)throw new Exception("Daily alarm update failed");
    string taskXml=dailyFake.SavedXml;int selectedAction=action.SelectedIndex;DateTime selectedTime=dailyTime.Value;
    language.SelectedIndex=1;
    if(!dailyInfo.Text.StartsWith("Her gün ") || !preview.Text.Contains("İlk hedef:"))throw new Exception("Turkish daily labels failed");
    language.SelectedIndex=0;
    if(!dailyInfo.Text.StartsWith("Every day ") || !preview.Text.Contains("First target:") || taskXml!=dailyFake.SavedXml || dailyFake.Saves!=2 || dailyFake.Removes!=0 || fake.Calls!=4 || action.SelectedIndex!=selectedAction || dailyTime.Value!=selectedTime)throw new Exception("Language switch changed daily task or selection");
    var reopened=new MainForm(new DemoBackend(),true,false,dailyFake,smokeOutputDirectory);if(!reopened.dailyInfo.Text.Contains("23:59") || reopened.dailyState.Spec.Action!=PowerAction.Restart)throw new Exception("Daily restore after reopen failed");reopened.Dispose();
    RemoveDaily(true);if(dailyFake.Read()!=null || removeDaily.Enabled)throw new Exception("Daily alarm removal failed");
    if(fake.Calls!=4)throw new Exception("Daily alarm invoked a power command");
    action.SelectedIndex=0;
    using(var image=new Bitmap(Width,Height)){DrawToBitmap(image,new Rectangle(0,0,Width,Height));image.Save(Path.Combine(smokeOutputDirectory,"ui-preview-v2.png"),System.Drawing.Imaging.ImageFormat.Png);}
    File.WriteAllText(Path.Combine(smokeOutputDirectory,"smoke-result.txt"),"PASS: relative/absolute plans, restart, cancellation; daily save/update, restore after reopen and removal; English/Turkish switches preserve active plans and task XML. Fake backends only; ZERO registered tasks or Windows power commands.",Encoding.UTF8);
    Close();
   }catch(Exception e){
    Environment.ExitCode=1;
    try{File.WriteAllText(Path.Combine(smokeOutputDirectory,"smoke-result.txt"),"FAIL: "+e,Encoding.UTF8);}
    catch(Exception){/* The nonzero exit code remains authoritative if reporting also fails. */}
    Close();
   }
  }
 }
 public static class Program {
  [STAThread] public static void Main(string[] args){
   bool smoke=Array.IndexOf(args,"--smoke")>=0;bool demo=smoke || Array.IndexOf(args,"--demo")>=0;
   Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
   IBackend backend=demo?(IBackend)new DemoBackend():new WindowsBackend();
   IDailyScheduler recurring=demo?(IDailyScheduler)new DemoDailyScheduler():new WindowsDailyScheduler();
   string smokeOutput=null;
   if(smoke)foreach(string arg in args)if(arg.StartsWith("--smoke-output=",StringComparison.Ordinal))smokeOutput=arg.Substring("--smoke-output=".Length);
   Application.Run(new MainForm(backend,demo,smoke,recurring,smokeOutput));
  }
 }
}
