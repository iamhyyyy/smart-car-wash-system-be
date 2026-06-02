using SmartCarWash.Domain.Common;
using SmartCarWash.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCarWash.Domain.Entities;

[Table("Vehicle")]
public class Vehicle : BaseEntity
{
    public Guid CustomerId { get; set; }

    [Required, MaxLength(20)]
    public string LicensePlate { get; set; } = string.Empty;

    public VehicleType VehicleType { get; set; }

    [MaxLength(50)]
    public string? Brand { get; set; }

    [MaxLength(50)]
    public string? Model { get; set; }

    [MaxLength(30)]
    public string? Color { get; set; }

    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(CustomerId))]
    public CustomerProfile Customer { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
