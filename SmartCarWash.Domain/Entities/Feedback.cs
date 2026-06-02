using SmartCarWash.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCarWash.Domain.Entities;

[Table("Feedback")]
public class Feedback : BaseEntity
{
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    public string? Comment { get; set; }

    [ForeignKey(nameof(BookingId))]
    public Booking Booking { get; set; } = null!;

    [ForeignKey(nameof(CustomerId))]
    public CustomerProfile Customer { get; set; } = null!;
}
