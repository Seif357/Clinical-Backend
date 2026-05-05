namespace Domain.Models.Schedule;

public class ScheduleSlot : ParentEntity
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int ScheduleId { get; set; }
    public int? PatientId { get; set; }
    public string? PatientNotes { get; set; }
    public string? DoctorNotes { get; set; }
    public string? CancellationReason { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Available;
    public DateTime? BookedAt { get; set; }
    public bool IsBooked => PatientId != null;
    public Schedule Schedule { get; set; } = null!;
    public Patient? Patient { get; set; }
}