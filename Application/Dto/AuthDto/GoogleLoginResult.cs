namespace Application.Dto.AuthDto;

public class GoogleLoginResult : AuthResult
{
    public bool RequiresRegistration { get; set; }
}