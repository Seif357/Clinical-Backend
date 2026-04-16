using Domain.Models.Communication;
using Microsoft.AspNetCore.Http;

namespace Application.Dto.Communication;

public record CreateDoctorRequestDto(
string PatientId,
string Subject,
string Message,
RequestType RequestType,
RequestImportance Importance,
List<IFormFile>? Images
);