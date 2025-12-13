using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MusicLearningApp.Data;
using MusicLearningApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Регистрация сервисов
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

builder.Services.AddScoped<DbInitializer>();
builder.Services.AddScoped<AuthService>();

// CORS — разрешаем все для учебного проекта
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT
var jwtSettings = new JwtSettings
{
    SecretKey = "super_secret_key_for_music_app_12345",
    Issuer = "MusicLearningApp",
    Audience = "MusicLearningAppUsers"
};
builder.Services.AddSingleton(jwtSettings);

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

var app = builder.Build();

// Инициализация БД
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    dbInitializer.Initialize();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// ?? КРИТИЧЕСКИ ВАЖНЫЙ ПОРЯДОК MIDDLEWARE:
app.UseRouting();               // 1. Сначала маршрутизация
app.UseCors("AllowAll");        // 2. Потом CORS
app.UseAuthentication();        // 3. Аутентификация
app.UseAuthorization();         // 4. Авторизация
app.UseStaticFiles();           // 5. Статические файлы

// Эндпоинты
app.MapControllers();

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