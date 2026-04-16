using Domain.Models.Communication;
using Microsoft.AspNetCore.Http;

namespace Application.Dto.Communication;

public record CreatePatientRequestDto
(
     string DoctorId,
     string Subject ,
     string Message,
     RequestImportance Importance,
     List<DateOnly>? AppointmentRequestedDates,
     List<IFormFile>? Images                   
);