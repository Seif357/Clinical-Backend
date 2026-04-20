namespace Application.Dto.Communication;

public record CreateDoctorResponseDto(
    int PatientRequestId,
    string Message,
    List<DateTime>? AppointmentSchedule
);