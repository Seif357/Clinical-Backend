using Application.DTOs;

namespace Application.Dto.AuthDto;

public class Result<T> : Result
{
    public new T? Data
    {
        get => (T?)base.Data;
        set => base.Data = value;
    }
}