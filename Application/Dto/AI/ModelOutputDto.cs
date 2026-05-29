namespace Application.Dto.AI;

public record ModelOutputDto(
    int      ModelInputId,
    string   Classification,
    float    Confidence,
    DateTime ProcessedAt
);