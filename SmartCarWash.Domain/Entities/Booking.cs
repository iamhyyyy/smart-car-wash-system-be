using SmartCarWash.Domain.Common;
using SmartCarWash.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCarWash.Domain.Entities;

[Table("Booking")]
public class Booking : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? PromoId { get; set; }

    public DateTime ScheduledTime { get; set; }
    public DateTime? CheckinTime { get; set; }
    public DateTime? CompletedTime { get; set; }

    [Required, Column(TypeName = "decimal(10,2)")]
    public decimal BaseAmount { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    [Required, Column(TypeName = "decimal(10,2)")]
    public decimal FinalAmount { get; set; }

    public int PointsEarned { get; set; } = 0;
    public int PointsRedeemed { get; set; } = 0;

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    [MaxLength(255)]
    public string? CancelReason { get; set; }

    [MaxLength(500)]
    public string? StaffNotes { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public CustomerProfile Customer { get; set; } = null!;

    [ForeignKey(nameof(VehicleId))]
    public Vehicle Vehicle { get; set; } = null!;

    [ForeignKey(nameof(ServiceId))]
    public WashService Service { get; set; } = null!;

    [ForeignKey(nameof(PromoId))]
    public Promotion? Promo { get; set; }

    public Feedback? Feedback { get; set; }
    public ICollection<PointLog> PointLogs { get; set; } = new List<PointLog>();
}
