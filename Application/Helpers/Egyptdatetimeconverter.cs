using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Helpers;

/// <summary>
/// Custom <see cref="DateTime"/> converter for System.Text.Json.
///
/// Problem it solves
/// -----------------
/// The default converter converts a timezone-aware string like "2026-06-16T09:00:00+03:00"
/// to UTC (06:00:00) before storing — causing a 3-hour shift for Egypt clients.
/// Strings without any offset ("09:00:00") are left as Unspecified and stored correctly.
///
/// How this fixes it
/// -----------------
/// On READ  : If the string carries an explicit timezone indicator (Z or ±HH:mm),
///            the UTC instant is converted to Egypt local time.
///            Bare strings (no offset) are treated as Egypt local time directly.
///
///            "09:00:00+03:00" → 09:00:00   (UTC 06:00 → Egypt 09:00) ✓
///            "06:00:00Z"      → 09:00:00   (UTC 06:00 → Egypt 09:00) ✓
///            "09:00:00"       → 09:00:00   (bare — already Egypt local) ✓
///
/// On WRITE : Values are emitted without an offset suffix so the client always
///            receives Egypt wall-clock time as-is.
/// </summary>
public sealed class EgyptDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var raw = reader.GetString()!;

        if (DateTimeOffset.TryParse(raw, out var dto))
        {
            // If the raw string explicitly carries a timezone (Z, +HH:mm, or -HH:mm),
            // convert the UTC instant to Egypt local time so stored values are always
            // Egypt wall-clock regardless of what the client sent.
            // Bare strings (no offset indicator) are already Egypt local — use as-is.
            if (HasExplicitOffset(raw))
            {
                var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(dto.UtcDateTime, EgyptTimeHelper.EgyptTz);
                return DateTime.SpecifyKind(egyptTime, DateTimeKind.Unspecified);
            }

            return DateTime.SpecifyKind(dto.DateTime, DateTimeKind.Unspecified);
        }

        return DateTime.SpecifyKind(DateTime.Parse(raw), DateTimeKind.Unspecified);
    }

    /// <summary>
    /// Returns true when the ISO-8601 string contains an explicit UTC/offset indicator:
    /// a trailing 'Z', a '+' after the time component, or a '-' after the time component
    /// (distinct from the date-separator dashes which appear before the 'T').
    /// </summary>
    private static bool HasExplicitOffset(string raw)
    {
        if (raw.EndsWith("Z", StringComparison.OrdinalIgnoreCase)) return true;
        var tIdx = raw.IndexOf('T');
        if (tIdx < 0) return false;
        if (raw.IndexOf('+', tIdx) >= 0) return true;
        // A timezone '-' comes after the seconds digits; date-separator '-' are before 'T'.
        return raw.LastIndexOf('-') > tIdx + 4;
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options)
    {
        // Emit without offset so the client always sees Egypt local time as-is.
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
}