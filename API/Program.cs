using Application;
using Scalar.AspNetCore;

namespace API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddInfrastructureAsync(builder.Configuration, builder.Environment);
        builder.Services.AddApplication(builder.Configuration);
        builder.Services.AddOpenApi();
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        var app = builder.Build();
        app.MapOpenApi();
        app.UseCors();
        app.Map("/", () => Results.Redirect("/scalar/v1", true));
        app.UseHttpsRedirection();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("My API Documentation")
                .WithTheme(ScalarTheme.Purple);
        });
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}