using B2BStore.TechnicalSettings.Api.Catalog;
using B2BStore.TechnicalSettings.Api.Models;
using B2BStore.TechnicalSettings.Api.Repositories;
using B2BStore.TechnicalSettings.Api.Services;
using Microsoft.AspNetCore.Diagnostics;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---- services (Spring Boot karşılığı: @Bean / @Service / @Repository kayıtları)
var catalogPath = Path.Combine(AppContext.BaseDirectory, "Catalog", "settings-catalog.json");
builder.Services.AddSingleton(SettingsCatalog.Load(catalogPath));
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();  // veri erişim katmanı
builder.Services.AddScoped<ISettingsService, SettingsService>();        // iş kuralları katmanı
builder.Services.AddControllers();        // Controllers/ klasöründeki endpoint'ler
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ---- global hata yönetimi (Spring karşılığı: @ControllerAdvice)
// Her hata RFC 7807 ProblemDetails formatında döner.
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var (status, title, detail) = exception switch
    {
        SettingsNotFoundException e => (StatusCodes.Status404NotFound, "Kayıt bulunamadı", e.Message),
        Microsoft.Data.SqlClient.SqlException => (StatusCodes.Status503ServiceUnavailable,
            "Veritabanına ulaşılamadı", "Veritabanı bağlantısını ve SQL Server'ın çalıştığını kontrol edin."),
        _ => (StatusCodes.Status500InternalServerError, "Beklenmeyen hata", "İşlem tamamlanamadı, değişiklikler kaydedilmedi.")
    };
    if (status == 500 && exception is not null)
        app.Logger.LogError(exception, "Beklenmeyen hata");
    await Results.Problem(title: title, detail: detail, statusCode: status).ExecuteAsync(context);
}));

app.UseCors();
app.UseDefaultFiles();   // production'da derlenmiş Vue uygulamasını wwwroot'tan sunar
app.UseStaticFiles();

app.MapOpenApi();
app.MapScalarApiReference(); // API dokümantasyonu: /scalar

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
