using SmartCarWash.Domain.Entities;

namespace SmartCarWash.Application.DTOs;

public class CustomerProfileDto
{
    public Guid Id { get; set; }
    public Guid CurrentTierId { get; set; }
    public string? CurrentTierName { get; set; }
    public int AvailablePoints { get; set; }
    public int LifetimePoints { get; set; }
    public int TotalVisits { get; set; }
    public decimal TotalSpending { get; set; }
    public DateTime? TierUpgradedAt { get; set; }
    public DateTime LastTierReviewDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdateBy { get; set; }
}

public class CreateCustomerProfileDto
{
    public Guid Id { get; set; }
    public Guid CurrentTierId { get; set; }
}

public class UpdateCustomerProfileDto
{
    public Guid CurrentTierId { get; set; }
    public int AvailablePoints { get; set; }
    public int LifetimePoints { get; set; }
    public int TotalVisits { get; set; }
    public decimal TotalSpending { get; set; }
    public DateTime? TierUpgradedAt { get; set; }
    public DateTime LastTierReviewDate { get; set; }
}
