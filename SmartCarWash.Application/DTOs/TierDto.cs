namespace SmartCarWash.Application.DTOs;

public class TierDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinPointsRequired { get; set; }
    public int BookingWindowDays { get; set; }
    public int PriorityLevel { get; set; }
    public decimal PointMultiplier { get; set; }
    public string? PerksDescription { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdateBy { get; set; }
}

public class CreateTierDto
{
    public string Name { get; set; } = string.Empty;
    public int MinPointsRequired { get; set; }
    public int BookingWindowDays { get; set; }
    public int PriorityLevel { get; set; }
    public decimal PointMultiplier { get; set; } = 1.0m;
    public string? PerksDescription { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateTierDto
{
    public string Name { get; set; } = string.Empty;
    public int MinPointsRequired { get; set; }
    public int BookingWindowDays { get; set; }
    public int PriorityLevel { get; set; }
    public decimal PointMultiplier { get; set; }
    public string? PerksDescription { get; set; }
    public bool IsActive { get; set; }
}
