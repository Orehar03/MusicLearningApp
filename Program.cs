using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MusicLearningApp.Data;
using MusicLearningApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Регистрация DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

// Регистрация DbInitializer (вот чего не хватало!)
builder.Services.AddScoped<DbInitializer>();

// JWT-настройки (без appsettings.json — проще)
var jwtSettings = new JwtSettings
{
    SecretKey = "super_secret_key_for_music_app_12345",
    Issuer = "MusicLearningApp",
    Audience = "MusicLearningAppUsers"
};
builder.Services.AddSingleton(jwtSettings);

// Аутентификация
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// Инициализация БД при старте
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    dbInitializer.Initialize();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles(); // Обязательно для отдачи HTML/CSS/JS
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Маршруты для HTML-страниц (без них будет 404)
app.MapGet("/", () => Results.Redirect("/index.html"));
app.MapGet("/materials", () => Results.Redirect("/materials.html"));
app.MapGet("/homework", () => Results.Redirect("/homework.html"));
app.MapGet("/consultation", () => Results.Redirect("/consultation.html"));

app.Run();

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}