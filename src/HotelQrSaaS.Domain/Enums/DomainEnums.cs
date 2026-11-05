namespace HotelQrSaaS.Domain.Enums;

public enum SubscriptionStatus
{
    Active = 1,
    Trial = 2,
    Suspended = 3,
    Expired = 4
}

public enum NotificationChannel
{
    None = 0,
    Email = 1,
    Telegram = 2,
    Both = 3
}

public enum LinkType
{
    StandardUrl = 1,
    Phone = 2,
    Email = 3,
    WhatsApp = 4,
    Telegram = 5,
    PdfMenu = 6,
    Flipbook = 7,
    GoogleReview = 8,
    TripAdvisor = 9,
    Booking = 10,
    Location = 11
}

public enum InquiryStatus
{
    New = 1,
    InProgress = 2,
    Resolved = 3,
    Archived = 4
}

public enum ThemeMode
{
    Dark = 1,
    Light = 2,
    System = 3
}

public enum AnalyticsEventType
{
    ProfileView = 1,
    LinkClick = 2,
    VCardDownload = 3,
    InquirySubmitted = 4,
    QrCodeScan = 5,
    OrderSubmitted = 6,
    ReviewSubmitted = 7,
    CatalogItemView = 8
}

public enum HubType
{
    HotelInfo = 1,
    FoodAndBeverage = 2,
    SpaAndWellness = 3,
    Entertainment = 4,
    Contacts = 5,
    DailySpecials = 6,
    Custom = 7
}

public enum CatalogItemType
{
    InfoArticle = 1,
    MenuProduct = 2,
    SpaService = 3,
    ActivityOffer = 4,
    SpecialDeal = 5
}

public enum OrderType
{
    RoomService = 1,
    RestaurantTable = 2,
    SpaReservation = 3,
    ActivityBooking = 4
}

public enum OrderStatus
{
    New = 1,
    Accepted = 2,
    Preparing = 3,
    Completed = 4,
    Cancelled = 5
}

