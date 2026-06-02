using SmartCarWash.Domain.Common;
using SmartCarWash.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCarWash.Domain.Entities;

[Table("Promotion")]
public class Promotion : BaseEntity
{
    [Required, MaxLength(100)]
    public string PromoName { get; set; } = string.Empty;

    public string? Description { get; set; }
    public Guid? MinTierId { get; set; }

    public PromoType PromoType { get; set; }

    public int PointsCost { get; set; } = 0;

    [Column(TypeName = "decimal(10,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    [Column(TypeName = "decimal(5,2)")]
    public decimal DiscountPercent { get; set; } = 0;

    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int? MaxUsesTotal { get; set; }
    public int MaxUsesPerCustomer { get; set; } = 1;
    public int CurrentUses { get; set; } = 0;
    public Guid? CreatedBy { get; set; }
    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(MinTierId))]
    public Tier? MinTier { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public User? CreatedByUser { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
