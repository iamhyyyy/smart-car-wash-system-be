using SmartCarWash.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCarWash.Domain.Entities;

[Table("Customer_Profile")]
public class CustomerProfile : BaseEntity
{
    public Guid CurrentTierId { get; set; }
    public int AvailablePoints { get; set; } = 0;
    public int LifetimePoints { get; set; } = 0;
    public int TotalVisits { get; set; } = 0;

    [Column(TypeName = "decimal(12,2)")]
    public decimal TotalSpending { get; set; } = 0;
    public DateTime? TierUpgradedAt { get; set; }
    public DateTime LastTierReviewDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(Id))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(CurrentTierId))]
    public Tier CurrentTier { get; set; } = null!;

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<PointLog> PointLogs { get; set; } = new List<PointLog>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
