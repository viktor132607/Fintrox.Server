using System.Text.Json;

namespace Fintrox.Modules.Consolidation.Domain;

internal static class DomainRules
{
    public static Guid Id(Guid value, string name)
    {
        if (value == Guid.Empty) throw new ArgumentException("An identifier is required.", name);
        return value;
    }

    public static string Text(string value, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        var result = value.Trim();
        if (result.Length > 2000) throw new ArgumentException("Text must not exceed 2000 characters.", name);
        return result;
    }

    public static string Currency(string value, string name) => Code(value, name, 3);
    public static string Country(string value, string name) => Code(value, name, 2);
    private static string Code(string value, string name, int length)
    {
        var result = Text(value, name).ToUpperInvariant();
        if (result.Length != length || result.Any(c => c < 'A' || c > 'Z'))
            throw new ArgumentException("A fixed-length ASCII alphabetic code is required.", name);
        return result;
    }

    public static string Hash(string value, string name)
    {
        var result = Text(value, name);
        if (result.Length != 64 || result.Any(c => !Uri.IsHexDigit(c)))
            throw new ArgumentException("A SHA-256 hexadecimal digest is required.", name);
        return result.ToUpperInvariant();
    }

    public static string Json(string value, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        if (value.Length > 65536) throw new ArgumentException("JSON must not exceed 65536 characters.", name);
        try
        {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("A JSON object is required.", name);
        }
        catch (JsonException exception) { throw new ArgumentException("Invalid JSON.", name, exception); }
        return value;
    }

    public static DateOnly Date(DateOnly value, string name)
    {
        if (value == default) throw new ArgumentException("A date is required.", name);
        return value;
    }

    public static DateTimeOffset Instant(DateTimeOffset value, string name)
    {
        if (value == default) throw new ArgumentException("A timestamp is required.", name);
        return value.ToUniversalTime();
    }

    public static decimal Positive(decimal value, string name) { ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, name); return value; }
    public static decimal Nonnegative(decimal value, string name) { ArgumentOutOfRangeException.ThrowIfNegative(value, name); return value; }
    public static decimal Nonzero(decimal value, string name) { if (value == 0) throw new ArgumentOutOfRangeException(name); return value; }
    public static int PositiveInt(int value, string name) { ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, name); return value; }
    public static int NonnegativeInt(int value, string name) { ArgumentOutOfRangeException.ThrowIfNegative(value, name); return value; }
    public static long NonnegativeLong(long value, string name) { ArgumentOutOfRangeException.ThrowIfNegative(value, name); return value; }
    public static decimal Percent(decimal value, string name) { ArgumentOutOfRangeException.ThrowIfNegative(value, name); ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 100m, name); return value; }
    public static decimal Hours(decimal value, string name) { Positive(value, name); ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 24m, name); return value; }
    public static int Scale(int value, string name) { NonnegativeInt(value, name); ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 8, name); return value; }
    public static int Level(int value, string name) { PositiveInt(value, name); ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 5, name); return value; }
    public static T Defined<T>(T value, string name) where T : struct, Enum
    {
        if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(name);
        return value;
    }
}
