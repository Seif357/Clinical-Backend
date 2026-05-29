namespace Application.Dto.AI;

public record ModelInputDto(
    int             Id,
    string          HistopathologyImagePath,
    string          OriginalFileName,
    long            FileSizeBytes,
    int?            PatientId,
    string?         Notes,
    DateTime        UploadedAt,
    string          Status,
    ModelOutputDto? Output
);