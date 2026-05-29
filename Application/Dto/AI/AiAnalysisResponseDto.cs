namespace Application.Dto.AI;

public record AiAnalysisResponseDto(
    int      ImageId,
    string   OriginalFileName,
    int?     PatientId,
    string   Label,
    float    Probability,
    bool     IsCancerous,
    DateTime AnalyzedAt
);