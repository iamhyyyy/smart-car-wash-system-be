using SmartCarWash.Domain.Enums;

namespace SmartCarWash.Application.DTOs;

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? PromoId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public DateTime? CheckinTime { get; set; }
    public DateTime? CompletedTime { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public int PointsEarned { get; set; }
    public int PointsRedeemed { get; set; }
    public BookingStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? CancelReason { get; set; }
    public string? StaffNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdateBy { get; set; }
}

public class CreateBookingDto
{
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? PromoId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? StaffNotes { get; set; }
}

public class UpdateBookingDto
{
    public Guid? PromoId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public DateTime? CheckinTime { get; set; }
    public DateTime? CompletedTime { get; set; }
    public BookingStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? CancelReason { get; set; }
    public string? StaffNotes { get; set; }
}
