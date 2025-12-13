using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MusicLearningApp.Data;
using MusicLearningApp.Services;

var builder = WebApplication.CreateBuilder(args);

// БД
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

// Сервисы
builder.Services.AddScoped<DbInitializer>();
builder.Services.AddScoped<AuthService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ?? Регистрация JwtSettings через Options
builder.Services.Configure<JwtSettings>(options =>
{
    options.SecretKey = "super_secret_key_for_music_app_12345";
    options.Issuer = "MusicLearningApp";
    options.Audience = "MusicLearningAppUsers";
});

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
        ValidIssuer = "MusicLearningApp",
        ValidAudience = "MusicLearningAppUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super_secret_key_for_music_app_12345"))
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

// Правильный порядок middleware
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

// Эндпоинты
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/index.html"));
app.MapGet("/materials", () => Results.Redirect("/materials.html"));
app.MapGet("/homework", () => Results.Redirect("/homework.html"));
app.MapGet("/consultation", () => Results.Redirect("/consultation.html"));

app.Run();

// Модель настроек
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}