public class UnifiedResponseDto
{
    public int Id { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }

    public string SenderType { get; set; } // Doctor / Patient

    public ICollection<DateTime?>? AppointmentSchedule { get; set; }

    public string? Subject { get; set; }
    public ICollection<string>? Images { get; set; }
    
}
