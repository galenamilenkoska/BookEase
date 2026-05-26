using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BookEase.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendBookingConfirmationAsync(BookingEmailData data)
    {
        try
        {
            var host     = _config["Email:Host"]!;
            var port     = _config.GetValue<int>("Email:Port");
            var username = _config["Email:Username"]!;
            var password = _config["Email:Password"]!;
            var fromAddr = _config["Email:FromAddress"]!;
            var fromName = _config["Email:FromName"] ?? "BookEase Barbershop";
            var adminEmail = _config["Email:AdminEmail"];

            var customerMsg = BuildCustomerEmail(data, fromAddr, fromName);
            await SendAsync(customerMsg, host, port, username, password);
            _logger.LogInformation("Booking confirmation sent to {Email}", data.CustomerEmail);

            if (!string.IsNullOrWhiteSpace(adminEmail))
            {
                var adminMsg = BuildAdminEmail(data, fromAddr, fromName, adminEmail);
                await SendAsync(adminMsg, host, port, username, password);
                _logger.LogInformation("Admin notification sent to {Admin}", adminEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking email for appointment #{Id}", data.AppointmentId);
            // Never re-throw — email failure must not break the booking
        }
    }

    private static MimeMessage BuildCustomerEmail(
        BookingEmailData d, string fromAddress, string fromName)
    {
        var endTime  = d.StartTime.AddMinutes(d.DurationMinutes);
        var notesRow = string.IsNullOrWhiteSpace(d.Notes)
            ? string.Empty
            : $"<div class=\"row\"><span class=\"label\">Notes</span><span class=\"value\">{d.Notes}</span></div>";

        var html = $@"<!DOCTYPE html>
<html>
<head>
  <meta charset=""utf-8""/>
  <style>
    body{{font-family:Arial,sans-serif;background:#f5f4f0;margin:0;padding:0}}
    .wrap{{max-width:560px;margin:32px auto;background:#fff;border-radius:12px;box-shadow:0 4px 20px rgba(0,0,0,.08);overflow:hidden}}
    .hdr{{background:#1a1a2e;padding:32px 40px;text-align:center}}
    .hdr h1{{color:#c9a84c;font-size:24px;margin:0}}
    .hdr p{{color:rgba(255,255,255,.6);margin:6px 0 0;font-size:13px}}
    .body{{padding:32px 40px}}
    .greeting{{font-size:18px;color:#1a1a2e;margin-bottom:8px}}
    .sub{{color:#666;font-size:14px;margin-bottom:24px;line-height:1.6}}
    .box{{background:#f5f4f0;border-radius:8px;padding:20px 24px;margin-bottom:24px;border:1px solid #e0ddd5}}
    .row{{display:flex;justify-content:space-between;padding:8px 0;border-bottom:1px solid #e0ddd5;font-size:14px}}
    .row:last-child{{border-bottom:none}}
    .label{{color:#999;font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:.06em}}
    .value{{color:#2d2d2d;font-weight:600;text-align:right}}
    .price{{color:#1a1a2e;font-size:17px}}
    .badge{{display:inline-block;background:#fff8e6;color:#92620a;border:1px solid #ffe08a;border-radius:100px;font-size:11px;font-weight:700;padding:3px 10px;text-transform:uppercase;letter-spacing:.06em}}
    .addr{{background:#fff8e6;border-radius:8px;padding:16px 20px;font-size:13px;color:#555;line-height:1.7;margin-bottom:24px}}
    .note{{font-size:13px;color:#888}}
    .ftr{{background:#f5f4f0;padding:16px 40px;text-align:center;font-size:12px;color:#aaa;border-top:1px solid #e0ddd5}}
  </style>
</head>
<body>
  <div class=""wrap"">
    <div class=""hdr"">
      <h1>&#9986; BookEase</h1>
      <p>Premium Barbershop &middot; Est. 2018</p>
    </div>
    <div class=""body"">
      <p class=""greeting"">You're all booked, {d.CustomerName}! &#9989;</p>
      <p class=""sub"">Your appointment has been received and is pending confirmation. We look forward to seeing you.</p>
      <div class=""box"">
        <div class=""row""><span class=""label"">Service</span><span class=""value"">{d.ServiceName}</span></div>
        <div class=""row""><span class=""label"">Date</span><span class=""value"">{d.StartTime:dddd, MMMM d, yyyy}</span></div>
        <div class=""row""><span class=""label"">Time</span><span class=""value"">{d.StartTime:h:mm tt} &ndash; {endTime:h:mm tt}</span></div>
        <div class=""row""><span class=""label"">Duration</span><span class=""value"">{d.DurationMinutes} minutes</span></div>
        <div class=""row""><span class=""label"">Price</span><span class=""value price"">${d.Price:F2}</span></div>
        <div class=""row""><span class=""label"">Status</span><span class=""value""><span class=""badge"">Pending</span></span></div>
        {notesRow}
      </div>
      <div class=""addr"">
        &#128205; <strong>BookEase Barbershop</strong><br/>
        123 Main Street, Your City<br/>
        Mon&ndash;Sat: 9:00 AM &ndash; 5:00 PM
      </div>
      <p class=""note"">Need to make changes? Reply to this email or call us directly.</p>
    </div>
    <div class=""ftr"">&copy; {DateTime.Now.Year} BookEase Barbershop &middot; All rights reserved</div>
  </div>
</body>
</html>";

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(fromName, fromAddress));
        msg.To.Add(new MailboxAddress(d.CustomerName, d.CustomerEmail));
        msg.Subject = $"Booking Confirmed \u2013 {d.ServiceName} on {d.StartTime:MMM d}";
        msg.Body    = new TextPart("html") { Text = html };
        return msg;
    }

    private static MimeMessage BuildAdminEmail(
        BookingEmailData d, string fromAddress, string fromName, string adminEmail)
    {
        var notesRow = string.IsNullOrWhiteSpace(d.Notes)
            ? string.Empty
            : $"<div class=\"row\"><span class=\"label\">Notes</span><span class=\"value\">{d.Notes}</span></div>";

        var html = $@"<!DOCTYPE html>
<html>
<head>
  <meta charset=""utf-8""/>
  <style>
    body{{font-family:Arial,sans-serif;background:#f5f4f0;margin:0;padding:0}}
    .wrap{{max-width:520px;margin:32px auto;background:#fff;border-radius:12px;box-shadow:0 4px 20px rgba(0,0,0,.08);overflow:hidden}}
    .hdr{{background:#1a1a2e;padding:24px 32px}}
    .hdr h2{{color:#c9a84c;margin:0;font-size:20px}}
    .hdr p{{color:rgba(255,255,255,.6);margin:4px 0 0;font-size:13px}}
    .body{{padding:28px 32px}}
    .row{{display:flex;justify-content:space-between;padding:8px 0;border-bottom:1px solid #eee;font-size:14px}}
    .row:last-child{{border-bottom:none}}
    .label{{color:#999;font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:.06em}}
    .value{{color:#2d2d2d;font-weight:600;text-align:right}}
    .note{{margin-top:20px;font-size:13px;color:#666}}
    .ftr{{background:#f5f4f0;padding:16px 32px;font-size:12px;color:#aaa;text-align:center;border-top:1px solid #e0ddd5}}
  </style>
</head>
<body>
  <div class=""wrap"">
    <div class=""hdr"">
      <h2>New Booking Received</h2>
      <p>Appointment #{d.AppointmentId} &mdash; action may be required</p>
    </div>
    <div class=""body"">
      <div class=""row""><span class=""label"">Customer</span><span class=""value"">{d.CustomerName}</span></div>
      <div class=""row""><span class=""label"">Email</span><span class=""value"">{d.CustomerEmail}</span></div>
      <div class=""row""><span class=""label"">Service</span><span class=""value"">{d.ServiceName}</span></div>
      <div class=""row""><span class=""label"">Date &amp; Time</span><span class=""value"">{d.StartTime:MMM d, yyyy @ h:mm tt}</span></div>
      <div class=""row""><span class=""label"">Duration</span><span class=""value"">{d.DurationMinutes} min</span></div>
      <div class=""row""><span class=""label"">Price</span><span class=""value"">${d.Price:F2}</span></div>
      {notesRow}
      <p class=""note"">Visit <strong>/admin</strong> to confirm or cancel this appointment.</p>
    </div>
    <div class=""ftr"">BookEase Admin Notification</div>
  </div>
</body>
</html>";

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(fromName, fromAddress));
        msg.To.Add(new MailboxAddress("BookEase Admin", adminEmail));
        msg.Subject = $"New Booking #{d.AppointmentId} \u2013 {d.CustomerName} \u2013 {d.StartTime:MMM d @ h:mm tt}";
        msg.Body    = new TextPart("html") { Text = html };
        return msg;
    }

    private static async Task SendAsync(
        MimeMessage message, string host, int port, string username, string password)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
