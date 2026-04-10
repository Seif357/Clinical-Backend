namespace Application.Dto.AuthDto;

/// <summary>
/// DTO for Google OAuth sign-in / sign-up.
/// The frontend obtains an id_token via Google Sign-In and sends it here;
/// the backend validates it server-side and issues our own JWT pair.
/// </summary>
public record GoogleLoginDto(
    /// <summary>The Google id_token returned by the Google Sign-In SDK on the client.</summary>
    string IdToken,

    /// <summary>true = register / login as Doctor; false = Patient.</summary>
    bool IsDoctor,

    /// <summary>Required when IsDoctor = true for first-time sign-up.</summary>
    string? ProfessionalPracticeLicense,

    /// <summary>Required when IsDoctor = true for first-time sign-up.</summary>
    string? IssuingAuthority
);
