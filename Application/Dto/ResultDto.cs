using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


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
        var response = context.HttpContext.Response;
        response.ContentType = "application/json";
        response.StatusCode = Success ? 200 : 400;

        var payload = new { Success, Message, Data };

        await response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}