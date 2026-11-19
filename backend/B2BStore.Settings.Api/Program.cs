using B2BStore.Settings.Api.Catalog;
using B2BStore.Settings.Api.Repositories;
using B2BStore.Settings.Api.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Ayar kataloğunu uygulama açılırken bir kez oku ve her yerde kullanılabilir yap
var catalogPath = Path.Combine(AppContext.BaseDirectory, "Catalog", "settings-catalog.json");
builder.Services.AddSingleton(SettingsCatalog.Load(catalogPath));
//yazdıgımız settingcatalog sınıfının ıcındekı metodu cagırırır nesne uretır sonrasında addsıngletona verırız.
builder.Services.AddScoped<SettingsRepository>();
builder.Services.AddScoped<SettingsService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// Canlıda frontend (npm run build çıktısı) wwwroot klasöründen sunulur
app.UseDefaultFiles();   // "/" isteğinde index.html'i aç
app.UseStaticFiles();    // js, css, görseller

app.UseAuthorization();

app.MapControllers();

// /api ile başlamayan bilinmeyen adresler Vue uygulamasına gitsin
app.MapFallbackToFile("index.html");

app.Run();
