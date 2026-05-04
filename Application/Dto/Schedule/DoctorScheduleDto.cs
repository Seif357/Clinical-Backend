namespace Application.Dto.Schedule;

public record DoctorScheduleDto(
    int ScheduleId,
    int DoctorId,
    string DoctorName,
    IEnumerable<ScheduleSlotDto> Slots
);