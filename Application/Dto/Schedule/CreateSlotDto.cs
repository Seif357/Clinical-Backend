namespace Application.Dto.Schedule;

public record CreateSlotDto(
    DateTime StartTime,
    DateTime EndTime
);