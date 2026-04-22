using Microsoft.AspNetCore.Http;

namespace Application.Dto.Communication;

public record CreatePatientResponseDto(
int DoctorRequestId,
string Subject ,
string Message ,
List<IFormFile>? Images
);