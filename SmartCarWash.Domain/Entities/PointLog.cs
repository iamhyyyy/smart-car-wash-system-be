using SmartCarWash.Domain.Common;
using SmartCarWash.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCarWash.Domain.Entities;

[Table("Point_Log")]
public class PointLog : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid? BookingId { get; set; }
    public int PointsChanged { get; set; }

    public PointTransactionType TransactionType { get; set; }

    public int BalanceAfter { get; set; }

    [MaxLength(255)]
    public string? Note { get; set; }

    public DateTime? ExpiresAt { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public CustomerProfile Customer { get; set; } = null!;

    [ForeignKey(nameof(BookingId))]
    public Booking? Booking { get; set; }
}
