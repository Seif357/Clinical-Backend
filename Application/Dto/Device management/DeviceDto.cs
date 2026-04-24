namespace Application.Dto.Device_management;

public class DeviceDto
{
    public int Id { get; set; }
    public string? DeviceName { get; set; }
    public string UserAgent { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime LastSeenAt { get; set; }
    public bool IsCurrentDevice { get; set; }
}