namespace Application.Dto.Schedule;

public record CompleteSlotDto(
    int SlotId,
    string? DoctorNotes
);