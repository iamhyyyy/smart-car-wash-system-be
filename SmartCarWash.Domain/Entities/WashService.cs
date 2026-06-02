using SmartCarWash.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCarWash.Domain.Entities;

[Table("Wash_Service")]
public class WashService : BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required, Column(TypeName = "decimal(10,2)")]
    public decimal BasePrice { get; set; }

    public int EstimatedDurationMinutes { get; set; }
    public int PointsPerTransaction { get; set; } = 10;
    public bool IsActive { get; set; } = true;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
