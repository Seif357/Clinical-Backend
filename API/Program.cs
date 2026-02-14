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
        var app = builder.Build();
        app.MapOpenApi();

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