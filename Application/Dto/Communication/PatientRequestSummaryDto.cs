using Domain.Models.Communication;
 
namespace Application.Dto.Communication;
 
public record PatientRequestSummaryDto
(
     int Id, 
     string Subject, 
     string MessagePreview, 
     RequestImportance Importance, 
     string DoctorId, 
     int ResponseCount, 
     DateTime CreatedAt
);