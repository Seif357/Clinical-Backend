namespace Application.DTOs;

public class Result
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }

    public override string ToString()
    {
        return $"{Success}\nMessage: {Message}, Data: {Data}";
    }
}