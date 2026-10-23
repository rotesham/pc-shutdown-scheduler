[English](README.md) · **Türkçe**

# PC Zamanlayıcı ⏱️

[![Windows build](https://github.com/rotesham/pc-shutdown-scheduler/actions/workflows/build.yml/badge.svg)](https://github.com/rotesham/pc-shutdown-scheduler/actions/workflows/build.yml)
![C#](https://img.shields.io/badge/C%23-.NET_Framework_4.8-512BD4)
[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**Windows için English / Türkçe kapatma ve yeniden başlatma zamanlayıcısı.** Tek seferlik planlar veya her gün tekrarlayan bir alarm oluşturun.

[⬇️ EXE sürümünü indir](https://github.com/rotesham/pc-shutdown-scheduler/releases/latest) · [Kullanım](#kullanım) · [Kaynak koddan derleme](#derleme-ve-test) · [Güvenlik](SECURITY.md)

![Güvenli demo ekranı](docs/screenshot.png)

## Özellikler

- **Süre sonra:** Saat, dakika ve saniye ile tek seferlik plan.
- **Tarih ve saatte:** Belirli yerel tarih/saat hedefi.
- **Her gün:** Seçilen saatte kalıcı günlük alarm; kapatma veya yeniden başlatma.
- **İki ayrı iptal:** Bekleyen Windows geri sayımını iptal etme ve günlük alarmı kalıcı kaldırma.
- **Onaylı işlemler:** Uygulamayı açmak veya saat seçmek kendiliğinden güç işlemi başlatmaz.
- Kurulum ve internet bağlantısı gerektirmez. Günlük kayıt uygulama kapatılsa da Windows Görev Zamanlayıcı'da saklanır.

## Dil seçimi

Varsayılan dil İngilizcedir. Uygulamadaki **English / Türkçe** seçicisinden dili değiştirebilirsiniz. Windows'un yerel dosya/seçim ve UAC düğmeleri sistem dilinde kalabilir.

## Kullanım

1. Releases bölümündeki ZIP'i indirip çıkarın; `PCZamanlayici.exe` dosyasını açın.
2. **Süre sonra**, **Tarih ve saatte** veya **Her gün** seçin.
3. Zamanı ve **Bilgisayarı kapat / Yeniden başlat** işlemini belirleyin.
4. Çalışmalarınızı kaydedin, risk onayını işaretleyin ve düğmeye basın.
5. Son onay penceresinde hedefi kontrol edin.

**Örnek:** `Her gün → 23:59 → Bilgisayarı kapat → Günlük planı kaydet`.

> **Veri kaybı uyarısı:** Süre dolunca açık uygulamalar zorla kapanabilir. Kaydedilmemiş dosyalar kaybolabilir. Pencereyi kapatmak planı iptal etmez.

### Günlük alarmın sınırları

- Kullanıcı başına **bir** günlük alarm vardır; yeni kayıt eskisini değiştirir.
- Bilgisayar ve ilgili Windows kullanıcı oturumu açık olmalıdır. Kapalı PC'yi açmaz, uyuyan PC'yi uyandırmaz.
- Hedeften **60 saniye önce** Windows geri sayımı başlar. `23:59` hedefi için görev `23:58`'de tetiklenir.
- Kaydederken hedefe bir dakikadan az kalmışsa ilk çalışma ertesi güne alınır; ilk hedef ekranda gösterilir.
- Kaçırılmış bir çalışma daha sonra oturum açıldığında çalıştırılmaz.
- **Bekleyen işlemi iptal et**, günlük kaydı silmez. **Günlük planı kaldır**, başlamış Windows geri sayımını iptal etmez. Gerekirse ikisini de kullanın.
- Başka programların oluşturduğu bekleyen Windows güç planı da iptal düğmesinden etkilenebilir.
- Saat dilimi/saat değişikliklerinden sonra günlük planınızı yeniden kontrol edin. Windows zamanlayıcısı gerçek zamanlı yürütme garantisi vermez.

## Derleme ve test

Windows ve .NET Framework C# derleyicisi gerekir; NuGet bağımlılığı yoktur. Windows 11 üzerinde yerel olarak test edilmiştir.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

Çıktı: `build\PCZamanlayici.exe`.

Varsayılan testler saf mantığı ve **sahte arka uçlu** arayüzü kullanır; gerçek kapatma/yeniden başlatma yapmaz.

`-RunIntegration` saf test değildir: Görev Zamanlayıcı'da ayrı bir geçici görev oluşturur, okur, değiştirir ve siler. Önce kaynak kodunu inceleyin; `build.ps1` bu test için güvenilir `tests/SchedulerNoop.cs` kaynağından yeni, hiçbir iş yapmayan `SchedulerNoop.exe` derler. Eski veya kaynağı bilinmeyen bir EXE ile entegrasyon testini çalıştırmayın. Gerçek güç görevi kullanılmaz.

```powershell
# İsteğe bağlı: Görev Zamanlayıcı'da yalnız hiçbir iş yapmayan test EXE'siyle kayıt testi.
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1 -RunIntegration

# Güvenli görsel demo
.\build\PCZamanlayici.exe --demo
```

## Proje düzeni

```text
src/       Uygulama, zaman hesabı ve Windows görev arka ucu
 tests/    Birim ve açıkça seçilen entegrasyon testleri
 assets/   Uygulama ikonu
 docs/     Ekran görüntüsü
build.ps1  Çevrimdışı derleme + güvenli testler
```

## English summary

A Windows shutdown/restart scheduler with English/Turkish UI with relative delays, absolute local dates and an opt-in persistent daily alarm. Default tests use fake OS backends. Daily schedules require an existing interactive Windows session and do not wake the computer. **Unsigned software, provided as is; save your work before scheduling power operations.**

## Lisans

[MIT](LICENSE) · rotesham. İnceleme ve testler kusursuzluk veya mutlak güvenlik garantisi değildir.
