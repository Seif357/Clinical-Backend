using Domain.Models.Communication;
using Microsoft.AspNetCore.Http;

namespace Application.Dto.Communication;

public record UpdateDoctorRequestDto
(
string? Subject ,
string? Message ,
RequestImportance? Importance ,
RequestType? RequestType ,
List<IFormFile>? NewImages ,
List<int>? ImageIdsToRemove 
);