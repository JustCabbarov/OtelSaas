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
    QrCodeScan = 5
}

