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
/// On READ  : DateTimeOffset.Parse captures the full string; .DateTime gives the local
///            wall-clock digits without any UTC conversion.
///            "09:00:00+03:00" → 09:00:00   (Unspecified, not 06:00:00)
///            "09:00:00"       → 09:00:00   (unchanged)
///            "09:00:00Z"      → 09:00:00   (treated the same — Z clients not expected)
///
/// On WRITE : Values are emitted without an offset suffix so they round-trip identically.
/// </summary>
public sealed class EgyptDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var raw = reader.GetString()!;

        // DateTimeOffset handles all ISO-8601 forms (+HH:mm, Z, or bare).
        // .DateTime returns the local portion as written — no UTC conversion.
        if (DateTimeOffset.TryParse(raw, out var dto))
            return DateTime.SpecifyKind(dto.DateTime, DateTimeKind.Unspecified);

        return DateTime.SpecifyKind(DateTime.Parse(raw), DateTimeKind.Unspecified);
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