namespace Application.Helpers;

/// <summary>
/// Egypt local-time helpers (Africa/Cairo — UTC+2 winter / UTC+3 summer DST).
///
/// Storage strategy: all slot DateTimes are stored as Egypt local time (DateTimeKind.Unspecified).
/// The <see cref="EgyptDateTimeConverter"/> ensures incoming offsets are stripped on input,
/// and <see cref="NowEgypt"/> gives a comparable "now" for past/future checks.
/// </summary>
public static class EgyptTimeHelper
{
    internal static readonly TimeZoneInfo EgyptTz =
        TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    /// <summary>Current Egypt local time — use instead of DateTime.UtcNow when comparing
    /// against slot StartTime / EndTime values stored as Egypt local.</summary>
    public static DateTime NowEgypt =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EgyptTz);
}