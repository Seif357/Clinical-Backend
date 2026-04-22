namespace Application.Dto.Communication;

public record UpdateDoctorResponseDto
(
   string? Message,
   List<DateTime>? AppointmentSchedule
);