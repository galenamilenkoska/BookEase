namespace BookEase.Services;

/// <summary>
/// No-op email service used when Email:Enabled is false (e.g. local dev without SMTP config).
/// Logs a message instead of sending, so you can confirm the call is reaching the service.
/// </summary>
public class NullEmailService : IEmailService
{
    private readonly ILogger<NullEmailService> _logger;

    public NullEmailService(ILogger<NullEmailService> logger) => _logger = logger;

    public Task SendBookingConfirmationAsync(BookingEmailData data)
    {
        _logger.LogInformation(
            "[Email disabled] Would have sent confirmation to {Email} for appointment #{Id}",
            data.CustomerEmail, data.AppointmentId);
        return Task.CompletedTask;
    }
}
