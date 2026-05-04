namespace Domain.Models.Schedule;

public class Schedule : ParentEntity
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public ICollection<ScheduleSlot> ScheduleSlots { get; set; } = new List<ScheduleSlot>();
}