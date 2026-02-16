using Application.Dto.AI;
using Domain.Models.AI;

namespace Application.Interfaces;

public interface IImageProcessingService
{
    Task<ImageUploadResponseDto> UploadImageAsync(UploadImageDto dto);
    Task<ModelInput?> GetImageByIdAsync(int id);
    Task<IEnumerable<ModelInput>> GetImagesByPatientIdAsync(int patientId);
    Task<bool> DeleteImageAsync(int id);
}
