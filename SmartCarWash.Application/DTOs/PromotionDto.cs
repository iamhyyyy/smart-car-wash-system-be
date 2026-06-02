using SmartCarWash.Domain.Enums;

namespace SmartCarWash.Application.DTOs;

public class PromotionDto
{
    public Guid Id { get; set; }
    public string PromoName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? MinTierId { get; set; }
    public PromoType PromoType { get; set; }
    public int PointsCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int? MaxUsesTotal { get; set; }
    public int MaxUsesPerCustomer { get; set; }
    public int CurrentUses { get; set; }
    public Guid? CreatedBy { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdateBy { get; set; }
}

public class CreatePromotionDto
{
    public string PromoName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? MinTierId { get; set; }
    public PromoType PromoType { get; set; }
    public int PointsCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int? MaxUsesTotal { get; set; }
    public int MaxUsesPerCustomer { get; set; } = 1;
    public Guid? CreatedBy { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdatePromotionDto
{
    public string PromoName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? MinTierId { get; set; }
    public PromoType PromoType { get; set; }
    public int PointsCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int? MaxUsesTotal { get; set; }
    public int MaxUsesPerCustomer { get; set; }
    public bool IsActive { get; set; }
}
