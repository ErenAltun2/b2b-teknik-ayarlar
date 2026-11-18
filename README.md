# B2B Store – Teknik Ayarlar Modülü

B2B Store platformundaki **Teknik Ayarlar** ekranının yeniden tasarlanıp geliştirilmiş hâli.
Amaç, teknik bilgisi sınırlı bir müşterinin bile bayi portalının ayarlarını kolayca anlayıp yönetebilmesi.

Mevcut `DealerSettings` tablosunun yapısı, parametrelerin anlamları ve veri karşılıkları **korunmuştur**; veritabanı şemasında değişiklik yapılmamıştır.

![Vue 3](https://img.shields.io/badge/Vue-3-42b883?logo=vuedotjs&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-5-3178c6?logo=typescript&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-11-512bd4?logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-cc2927?logo=microsoftsqlserver&logoColor=white)
![Tests](https://img.shields.io/badge/tests-8%20passed-1e8e4f)

> 🔗 **Canlı demo:** _(eklenecek)_ · 🎬 **Ekran kaydı:** _(eklenecek)_

![Yeni Teknik Ayarlar ekranı](docs/screenshots/yeni-ekran.png)

---

## İçindekiler

- [Özellikler](#özellikler)
- [Eski ekrana göre farklar](#eski-ekrana-göre-farklar)
- [Mimari](#mimari)
- [Kurulum ve çalıştırma](#kurulum-ve-çalıştırma)
- [API](#api)
- [Bitwise parametreler nasıl kaydediliyor?](#bitwise-parametreler-nasıl-kaydediliyor)
- [Testler](#testler)
- [Proje yapısı](#proje-yapısı)
- [Bilinen sınırlamalar](#bilinen-sınırlamalar)

---

## Özellikler

- **93 parametre, 12 anlaşılır kategori** – Genel, Görünüm & Tasarım, Menü, Ürün Listeleme & Stok, Ürün Detay, Arama & Filtre, Sepet & Sipariş, Ödeme & Cari, Fiyat & Döviz, Başvuru Formu, Entegrasyon, Admin
- **Arama** – ayar adı, açıklama, seçenek ve kolon adında; **"sadece değişenler"** filtresi
- **Doğru bileşen, doğru ayar** – açık/kapalı anahtarlar, açılır listeler, çoklu seçim (bitwise), sayı, metin, renk
- **Açıklama metinleri** ve **ilişkili ayar uyarıları** ("Bu ayar şuna bağlı → ilgili ayara git")
- **Kaydetmeden önce değişiklik özeti** – eski → yeni değer; bitwise ayarlarda eklenen / kaldırılan seçenekler
- **Güvenli kayıt** – sadece değişen ayarlar, bitwise'da sadece değişen bitler; transaction + satır kilidi
- **Sunucu tarafı doğrulama** – tip, aralık, seçenek listesi, kolona sığmayan bitler; ayar bazında hata mesajları (RFC 7807)
- Değişiklik işaretleri, tek tek geri alma, `Ctrl + S`, mobil uyumlu yerleşim

![Kaydetmeden önce değişiklik özeti](docs/screenshots/degisiklik-ozeti.jpg)

## Eski ekrana göre farklar

| Eski ekran | Yeni ekran |
|---|---|
| ~95 ayar tek sayfada | 12 kategori + arama |
| Açıklama yok, ters mantıklı değerler ("1 = Hayır") | Anlaşılır başlıklar, yardım metinleri, açık/kapalı anahtarlar |
| Neyin değiştiği görünmüyor | Değişen ayarlar işaretli, geri alınabilir, kayıttan önce özet |
| Bütün form gönderiliyor | Sadece değişen ayarlar / bitler gönderiliyor |
| Veritabanı kolonuna sığmayan seçenekler seçilebiliyor | Bu seçenekler devre dışı ve uyarılı; API de reddediyor |

## Mimari

```
frontend/                         Vue 3 + TypeScript + Vite
   │  fetch /api/settings
   ▼
backend/B2BStore.TechnicalSettings.Api   .NET 11 Web API
   ├─ Controllers/    HTTP katmanı (endpoint'ler)
   ├─ Services/       iş kuralları, doğrulama, bitwise birleştirme
   ├─ Repositories/   SQL (Dapper), transaction, satır kilitleme
   ├─ Models/         DTO'lar ve hata sınıfları
   └─ Catalog/        settings-catalog.json – 93 ayarın tanımı
   ▼
SQL Server 2022 – dbo.DealerSettings
```

Ekran **katalog tabanlı** çalışır: `settings-catalog.json` her ayarın grubunu, etiketini, kolonunu, SQL tipini, kontrol tipini, seçeneklerini ve bit değerlerini tanımlar. Hem arayüz hem doğrulama bu katalogdan beslenir; katalogda olmayan bir kolon okunamaz ve güncellenemez.

## Kurulum ve çalıştırma

### Gereksinimler

- [.NET SDK 11](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (SQL Server için) ve SSMS

### 1. Veritabanı

Proje ana klasöründe bir `.env` dosyası oluşturun:

```env
SA_PASSWORD=<güçlü-bir-şifre>
```

SQL Server'ı başlatın:

```powershell
docker compose up -d
```

`store5.bak` dosyasını `backup/` klasörüne koyup SSMS'te geri yükleyin:

```sql
RESTORE DATABASE store5
FROM DISK = N'/var/opt/mssql/backup/store5.bak'
WITH MOVE N'store5'     TO N'/var/opt/mssql/data/store5.mdf',
     MOVE N'store5_log' TO N'/var/opt/mssql/data/store5_log.ldf',
     REPLACE;
```

> Yedek dosyası, `.env` ve bağlantı bilgileri repoya dahil değildir.

### 2. Backend

`backend/B2BStore.TechnicalSettings.Api/appsettings.Development.json` dosyasını oluşturun:

```json
{
  "ConnectionStrings": {
    "Store": "Server=localhost,1433;Database=store5;User Id=sa;Password=<şifre>;TrustServerCertificate=True;"
  }
}
```

```powershell
cd backend/B2BStore.TechnicalSettings.Api
dotnet run
```

- API: `http://localhost:5080`
- API dokümantasyonu: `http://localhost:5080/scalar`

Visual Studio ile: `B2BStore.TechnicalSettings.slnx` → **F5**

### 3. Frontend

```powershell
cd frontend
npm install
npm run dev
```

Uygulama: `http://localhost:5173` (`/api` istekleri otomatik olarak API'ye yönlendirilir).

### Tek servis olarak yayın

```powershell
cd frontend && npm run build        # çıktı: backend/.../wwwroot
cd ../backend/B2BStore.TechnicalSettings.Api
dotnet publish -c Release
```

API, derlenmiş Vue uygulamasını kendisi sunar.

## API

| Metot | Adres | Açıklama |
|---|---|---|
| `GET` | `/api/settings/catalog` | Ayar tanımları (ekran bu katalogdan oluşur) |
| `GET` | `/api/settings/rows` | DealerSettings satırları |
| `GET` | `/api/settings?row=1` | Seçili satırın değerleri |
| `PUT` | `/api/settings?row=1` | Değişiklikleri kaydeder |
| `GET` | `/api/health` | Uygulama + veritabanı kontrolü |

Örnek kayıt isteği:

```json
{
  "changes": [
    { "key": "MinCartCount", "value": "5" },
    { "key": "ParameterStock", "setBits": "8", "clearBits": "0" }
  ]
}
```

Hatalı istekte ayar bazında hata döner:

```json
{
  "title": "Bazı ayarlar kaydedilemedi",
  "status": 400,
  "errors": { "ProductTableStockType": ["Geçersiz seçenek."] }
}
```

## Bitwise parametreler nasıl kaydediliyor?

`ParameterMenu`, `ParameterProductDetail`, `ParameterStock` gibi kolonlar birden çok açık/kapalı seçeneği tek bir sayıda saklar; her seçenek bir bittir (1, 2, 4, 8 …).

Arayüz değerin tamamını değil, sadece **açılan** (`setBits`) ve **kapatılan** (`clearBits`) bitleri gönderir. API, satırı kilitleyip güncel değeri okur ve şu formülü uygular:

```
yeniDeğer = (mevcutDeğer | setBits) & ~clearBits

örnek:  mevcut 3 (0011)  +  setBits 8 (1000)  →  11 (1011)   önceki seçimler korunur
```

Böylece ekranda gösterilmeyen bitler ve aynı anda başka bir kullanıcının değiştirdiği bitler bozulmaz.
`bigint` değerler 2^62'ye kadar çıktığı için API'de string, arayüzde `BigInt` olarak taşınır (JavaScript `number` 2^53'ten büyük tam sayıları doğru tutamaz).

## Testler

```powershell
dotnet test
```

Birim testleri (xUnit): bitwise birleştirme, bilinmeyen bitlerin korunması, kolon tipi sınırları, katalog doğrulaması (her bitwise seçenek tek bit mi, kolona sığıyor mu), değer dönüşümleri.

## Proje yapısı

```
├─ backend/
│  ├─ B2BStore.TechnicalSettings.Api/     Web API
│  └─ B2BStore.TechnicalSettings.Tests/   xUnit testleri
├─ frontend/                              Vue 3 + TypeScript
│  └─ src/
│     ├─ api/            API istemcisi ve tipler
│     ├─ composables/    useSettings – durum, değişiklik takibi, BigInt bitwise
│     └─ components/     SettingField, BitmaskField, ChangeSummaryDialog, ToggleSwitch
├─ tools/build_catalog.py                 ayar kataloğu üretici + doğrulama
├─ SQL-Sorguları/                         analizde kullanılan sorgular
├─ docs/screenshots/
└─ docker-compose.yml                     SQL Server 2022
```

## Bilinen sınırlamalar

- `DealerSettings` tablosunda birincil anahtar yoktur. Satır, fiziksel konumuna (`%%physloc%%`) göre hedeflenir; güncelleme tam olarak 1 satırı etkilemezse işlem geri alınır. Kalıcı çözüm olarak tabloya anahtar kolon eklenmesi önerilir.
- Görsel alanlar (logo, favicon) dosya adı olarak düzenlenir; dosya yükleme bu modülün kapsamında değildir.
- Eski ekranda olup veritabanında karşılığı bulunmayan 4 kontrol (Menü Gösterimi 2, Ürün Detay Parametresi 2 – Oturum Kapalı, WhatsApp Sipariş Bildirimi, Favori İkonu Pozisyonu) yeni ekrana alınmamıştır.

---

Eren Altun · B2B Store İş Başvurusu – Teknik Değerlendirme
