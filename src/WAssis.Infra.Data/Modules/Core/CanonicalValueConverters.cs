using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace WAssis.Infra.Data.Modules.Core;

internal static class CanonicalValueConverters
{
    // The unauthenticated query-filter sentinel maps to an impossible empty tenant.
    public static readonly ValueConverter<string, Guid> Uuid = new(
        value => value == "__missing_authenticated_tenant__" ? Guid.Empty : Guid.Parse(value),
        value => value.ToString());

    public static readonly ValueConverter<string?, Guid> NullableUuid = new(
        value => Guid.Parse(value!), value => value.ToString());

    public static readonly ValueConverter<DateTime, DateOnly> CalendarDate = new(
        value => DateOnly.FromDateTime(value),
        value => DateTime.SpecifyKind(value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc));
}
