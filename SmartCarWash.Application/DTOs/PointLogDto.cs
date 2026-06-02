using SmartCarWash.Domain.Enums;

namespace SmartCarWash.Application.DTOs;

public class PointLogDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? BookingId { get; set; }
    public int PointsChanged { get; set; }
    public PointTransactionType TransactionType { get; set; }
    public int BalanceAfter { get; set; }
    public string? Note { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdateBy { get; set; }
}

public class CreatePointLogDto
{
    public Guid CustomerId { get; set; }
    public Guid? BookingId { get; set; }
    public int PointsChanged { get; set; }
    public PointTransactionType TransactionType { get; set; }
    public int BalanceAfter { get; set; }
    public string? Note { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class UpdatePointLogDto
{
    public int PointsChanged { get; set; }
    public PointTransactionType TransactionType { get; set; }
    public int BalanceAfter { get; set; }
    public string? Note { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
