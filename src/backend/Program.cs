using backend;
using backend.Common;
using backend.Data;
using FastEndpoints;
using FastEndpoints.Swagger;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddTrackrApplication(builder.Configuration);

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    builder.Services
        .AddFastEndpoints()
        .SwaggerDocument(o =>
        {
            o.ShortSchemaNames = true;
            o.AutoTagPathSegmentIndex = 0;
            o.DocumentSettings = s =>
            {
                s.DocumentName = "v1";
                s.Title = "Trackr API";
                s.Version = "v1";
            };
        });

    builder.Services.AddValidatorsFromAssemblyContaining<AssemblyMarker>();
    builder.Services.AddProblemDetails();

    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod());
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        if (app.Configuration.GetValue("SeedData", false))
        {
            await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync();
        }
    }

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();
    app.UseCors();

    app.UseFastEndpoints(config =>
        {
            config.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
        })
        .UseSwaggerGen(config =>
        {
            config.Path = "/openapi/{documentName}.json";
        });

    if (app.Environment.IsDevelopment())
    {
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("Trackr API");
            options.DarkMode = true;
            options.Theme = ScalarTheme.BluePlanet;
        });
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
