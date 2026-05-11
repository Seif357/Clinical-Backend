namespace Application.Dto.Schedule;

public record GenerateSlotsDto(
    DateTime BlockStart,
    DateTime BlockEnd,
    int SlotDurationMinutes 
);