# 🏨 Hotel QR SaaS — Frontend Developer Bələdçisi & API Sənədləşməsi

Bu sənəd **Hotel QR SaaS** platformasının frontend (UI/UX) proqramçısı üçün hazırlanmış tam texniki bələdçi və API sənədləşməsidir. Sistem **.NET 8 Web API** və **PostgreSQL** arxitekturası üzərində qurulub.

---

## 🚀 0. Backend-i Lokalda 1 Əmrlə İşə Salmaq (Docker Compose)

Frontend proqramçının kompüterində **.NET SDK** və ya **PostgreSQL** quraşdırmasına ehtiyac yoxdur. Sadəcə olaraq terminalda repozitoriya qovluğunda bu tək əmri işə salmaq kifayətdir:

```bash
docker compose up --build
```

Bu əmr avtomatik olaraq:
1. **PostgreSQL** bazasını qaldırır (`port: 5432`).
2. **.NET 8 Web API** tətbiqini build edir və işə salır (`port: 5281`).
3. Verilənlər bazasının cədvəllərini avtomatik yaradır və hazır demo otel məlumatlarını (`hotel-resort`), kateqoriyaları və test istifadəçilərini bazaya yazır (seed edir).

* Backend API URL: **`http://localhost:5281`**
* Swagger API Sənədləri: **`http://localhost:5281/swagger`**
* Test Admin Girişi: `admin@hotelresort.com` / `Hotel123!`

---

## 📌 1. Sistemin Arxitekturası & Əsas Məntiqi

Platforma **Multi-Tenant SaaS** (çoxlu otelli) arxitekturaya malikdir:
* Hər otel ayrıca bir `Tenant`-dır və unikal **`slug`** (məsələn: `hotel-resort`) ilə fəaliyyət göstərir.
* **Qonaqlar** heç bir qeydiyyat və ya login olmadan QR kodu skan edərək otelin mobil səhifəsinə daxil olurlar.
* Sistemdə 3 növ istifadəçi rolü var:
  1. **Qonaq (Public):** QR kod vasitəsilə mobil menyu və xidmətləri görür, reception-a mesaj yazır.
  2. **Receptionist:** Yalnız qonaqlardan gələn otaq xidməti və mesajları görüb zəng/WhatsApp ilə cavablayır.
  3. **HotelAdmin:** Otelin brendinqini (loqo, banner), menyularını, linklərini, əyləncə fəaliyyətlərini və statistikasını idarə edir.
  4. **SuperAdmin:** SaaS platformasının sahibidir — abunə haqqı ödənməyəndə oteli 1 kliklə dondurur (**Kill-Switch**).

Backend Base URL: `http://localhost:5281` (və ya production domeni)

---

## 📱 2. Qurulmalı Olan 4 Əsas Frontend Ekranı

Frontend layihəsində 4 əsas bölmə hazırlanmalıdır:

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Qonaq Portalı (Guest Mobile Web / PWA)                   │
│    URL: /hotel/{slug}  (Mobil cihazlar üçün 100% optimizasiya)│
├─────────────────────────────────────────────────────────────┤
│ 2. Resepşn Masası (Reception Desk Dashboard)                │
│    URL: /reception     (Sürətli bildiriş və çat menyusu)     │
├─────────────────────────────────────────────────────────────┤
│ 3. Otel Admin Paneli (Hotel Management Dashboard)           │
│    URL: /admin         (Brendinq, linklər, menyular, QR)    │
├─────────────────────────────────────────────────────────────┤
│ 4. SuperAdmin & Kill-Switch Paneli                          │
│    URL: /super-admin   (Bütün otellərin idarəsi və abunəlik) │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔑 3. Autentifikasiya (Giriş)

Admin, Resepşn və SuperAdmin üçün JWT Bearer token istifadə olunur.

* **Endpoint:** `POST /api/auth/login`
* **Request Body:**
  ```json
  {
    "email": "admin@hotelresort.com",
    "password": "Hotel123!"
  }
  ```
* **Response Body:**
  ```json
  {
    "success": true,
    "data": {
      "accessToken": "eyJhbGciOi...",
      "email": "admin@hotelresort.com",
      "role": "HotelAdmin",
      "tenantId": "d140e691-1234-4567-8901-abcdef123456",
      "tenantSlug": "hotel-resort"
    },
    "message": "Uğurla daxil oldunuz."
  }
  ```
* Bütün səlahiyyət tələb edən sorğularda header mütləq göndərilməlidir:
  `Authorization: Bearer <accessToken>`

---

## 📱 4. Ekran 1: Qonaq Mobil Portalı (Public Guest App)

Bu ekran otel qonağının QR kodu skan etdikdə gördüyü mobil veb səhifədir.

### 4.1. Əsas Portal Məlumatlarını Gətirmək
* **Endpoint:** `GET /api/public/{slug}?lang=az` *(Dəstəklənən dillər: az, en, ru)*
* **Xüsusi Hal (Kill-Switch Aktivdirsə):** Əgər otel dondurulubsa, API **`403 Forbidden`** qaytarır:
  ```json
  {
    "success": false,
    "message": "Bu otelin rəqəmsal xidmət abunəliyi dayandırılıb."
  }
  ```
  Bu halda frontend qonağa xidmətin dondurulduğu barədə məlumat ekranı göstərməlidir.

* **Uğurlu Cavab Strukturu (`PublicHotelPortalDto`):**
  ```json
  {
    "success": true,
    "data": {
      "slug": "hotel-resort",
      "name": "Hotel & Resort Baku",
      "bio": "Lüks istirahət və 5 ulduzlu otel xidmətləri",
      "logoUrl": "/uploads/hotel-resort/logo/logo.png",
      "coverImageUrl": "/uploads/hotel-resort/cover/banner.jpg",
      "phone": "+994125990000",
      "whatsApp": "+994501234567",
      "telegram": "@hotelresort",
      "email": "info@hotelresort.az",
      "mapUrl": "https://maps.google.com/...",
      "categories": [
        {
          "id": "e9fa24e2-65ba-4b7e-b7d4-dc9f6b29f881",
          "title": "Xidmətlər & Linklər",
          "layoutType": "list", // "list" (şaquli zolaq kartlar) və ya "icons" (4 sütunlu qrid)
          "links": [
            {
              "id": "06633cd3-520b-43d4-a73f-eb61a643435c",
              "title": "Otaq növləri",
              "linkType": 1,
              "actionUrl": "/api/r/06633cd3-520b-43d4-a73f-eb61a643435c",
              "iconKey": "bed",
              "openInNewTab": true
            }
          ]
        }
      ],
      "activities": [
        {
          "id": "7b0a8848-1111-2222-3333-444455556666",
          "title": "Hovuz Partisi & DJ Performance",
          "location": "Əsas Açıq Hovuz Kənarı",
          "startTime": "15:00",
          "endTime": "18:00",
          "category": "party"
        }
      ]
    }
  }
  ```

### 4.2. Kliklərin Hesablanması və Yönləndirmə (Redirect)
Qonaq istənilən menyu və ya linkə toxunduqda frontend linkin `actionUrl`-nə yönləndirir:
* **URL:** `GET /api/r/{linkId}`
* Backend klik sayını 1 vahid artırır, analitika bazasına (IP, Cihaz, Tarix) qeyd edir və qonağı **`302 Redirect`** ilə təyin olunmuş əsl hədəf linkinə yönləndirir.

### 4.3. Qonaq Sorğusu / Otaq Xidməti (Inquiry Modal)
Qonaq "Otaq Xidməti / Əlaqə" düyməsinə basdıqda açılan formanın backend sorğusu:
* **Endpoint:** `POST /api/public/{slug}/inquiries`
* **Request:**
  ```json
  {
    "fullName": "Elmir Qasımov",
    "phoneNumber": "+994501112233",
    "roomNumber": "304",
    "message": "Otağa əlavə su və dəsmal xahiş edirəm."
  }
  ```
* Bu sorğu dərhal Resepşn Masasına ötürülür.

### 4.4. Gündəlik Fəaliyyət Cədvəli (Activities Subpage)
Qonaq gündəlik partiləri, konsertləri və idman saatlarını görmək üçün:
* **Endpoint:** `GET /api/public/{slug}/activities?lang=az&date=2026-11-03`
* Frontend-də: **Bugün**, **Sabah** və **Bütün Həftə** tabları vasitəsilə filtrlənir.

---

## 🛎️ 5. Ekran 2: Resepşn Masası (Reception Desk Dashboard)

Resepşn əməkdaşlarının qonaqlardan daxil olan müraciətləri tez görüb dərhal reaksiya verməsi üçün nəzərdə tutulub.

* **Bütün Müraciətləri Gətirmək:**
  `GET /api/hotel-admin/inquiries`
  * Parametr: `?status=1` (Gözləyən/Yeni), `?status=2` (Cavablandı), `?status=3` (Ləğv edildi)
* **Cavab Strukturu:**
  ```json
  [
    {
      "id": "e9821a9c-0000-1111-2222-333344445555",
      "fullName": "Elmir Qasımov",
      "phoneNumber": "+994501112233",
      "roomNumber": "304",
      "message": "Otağa əlavə su və dəsmal xahiş edirəm.",
      "status": 1, // 1 = Pending, 2 = Contacted, 3 = Cancelled
      "createdAtUtc": "2026-11-02T19:30:00Z"
    }
  ]
  ```
* **Statusu Yeniləmək:**
  `PATCH /api/hotel-admin/inquiries/{id}/status?status=2`
* **Frontend Tələbləri:**
  - Hər sorğu kartında birbaşa **"Zəng Et"** (`tel:...`) və **"WhatsApp-da Cavabla"** (`https://wa.me/...`) düymələri.
  - Yeni müraciət daxil olduqda vizual xəbərdarlıq və ya səs siqnalı.
  - Siyahını hər 5-10 saniyədən bir avtomatik yeniləmək (polling).

---

## ⚙️ 6. Ekran 3: Otel Admin Paneli (Hotel Management)

Otel rəhbərliyinin öz portalını və məzmununu idarə etdiyi sahədir.

### 6.1. Brendinq: Profil Şəkli və Qapaq Banneri
* **Loqo Yükləmə:**
  `POST /api/hotel-admin/profile/logo`
  Header: `Content-Type: multipart/form-data`
  Body: `file` (şəkil faylı)
* **Cover Banner Yükləmə:**
  `POST /api/hotel-admin/profile/cover`
  Header: `Content-Type: multipart/form-data`
  Body: `file` (şəkil faylı)

### 6.2. Kateqoriyaların İdarə Edilməsi
* **Siyahı:** `GET /api/hotel-admin/categories`
* **Yeni Bölmə Yaratmaq:** `POST /api/hotel-admin/categories`
  ```json
  {
    "titleAz": "Restoran & Barlar",
    "titleEn": "Restaurants & Bars",
    "titleRu": "Рестораны и бары",
    "layoutType": "list", // "list" və ya "icons"
    "displayOrder": 1,
    "isActive": true
  }
  ```
* **Bölməni Silmək:** `DELETE /api/hotel-admin/categories/{id}`

### 6.3. Linklərin İdarə Edilməsi (Otaq növləri, Menyular, Sosial şəbəkələr)
* **Yeni Keçid Əlavə Et:** `POST /api/hotel-admin/links`
  ```json
  {
    "categoryId": "e9fa24e2-65ba-4b7e-b7d4-dc9f6b29f881",
    "titleAz": "Otaq növləri & Qiymətlər",
    "titleEn": "Room Types & Rates",
    "titleRu": "Номера и цены",
    "linkType": 1, // 1=StandardUrl, 6=PdfMenu, 7=Flipbook, 10=Booking, 8=GoogleReview, 4=WhatsApp, 2=Phone
    "targetUrl": "https://hotelresort.az/rooms",
    "iconKey": "bed", // bed, restaurant, spa, menu, booking, google, phone, whatsapp, telegram, instagram, facebook, youtube, tripadvisor
    "isCustomIcon": false,
    "openInNewTab": true,
    "displayOrder": 1,
    "isActive": true
  }
  ```
* **PDF Menyu Yükləmək:**
  `POST /api/hotel-admin/links/{id}/upload-file`
  Header: `multipart/form-data`, Body: `file` (PDF sənədi).
  *Bu sorğudan sonra `targetUrl` avtomatik olaraq həmin PDF faylının server linkinə çevrilir.*
* **Keçidi Silmək:** `DELETE /api/hotel-admin/links/{id}`

### 6.4. Gündəlik Əyləncə & Fəaliyyət Cədvəli (Activities)
* **Siyahı:** `GET /api/hotel-admin/activities`
* **Yeni Fəaliyyət Yarat:** `POST /api/hotel-admin/activities`
  ```json
  {
    "title": "Hovuz Partisi & DJ Performance",
    "location": "Əsas Açıq Hovuz",
    "startTime": "15:00",
    "endTime": "18:00",
    "category": "party", // party, music, wellness, kids, food
    "activityDate": "2026-11-03T00:00:00Z",
    "isRecurringDaily": true,
    "displayOrder": 1,
    "isActive": true
  }
  ```
* **Statusu Dəyişmək (Aktiv/Passiv):** `PATCH /api/hotel-admin/activities/{id}/toggle`
* **Fəaliyyəti Silmək:** `DELETE /api/hotel-admin/activities/{id}`

### 6.5. Dinamik QR Kod İdarəetməsi
* **PNG QR Kodu Əldə Et:** `GET /api/hotel-admin/qr/png` (PNG formatında yükləmə)
* **SVG QR Kodu Əldə Et:** `GET /api/hotel-admin/qr/svg` (Vektor formatında)
* **Qeyd:** QR kodların mərkəzində şirkətin rəsmi **Altensor** mərkəzi loqosu yerləşdirilib. Otel istədiyi vaxt QR kodunu yüksək keyfiyyətlə endirib çap edə bilər.

### 6.6. Statistika və Analitika
* **İcmal Məlumatlar:** `GET /api/hotel-admin/analytics/summary`
  Qaytarılan göstəricilər:
  - `totalViews`: Qonaq portalına baxış sayı
  - `totalLinkClicks`: Linklərə edilən kliklərin cəmi
  - `totalInquiries`: Resepşnə daxil olan müraciətlər
  - `deviceBreakdown`: Mobil vs Kompüter faizləri

---

## 🛡️ 7. Ekran 4: SuperAdmin & Kill-Switch İdarəetməsi

SaaS rəhbərliyinin otelləri və onların abunəliklərini tənzimlədiyi kabinetdir.

* **Bütün Otellərin Siyahısı:** `GET /api/super-admin/tenants`
* **Yeni Otel Qeydiyyatı:** `POST /api/super-admin/tenants`
* **Kill-Switch (Oteli Dondurmaq və ya Açmaq):**
  `PATCH /api/super-admin/tenants/{id}/toggle-status`
  ```json
  {
    "suspend": true,
    "reason": "Abunəlik ödənişi gecikir."
  }
  ```
  *Otel dondurulduqda qonaq portalları dərhal qapanır və ödəniş xəbərdarlığı çıxır.*

---

## 🗂️ 8. Sistemdə İstifadə Olunan Əsas Enumlar

### 8.1. Keçid Növləri (`LinkType`)
| ID | Kod | Təsvir |
|---|---|---|
| 1 | `StandardUrl` | Adi internet veb səhifəsi URL-i |
| 2 | `Phone` | Birbaşa telefon zəngi (`tel:+994...`) |
| 3 | `Email` | E-poçt ünvanı (`mailto:...`) |
| 4 | `WhatsApp` | WhatsApp çatı (`https://wa.me/...`) |
| 5 | `Telegram` | Telegram botu / kanalı (`https://t.me/...`) |
| 6 | `PdfMenu` | Serverdə saxlanılan PDF menyu faylı |
| 7 | `Flipbook` | İnteraktiv Heyzine və s. menyu vərəqləyicisi |
| 8 | `GoogleReview` | Google Maps rəy yazma linki |
| 9 | `TripAdvisor` | TripAdvisor rəy səhifəsi |
| 10 | `Booking` | Booking.com və ya otelin birbaşa rezervasiya sistemi |

### 8.2. Müraciət Statusları (`InquiryStatus`)
* `1` = **Pending** (Yeni / Gözləyən)
* `2` = **Contacted** (Cavablandı / Əlaqə saxlanıldı)
* `3` = **Cancelled** (Ləğv edildi)

### 8.3. Fəaliyyət Kateqoriyaları (`category`)
* `party` = Hovuz & Çimərlik partiləri
* `music` = Canlı musiqi & Konsertlər
* `wellness` = Yoqa, Fitness & Sağlamlıq
* `kids` = Uşaq animatorları & Əyləncələr
* `food` = Qastronomiya & Xüsusi şam proqramları

---

## 🎨 9. Frontend Dizayn & UI / UX Tövsiyələri

1. **Mobil-First Yanaşma:** Qonaq səhifəsi mütləq şəkildə mobil smartfon ekranlarında (375px - 430px) native tətbiq (PWA) kimi rahat və axıcı olmalıdır.
2. **Dizayn Stili:** Dark Premium UI (Apple / Spotify tərzində tünd göy və qara palitra `#0b0f19`, `#111827`, `#1e293b`).
3. **İkonlar:** FontAwesome və ya Lucide icon dəstindən istifadə edilə bilər.
4. **Çoxdillilik:** Qonaq portalının yuxarısında `AZ | EN | RU` keçidi yerləşdirilməlidir.
5. **Real-time:** Reception masası üçün məlumatların avtomatik yenilənməsi təmin edilməlidir.

---

## 🧪 10. Test Giriş Məlumatları (Default Credentials)

* **Otel Admin & Reception Hesabı:**
  - Email: `admin@hotelresort.com`
  - Şifrə: `Hotel123!`
  - Otel Slugu: `hotel-resort`
* **SaaS SuperAdmin Hesabı:**
  - Email: `admin@hotelqr.com`
  - Şifrə: `SuperAdmin123!`

