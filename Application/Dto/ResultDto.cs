using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.DTOs;

public class Result : IActionResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }

    public override string ToString()
    {
        return $"{Success}\nMessage: {Message}, Data: {Data}";
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.StatusCode = 200;
        await context.HttpContext.Response.WriteAsync(Message);
        
    }
}