# Hotel QR & Rəqəmsal Konsyerj SaaS Platforması (.NET 8)

`zeki.link/Hotel.Resort` sistemi əsasında hazırlanmış, tam funksional, multi-tenant arxitekturaya malik .NET 8 Web API backend platforması.

---

## 🏗️ Arxitektura və Texnoloji Stek

- **Platforma:** .NET 8 (LTS) Web API
- **Arxitektura:** Clean Architecture (Domain, Application, Persistence, Infrastructure, WebApi)
- **Dizayn Patternləri:** Repository Pattern, Unit of Work, Strategy (Bildirişlər üçün)
- **Verilənlər Bazası:** PostgreSQL (EF Core + Npgsql)
- **Autentifikasiya:** ASP.NET Core Identity + JWT Bearer Token (Access & Refresh Token)
- **Fayl Saxlama:** Serverin lokal qovluğu (`wwwroot/uploads/{tenantId}/...`) — heç bir AWS/S3 xərci yoxdur.
- **QR & vCard:** `QRCoder` (PNG və çap üçün vektor SVG), dinamik vCard 3.0 generatoru
- **Bildirişlər:** MailKit (SMTP) və Telegram Bot API (HTTP)

---

## 🔑 Əsas SaaS Xüsusiyyətləri

### 1. Ödəniş Etməyən Müştərinin Bağlanması (Kill-Switch)
- `SuperAdmin` istənilən vaxt `PATCH /api/super-admin/tenants/{id}/subscription` endpoint-i ilə otelin abunəliyini dayandıra (`Suspended` / `Expired`) və ya `IsActive = false` edə bilər.
- Abunəlik dayandırıldıqda:
  - Qonaq QR kodu oxudarkən (`GET /api/public/{slug}`) və ya menyu linklərinə klikləyərkən (`GET /api/r/{linkId}`) sistem dərhal **`403 Forbidden`** və `"TENANT_SUBSCRIPTION_INACTIVE"` statusu qaytarır.
  - Bütün QR və xidmətlər ödəniş edilənə qədər avtomatik bloklanır.

### 2. Qonaq Sorğuları Bildirişi (Telegram və E-poçt İnteqrasiyası)
- **A. Telegram:**
  1. Otel Telegram-da `@BotFather`-ə daxil olub öz brend adında bot açır (məs: `@GalaaltiResortBot`) və **Token** alır.
  2. Otel admini öz botuna girib **/start** basır (və ya botu qəbul/reception qrupuna əlavə edir).
  3. Admin panelində `POST /api/hotel-admin/notifications/telegram/connect` endpoint-inə sadəcə botun **Token**ini yapışdırır.
  4. Sistem avtomatik olaraq Telegram API vasitəsilə **Chat ID**-ni aşkar edir, botu təsdiqləyir və çat qrupuna sınaq mesajı göndərir.

- **B. E-poçt (Otel Özü Təyin Edir):**
  1. Otel admini bildirişləri almaq istədiyi e-poçt ünvanını (məs: `reception@galaalti.az` və ya bir neçə ünvan `reception@otel.az, manager@otel.az`) daxil edir:
     * **Endpoint:** `POST /api/hotel-admin/notifications/email/connect`
     * **Request:** `{ "email": "reception@galaalti.az", "sendTestEmail": true }`
  2. Sistem həmin ünvana sınaq məktubu göndərərək təsdiqləyir.
  3. *(Opsional)* Əgər otel məktubların öz korporativ poçt serveri (məs: Exchange / Office365 / Gmail) vasitəsilə göndərilməsini istəsə, öz SMTP tənzimləmələrini də yaza bilir; əks halda platformanın ümumi poçt servisi işləyir.

### 3. İstifadəçilər (Qonaqlar) və Adminlərin Ayrılması
- **Qonaqlar (İstifadəçilər) üçün:** Heç bir login, qeydiyyat və ya şifrə tələb olunmur. Bütün `/api/public/*` və `/api/r/*` endpoint-ləri tamamilə anonim və açıqdır (`AllowAnonymous`).
- **Adminlər üçün:** Yalnız sistem idarəçiləri (`SuperAdmin`) və otel menecerləri (`HotelAdmin`) JWT token vasitəsilə daxil olurlar.

---

## 🚀 Başlanğıc və Test Hesabları

Verilənlər bazası işə düşərkən sistem tərəfindən avtomatik olaraq ilkin rollar və test hesabları yaradılır:

### 1. Platform SuperAdmin:
- **E-poçt:** `admin@hotelqr.com`
- **Şifrə:** `SuperAdmin123!`
- **Səlahiyyət:** Bütün otellərə nəzarət, yeni otel açmaq, abunəlikləri bloklamaq/aktivləşdirmək.

### 2. Demo Otel ("Hotel & Resort") Admini:
- **E-poçt:** `admin@hotelresort.com`
- **Şifrə:** `Hotel123!`
- **Slug:** `hotel-resort`
- **Səlahiyyət:** Öz menyusunu, PDF menyularını, nömrələrini, QR kodlarını və gələn qonaq mesajlarını idarə etmək.

---

## 📡 Əsas API Endpoint-ləri

### A. Qonaqlar üçün (Açıq / Anonim)
- `GET /api/public/{slug}?lang=az` — Otelin rəqəmsal profilini və aktiv menyularını gətirir.
- `GET /api/public/{slug}/vcard` — Otelin nömrələrini birbaşa telefona kontakt kimi yazır.
- `POST /api/public/{slug}/inquiries` — Qonağın əlaqə sorğusu göndərməsi.
- `GET /api/r/{linkId}` — Menyu/flipbook linkinə klikləri qeydə alaraq hədəfə `302 Redirect` edir.

### B. Otel Admini üçün (`HotelAdmin`)
- `GET /api/hotel-admin/profile` — Profil məlumatları.
- `POST /api/hotel-admin/profile/logo` — Loqo yükləmək.
- `POST /api/hotel-admin/profile/cover` — Qapaq şəkli yükləmək.
- `GET /api/hotel-admin/categories` & `POST /api/hotel-admin/categories` — Kateqoriyalar.
- `POST /api/hotel-admin/links` — Yeni link və ya menyu əlavə etmək.
- `POST /api/hotel-admin/links/{id}/upload-file` — PDF menyu yükləmək.
- `GET /api/hotel-admin/qr/png` & `GET /api/hotel-admin/qr/svg` — Çap üçün vektor və PNG QR kod yükləmək.
- `GET /api/hotel-admin/analytics/summary` — Oxunma və klik statistikası.
- `GET /api/hotel-admin/inquiries` — Gələn qonaq mesajları.

### C. Super Admin üçün (`SuperAdmin`)
- `GET /api/super-admin/tenants` — Bütün otellər.
- `POST /api/super-admin/tenants` — Yeni otel yaratmaq.
- `PATCH /api/super-admin/tenants/{id}/subscription` — Abunəliyi bloklamaq və ya uzatmaq.
- `GET /api/super-admin/dashboard` — SaaS platformasının ümumi göstəriciləri.

