namespace BookEase.Services;

public interface IEmailService
{
    Task SendBookingConfirmationAsync(BookingEmailData data);
}

public record BookingEmailData(
    string CustomerName,
    string CustomerEmail,
    string ServiceName,
    DateTime StartTime,
    int DurationMinutes,
    decimal Price,
    int AppointmentId,
    string? Notes
);
