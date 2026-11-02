namespace HotelQrSaaS.Application.Common.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }

    public AppException(string message, int statusCode = 400, string? errorCode = null) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404, "NOT_FOUND") { }
}

public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message, 400, "BAD_REQUEST") { }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "İcazə verilmədi.") : base(message, 401, "UNAUTHORIZED") { }
}

public class TenantSuspendedException : AppException
{
    public TenantSuspendedException(string message = "Bu otelin rəqəmsal xidmət abunəliyi aktiv deyil.")
        : base(message, 403, "TENANT_SUBSCRIPTION_INACTIVE") { }
}

