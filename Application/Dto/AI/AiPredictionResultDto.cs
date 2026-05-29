using System.Text.Json.Serialization;

namespace Application.Dto.AI;

public record AiPredictionResultDto(
    [property: JsonPropertyName("label")]       string Label,
    [property: JsonPropertyName("probability")] float  Probability
);