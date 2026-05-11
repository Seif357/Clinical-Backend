namespace Application.Dto.Schedule;

public record AddDoctorNoteDto(
    int SlotId,
    string Note
);