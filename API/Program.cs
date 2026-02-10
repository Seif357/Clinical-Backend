using System.Reflection;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Scalar.AspNetCore;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddOpenApi();
            var app = builder.Build();
            app.MapOpenApi();

            app.Map("/", () => Results.Redirect("/scalar/v1", permanent: true));
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
}
