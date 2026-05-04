namespace Application.Dto.Schedule;

public record RescheduleDto(
    int OldSlotId,
    int NewSlotId,
    string? PatientNotes
);