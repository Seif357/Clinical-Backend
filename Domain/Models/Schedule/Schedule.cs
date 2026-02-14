namespace Domain.Models.Schedule;

public class Schedule : ParentEntity
{
    public int DoctorId { get; set; }
    public ICollection<ScheduleSlot> ScheduleSlots { get; set; }
}