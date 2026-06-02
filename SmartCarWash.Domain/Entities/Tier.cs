using SmartCarWash.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCarWash.Domain.Entities;

[Table("Tier")]
public class Tier : BaseEntity
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    public int MinPointsRequired { get; set; } = 0;
    public int BookingWindowDays { get; set; }
    public int PriorityLevel { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal PointMultiplier { get; set; } = 1.0m;
    public string? PerksDescription { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CustomerProfile> CustomerProfiles { get; set; } = new List<CustomerProfile>();
    public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}
