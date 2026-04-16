using Domain.Models.Communication;
 
namespace Application.Dto.Communication;

public class DoctorRequestSummaryDto
(
 int Id ,
 string Subject ,               
 string MessagePreview,
 RequestImportance Importance,
 RequestType RequestType,
 string PatientId ,
 int ResponseCount,
DateTime CreatedAt 
);
