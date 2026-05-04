namespace Application.Dto.Schedule;

public record CancelSlotDto(
    int SlotId,
    string? Reason
);