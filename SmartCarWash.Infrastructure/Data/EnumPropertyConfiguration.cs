using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCarWash.Domain.Enums;

namespace SmartCarWash.Infrastructure.Data;

internal static class EnumPropertyConfiguration
{
    public static PropertyBuilder<TEnum> HasDbEnumConversion<TEnum>(this PropertyBuilder<TEnum> property, int maxLength)
        where TEnum : struct, Enum
    {
        return property
            .HasConversion(
                v => v.ToDbString(),
                v => EnumExtensions.ParseDbString<TEnum>(v))
            .HasMaxLength(maxLength);
    }
}
