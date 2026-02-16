using Application;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

namespace API;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Configure Serilog early in the pipeline
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: "Logs/app-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                retainedFileCountLimit: 30)
            .WriteTo.File(
                path: "Logs/errors-.txt",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Error,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                retainedFileCountLimit: 90)
            .CreateLogger();

        try
        {
            Log.Information("Starting Clinical Backend API application");

            var builder = WebApplication.CreateBuilder(args);

            // Add Serilog to the logging pipeline
            builder.Host.UseSerilog();

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
            
            // Add Serilog request logging middleware
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
                };
            });
            
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

            Log.Information("Clinical Backend API application started successfully");
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}