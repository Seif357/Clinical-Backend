using Domain.Models.Communication;
using Microsoft.AspNetCore.Http;
 
namespace Application.Dto.Communication;
 
public record UpdatePatientRequestDto
(
string? Subject,
string? Message,
RequestImportance? Importance,
List<DateOnly>? AppointmentRequestedDates,
List<IFormFile>? NewImages,
List<int>? ImageIdsToRemove
);