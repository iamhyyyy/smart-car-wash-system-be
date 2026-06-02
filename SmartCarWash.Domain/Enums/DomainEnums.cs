namespace SmartCarWash.Domain.Enums;

public enum PromoType
{
    Discount,
    FreeWash,
    Addon,
    PointBonus
}

public enum PointTransactionType
{
    Earn,
    Redeem,
    Expire,
    Bonus,
    Adjustment
}

public enum VehicleType
{
    Motorbike,
    Scooter,
    Other
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    [DbEnumValue("In-Progress")]
    InProgress,
    Completed,
    Cancelled,
    [DbEnumValue("No-Show")]
    NoShow
}

public enum PaymentMethod
{
    Cash,
    Transfer,
    Points
}
