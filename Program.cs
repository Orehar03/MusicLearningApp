using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MusicLearningApp.Data;
using MusicLearningApp.Services;
using System.Text;

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

// КРИТИЧЕСКИ ВАЖНО: правильная регистрация JWT
var jwtKey = "super_secret_key_for_music_app_12345"; // 32+ символа
builder.Services.AddSingleton(new JwtSettings
{
    SecretKey = jwtKey,
    Issuer = "MusicLearningApp",
    Audience = "MusicLearningAppUsers"
});

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero // НЕТ отклонения времени
    };

    // Для отладки: логируем ошибки валидации токена
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52428800; // 50 MB
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Создаём БД и админа
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    dbInitializer.Initialize();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

//ПРАВИЛЬНЫЙ ПОРЯДОК MIDDLEWARE
app.UseCors("AllowAll"); // Сначала CORS
app.UseStaticFiles();    // Потом статические файлы

// Настраиваем прием файлов размером до 50 МБ
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
});

app.UseHttpsRedirection();


app.UseRouting();        // Потом маршрутизация
app.UseAuthentication(); // Потом аутентификация
app.UseAuthorization();  // И наконец авторизация

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