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

### 4.5. Skyhill Modulları (qr.unipos.tech/skyhill ilə 1:1 Eyniləşdirilmiş): Stories (Video + CTA), 6 Əsas Hub, 30 Kateqoriya, 128 Məhsul, Səbət və Rəy

`GET /api/public/{slug}?lang=az` sorğusu mövcud `categories` və `activities` ilə yanaşı aşağıdakı **Skyhill** məlumatlarını da qaytarır:
* `subtitle`: `"Pirqulu"`, `tagline`: `"Hotel & Spa"`, `starRating`: `5`
* `currencyCode`: `"₼"`, `serviceFeePercent`: `0`, `themePreset`: `"otel"`, `themeStyle`: `"yumsaq"`
* Sosial şəbəkə linkləri: `instagramUrl`, `facebookUrl`, `youtubeUrl`, `tikTokUrl`, `tripAdvisorUrl`, `bookingUrl`
* Hər bir mətndə həm seçilmiş dildəki dəyər (`title`, `subtitle`, `description`), həm də anında (client-side) dil dəyişimi (`AZ | EN | RU`) üçün üçdilli obyektlər (`titleTr: { az, en, ru }`, `subtitleTr`, `descriptionTr`, `badgeTr`, `buttonTr`) qaytarılır.
* `stories`: Yuxarıda üfüqi sürüşdürülən **6 hekayə zolağı** (`externalId: 2..7`):
  - **Xoş gəlmisiniz** (`videoUrl: ".../story-welcome.mp4"`, `buttonText: "Otel haqqında"`, `actionUrl: "/skyhill/hotel/g/191"`)
  - **Günün menyusu** (`actionUrl: "/skyhill/hotel/c/4578"`)
  - **Canlı musiqi** (`videoUrl: ".../story-live-music.mp4"`, `actionUrl: "/skyhill/hotel/p/43219"`)
  - **Spa −20%** (`actionUrl: "/skyhill/hotel/g/193"`)
  - **Ulduzlu gecə** (`actionUrl: "/skyhill/hotel/p/43224"`)
  - **Səhər yeməyi** (`actionUrl: "/skyhill/hotel/c/4582"`)
* `hubs`: Ana səhifədəki **6 əsas mərkəzi plitə (Hubs — `externalId: 191..196`)**:
  1. `191` (`info`) — **Otel məlumatı** (`displayMode: "categories"`)
  2. `192` (`food`) — **Qida və içki** (`displayMode: "categories"`)
  3. `193` (`spa`) — **Spa və sağlamlıq — MARAL Spa** (`displayMode: "categories"`)
  4. `194` (`entertainment`) — **Əyləncə** (`displayMode: "filter_items"` — `Hamısı / Restoran / Spa / Musiqi` filtri ilə birbaşa tədbirlər)
  5. `195` (`contacts`) — **Əlaqə** (`displayMode: "direct_items"`)
  6. `196` (`specials`) — **Günün təklifi** (`displayMode: "direct_items"`)
* `dailySpecials`: Günün xüsusi təklifləri siyahısı
* `departmentContacts`: Şöbələrin birbaşa zəng, WhatsApp, e-poçt və xəritə əlaqələri

#### A. Bütöv Kataloqu / Səbət Məlumatlarını Tək Sorğuda Gətirmək (`/catalog` və `/cart`)
UniPOS Skyhill-də olduğu kimi səbət (`localStorage`) və sürətli axtarış/filtr üçün bütün **30 kateqoriya (`4569..4598`)** və **128 məhsulu (`43107..43234`)** tək sorğuda gətirmək mümkündür:
* **Endpoint:** `GET /api/public/{slug}/catalog?lang=az` *(və ya `GET /api/public/{slug}/cart?lang=az`)*
* **Qaytarılan Məlumat (`PublicFullCatalogDto`):** `hubs`, `categories`, `items` və `products` (UniPOS formatında `nameTr`, `categoryNameTr`, `price`, `ingredient`, `qrIcon`, `qrBadge`, `qrActionType`, `qrActionValue`, `infoOnly`).

#### B. Hub Daxili Səhifəni Gətirmək (`/hotel/g/{hubCodeOrId}`)
* **Endpoint:** `GET /api/public/{slug}/hubs/{hubCodeOrId}?lang=az`
  *(Alternativ alias: `GET /api/public/{slug}/hotel/g/{hubCodeOrId}?lang=az`)*
  `hubCodeOrId` həm rəqəmsal Skyhill ID (`191`, `192`, `193`, `194`, `195`, `196`), həm `Guid`, həm də qısa kod (`info`, `food`, `spa`, `entertainment`, `contacts`, `specials`) ola bilər.
* **1 Məhsullu Kateqoriyaların Birbaşa Detala Yönləndirilməsi (1-Item Category Collapse):**
  UniPOS Skyhill-də olduğu kimi, əgər bir kateqoriyada yalnız 1 element varsa və `collapseIfSingleItem == true`-dursa (məs: `4569` *Xoş gəlmisiniz*, `4570` *Otel haqqında*, `4574` *Qaydalar*, `4575` *Ünvan*, `4576` *Əlaqə*, `4585` *MARAL Spa*, `4592` *Spa qaydaları*, `4593` *Spa rezervasiya*), həmin kateqoriyanın `targetUrl` sahəsi birbaşa məhsul səhifəsinə (`/{slug}/hotel/p/{singleItemExternalId}`) işarə edir.

#### C. Restoran / Spa Bölməsinin Menyusunu Gətirmək (`/hotel/c/{categoryCodeOrId}`)
* **Endpoint:** `GET /api/public/{slug}/catalog/categories/{categoryCodeOrId}?lang=az`
  *(Alternativ alias: `GET /api/public/{slug}/hotel/c/{categoryCodeOrId}?lang=az`)*
  `categoryCodeOrId` həm rəqəmsal Skyhill ID (`4569..4598`, məs: `4578` = SHIRVAN, `4579` = SAQI, `4580` = ELIXIR, `4581` = STRIKE, `4582` = Otaq xidməti), həm də `Guid` ola bilər.

#### D. Məhsul / Otaq / Məlumat Kartının Detal Səhifəsi (`/hotel/p/{itemCodeOrId}`)
* **Endpoint:** `GET /api/public/{slug}/catalog/items/{itemCodeOrId}?lang=az`
  *(Alternativ alias: `GET /api/public/{slug}/hotel/p/{itemCodeOrId}?lang=az`)*
  `itemCodeOrId` həm rəqəmsal Skyhill ID (`43107..43234`, məs: `43219` = Canlı caz axşamı, `43224` = Rəsədxana gecə turu), həm də `Guid` ola bilər.
  Hər bir elementdə `actionType` (`"phone" | "whatsapp" | "email" | "map"`), `actionValue`, `actionUrl`, `isOrderable` (`!infoOnly`), `iconKey` və üçdilli `titleTr`/`descriptionTr` mövcuddur.

#### E. Səbət və Otağa / Masaya Sifariş Göndərmək (`/cart`)
Qonaq `isOrderable: true` olan yeməkləri və ya spa xidmətlərini səbətə yığıb təsdiqlədikdə:
* **Endpoint:** `POST /api/public/{slug}/orders`
* **Request Body (`SubmitOrderRequestDto`):**
  ```json
  {
    "orderType": 1, // 1=RoomService, 2=RestaurantTable, 3=SpaReservation, 4=ActivityBooking
    "roomOrTableNumber": "304",
    "guestName": "Elmir Qasımov",
    "phoneNumber": "+994501112233",
    "notes": "Zəhmət olmasa çörək isti olsun.",
    "items": [
      {
        "catalogItemId": "e1a2b3c4-...",
        "itemTitle": "Aşpazın təklifi — quzu qabırğası",
        "quantity": 1,
        "unitPrice": 34.00,
        "specialInstruction": "Az acılı"
      },
      {
        "catalogItemId": "f5d6e7a8-...",
        "itemTitle": "Şah plov",
        "quantity": 1,
        "unitPrice": 28.00
      }
    ]
  }
  ```
* *Qeyd:* Sifariş yaradılan kimi avtomatik `ORD-MMDD-XXXX` nömrəsi alır, **Resepşn panelinə**, otelin **Telegram qrupuna** (detallı çek formatında) və **E-poçtuna** göndərilir.

#### E. Daxili 5 Ulduz və Emoji Qiymətləndirməsi (`/review`)
* **Endpoint:** `POST /api/public/{slug}/reviews`
* **Request Body (`SubmitReviewRequestDto`):**
  ```json
  {
    "rating": 5, // 1..5 ulduz
    "guestName": "Elmir Qasımov",
    "roomNumber": "304",
    "phoneNumber": "+994501112233",
    "department": "Restaurant",
    "comment": "SHIRVAN restoranında Şah plov mükəmməl idi!"
  }
  ```
* *Ağıllı Bildiriş:* Əgər `rating <= 3` olarsa, otel rəhbərliyinə Telegram-da `⚠️ TƏCİLİ: Mənfi Qonaq Rəyi` xəbərdarlığı gedir.

#### F. Dinamik PWA Manifest (`/manifest.webmanifest`)
* **Endpoint:** `GET /api/public/{slug}/manifest.webmanifest`
* Hər otel üçün fərdi rəng, ad, loqo və qısa yollar (`/favourites`, `/cart`) ehtiva edən `application/manifest+json` qaytarır.

#### G. Otel Admin Paneli üçün Yeni CRUD Endpoint-ləri (`/api/hotel-admin/*`)
* **Stories:** `GET/POST /api/hotel-admin/stories`, `PUT/DELETE /api/hotel-admin/stories/{id}`, `POST /api/hotel-admin/stories/{id}/image`
* **Hubs (6 Əsas Plitə):** `GET/POST /api/hotel-admin/hubs`, `PUT/DELETE /api/hotel-admin/hubs/{id}`, `POST /api/hotel-admin/hubs/{id}/cover`
* **Kataloq Kateqoriyaları (Restoran/Spa/Bölmə):** `GET/POST /api/hotel-admin/catalog/categories?hubId=...`, `PUT/DELETE /api/hotel-admin/catalog/categories/{id}`, `POST /api/hotel-admin/catalog/categories/{id}/image`
* **Kataloq Elementləri (Yemək/Spa/Otaq/Məlumat):** `GET/POST /api/hotel-admin/catalog/items?hubId=...&categoryId=...`, `PUT/DELETE /api/hotel-admin/catalog/items/{id}`, `POST /api/hotel-admin/catalog/items/{id}/image`
* **Səbət Sifarişləri (Orders):** `GET /api/hotel-admin/orders?status=1` və `PATCH /api/hotel-admin/orders/{id}/status` (`{ "status": 3 }`)
* **Qonaq Rəyləri (Reviews):** `GET /api/hotel-admin/reviews?isResolved=false` və `PATCH /api/hotel-admin/reviews/{id}/resolve`
* **Şöbə Əlaqələri (Departments):** `GET/POST /api/hotel-admin/departments`, `PUT/DELETE /api/hotel-admin/departments/{id}`

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
  - `totalScans`: Qonaq portalına baxış sayı
  - `totalLinkClicks`: Linklərə edilən kliklərin cəmi
  - `totalInquiries`: Resepşnə daxil olan müraciətlər
  - `totalOrders`: Səbət sifarişlərinin sayı
  - `totalOrderRevenue`: Sifarişlərdən ümumi dövriyyə (₼)
  - `totalReviews` & `averageRating`: Qonaq rəylərinin sayı və orta ulduz balı (məs: `4.9`)
  - `deviceBreakdown`: Mobil vs Kompüter faizləri

### 6.7. Skyhill Kataloqu İdarəetməsi (Otel Admin Paneli — Tam CRUD)

Otel admini öz panelindən Skyhill-dəki bütün elementləri (Stories, 6 Hub, Restoran/Spa kateqoriyaları, Qiymətli yeməklər, Otaq növləri, Şöbə nömrələri) yarada, yeniləyə, şəklini yükləyə və silə bilir:

#### A. Yuxarı "Stories / Highlights" İdarəetməsi
* **Siyahı:** `GET /api/hotel-admin/stories`
* **Yeni Story Yarat:** `POST /api/hotel-admin/stories`
  ```json
  {
    "titleAz": "Spa −20%",
    "titleEn": "Spa −20%",
    "titleRu": "Спа −20%",
    "descriptionAz": "Həftəiçi bütün masajlara 20% endirim!",
    "badgeText": "Endirim",
    "actionUrl": "spa",
    "displayOrder": 1,
    "isActive": true
  }
  ```
* **Şəkil Yüklə:** `POST /api/hotel-admin/stories/{id}/image` (`multipart/form-data`, `file`)
* **Redaktə / Sil:** `PUT /api/hotel-admin/stories/{id}` | `DELETE /api/hotel-admin/stories/{id}`

#### B. 6 Əsas Bölmə Plitəsi (Hubs) İdarəetməsi
* **Siyahı:** `GET /api/hotel-admin/hubs`
* **Yeni Hub Yarat:** `POST /api/hotel-admin/hubs`
  ```json
  {
    "code": "spa",
    "hubType": 3, // 1=HotelInfo, 2=FoodAndBeverage, 3=SpaAndWellness, 4=Entertainment, 5=Contacts, 6=DailySpecials
    "kickerAz": "MARAL",
    "titleAz": "Spa və sağlamlıq",
    "titleEn": "Spa & Wellness",
    "titleRu": "Спа и велнес",
    "iconKey": "sparkles",
    "displayOrder": 3,
    "isActive": true
  }
  ```
* **Qapaq Şəkli Yüklə:** `POST /api/hotel-admin/hubs/{id}/cover` (`multipart/form-data`, `file`)
* **Redaktə / Sil:** `PUT /api/hotel-admin/hubs/{id}` | `DELETE /api/hotel-admin/hubs/{id}`

#### C. Məkanlar və Alt-Kateqoriyalar (Restoranlar, Barlar, Spa Bölmələri, Otaqlar)
* **Siyahı:** `GET /api/hotel-admin/catalog/categories?hubId={hubId}`
* **Yeni Məkan / Kateqoriya Yarat:** `POST /api/hotel-admin/catalog/categories`
  ```json
  {
    "hubId": "d140e691-...",
    "parentCategoryId": null,
    "titleAz": "SHIRVAN",
    "titleEn": "SHIRVAN",
    "titleRu": "SHIRVAN",
    "subtitleAz": "Restoran və teras · Bütün gün",
    "workingHours": "07:30–23:30",
    "internalExtension": "3",
    "iconKey": "restaurant",
    "layoutStyle": "list",
    "displayOrder": 1,
    "isActive": true
  }
  ```
* **Şəkil Yüklə:** `POST /api/hotel-admin/catalog/categories/{id}/image` (`multipart/form-data`, `file`)
* **Redaktə / Sil:** `PUT /api/hotel-admin/catalog/categories/{id}` | `DELETE /api/hotel-admin/catalog/categories/{id}`

#### D. Məhsullar, Yeməklər, Spa Xidmətləri, Otaq Növləri və Məlumat Kartları (`CatalogItems`)
* **Siyahı:** `GET /api/hotel-admin/catalog/items?hubId={hubId}&categoryId={categoryId}`
* **Yeni Yemək / Xidmət / Otaq Yarat:** `POST /api/hotel-admin/catalog/items`
  ```json
  {
    "hubId": "d140e691-...",
    "categoryId": "a230f112-...",
    "itemType": 2, // 1=InfoArticle, 2=MenuProduct, 3=SpaService, 4=ActivityOffer, 5=SpecialDeal
    "titleAz": "Aşpazın təklifi — quzu qabırğası",
    "titleEn": "Chef's Special — Lamb Ribs",
    "titleRu": "Предложение шефа — бараньи рёбрышки",
    "descriptionAz": "Dağ kəklikotusu ilə marinadlanmış təndir quzu qabırğası...",
    "badgeAz": "Aşpazın seçimi",
    "filterTag": "food",
    "price": 34.00,
    "oldPrice": null,
    "currency": "₼",
    "portionOrDuration": "380 qr",
    "bulletPointsAz": "Təndirdə bişirilmiş\nKözlənmiş tərəvəzlər daxildir",
    "isOrderable": true, // true olduqda qonaq səbətə əlavə edə bilir
    "isFeaturedInDailySpecials": true, // true olduqda "Günün təklifi" bölməsində də çıxır
    "actionLabelAz": null,
    "actionUrl": null,
    "displayOrder": 1,
    "isActive": true
  }
  ```
* **Məhsul Şəkli Yüklə:** `POST /api/hotel-admin/catalog/items/{id}/image` (`multipart/form-data`, `file`)
* **Redaktə / Sil:** `PUT /api/hotel-admin/catalog/items/{id}` | `DELETE /api/hotel-admin/catalog/items/{id}`

#### E. Şöbə Əlaqələri (`Departments`)
* **Siyahı:** `GET /api/hotel-admin/departments`
* **Yeni Şöbə Nömrəsi Yarat:** `POST /api/hotel-admin/departments`
  ```json
  {
    "titleAz": "Otaq xidməti (Room Service)",
    "titleEn": "In-Room Dining",
    "subtitleAz": "24 saat yemək və içki sifarişi",
    "phoneNumber": "+994125551234",
    "internalExtension": "3",
    "whatsAppNumber": "+994505551234",
    "workingHours": "24/7",
    "iconKey": "bell",
    "displayOrder": 2,
    "isActive": true
  }
  ```
* **Redaktə / Sil:** `PUT /api/hotel-admin/departments/{id}` | `DELETE /api/hotel-admin/departments/{id}`

#### F. Səbət Sifarişləri və Qonaq Rəyləri (Resepşn & Admin üçün)
* **Sifarişlərin Siyahısı:** `GET /api/hotel-admin/orders?status=1` (`1=New, 2=Accepted, 3=Preparing, 4=Completed, 5=Cancelled`)
* **Sifariş Statusunu Dəyiş:** `PATCH /api/hotel-admin/orders/{id}/status` -> Body: `{ "status": 3 }`
* **Qonaq Rəylərinin Siyahısı:** `GET /api/hotel-admin/reviews?isResolved=false`
* **Rəyi Baxılmış İşarələ:** `PATCH /api/hotel-admin/reviews/{id}/resolve`

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

* **Demo Otel 1 — Skyhill Pirgulu Hotel & Spa (Tam Skyhill Kataloqu ilə):**
  - Email: `admin@skyhill.az`
  - Şifrə: `Hotel123!`
  - Otel Slugu: `skyhill` (`GET /api/public/skyhill`)
* **Demo Otel 2 — Hotel & Resort (Həm PDF/Linklər, həm də Skyhill Kataloqu ilə):**
  - Email: `admin@hotelresort.com`
  - Şifrə: `Hotel123!`
  - Otel Slugu: `hotel-resort` (`GET /api/public/hotel-resort`)
* **SaaS SuperAdmin Hesabı:**
  - Email: `admin@hotelqr.com`
  - Şifrə: `SuperAdmin123!`

