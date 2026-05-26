using System.ComponentModel.DataAnnotations;

namespace BookEase.Models;

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Cancelled
}

public class Appointment
{
    public int Id { get; set; }

    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    [Required, MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? CustomerPhone { get; set; }

    public DateTime StartTime { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
}
