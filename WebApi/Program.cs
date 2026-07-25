using Data;
using Data.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebApi.DTOs;

var builder = WebApplication.CreateBuilder(args);

// 1. Добавляем CORS сервисы
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        //  Для разработки разрешаем все (в продакшене ограничьте доменом)
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Добавляем сервисы
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрируем DbContext через ваш Extension метод
builder.Services.Extension(builder.Configuration);

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Создаем базу данных при старте (АСИНХРОННО!)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // EnsureCreatedAsync требует await. Обернем в задачу
    await dbContext.Database.EnsureCreatedAsync();
}

// ===========================================
// 1. СОЗДАНИЕ КОРОТКОЙ ССЫЛКИ (POST)
// ===========================================
app.MapPost("/create", async (CreateShortUrlDto dto, AppDbContext dbContext, HttpContext context) =>
{
    // 1. Проверка на существование псевдонима
    if (await dbContext.Ulrs.AnyAsync(u => u.Pseudonym == dto.Pseudonym))
    {
        // Возвращаем 400 (Bad Request), а не выбрасываем исключение
        return Results.BadRequest(new { Error = "This Pseudonym is already taken" });
    }

    // 2. Создаем сущность
    var url = new Url
    {
        OriginalFullUrl = dto.OriginalUrl,
        Pseudonym = dto.Pseudonym,
        Password = dto.Password
    };

    await dbContext.Ulrs.AddAsync(url);
    await dbContext.SaveChangesAsync();

    // 3. Формируем полную ссылку для ответа клиенту
    var scheme = context.Request.Scheme;
    var host = context.Request.Host.Value;
    var fullShortUrl = $"{scheme}://{host}/{dto.Pseudonym}";

    // Возвращаем 200 OK с данными
    return Results.Ok(new
    {
        Message = "Short URL created!",
        ShortUrl = fullShortUrl,
        Pseudonym = dto.Pseudonym
    });
});

// ===========================================
// 2. РЕДИРЕКТ ПО ПСЕВДОНИМУ (GET)
// ===========================================
app.MapGet("/{pseudonym}", async (string pseudonym, AppDbContext dbContext) =>
{
    // Ищем по псевдониму (ПРАВИЛЬНО!)
    var url = await dbContext.Ulrs.FirstOrDefaultAsync(u => u.Pseudonym == pseudonym);

    if (url == null)
    {
        return Results.NotFound(new { Error = "Short URL not found" });
    }

    // ЕСЛИ ЕСТЬ ПАРОЛЬ — возвращаем статус 403, чтобы клиент знал, что нужно ввести пароль
    if (!string.IsNullOrEmpty(url.Password))
    {
        return Results.Json(new { RequiresPassword = true, Pseudonym = pseudonym }, statusCode: 403);
    }

    // Если пароля нет — редиректим
    return Results.Redirect(url.OriginalFullUrl);
});

// ===========================================
// 3. ОБРАБОТКА ПАРОЛЯ (POST)
// ===========================================
app.MapPost("/password", async (GotoOriginalUrlWithPassword dto, AppDbContext dbContext) =>
{
    var url = await dbContext.Ulrs.FirstOrDefaultAsync(u => u.Pseudonym == dto.Pseudonym);

    if (url == null)
    {
        return Results.NotFound(new { Error = "Short URL not found" });
    }

    // Проверяем пароль
    if (url.Password != dto.Password)
    {
        return Results.BadRequest(new { Error = "Wrong password" });
    }

    // Если пароль верный — редиректим
    return Results.Redirect(url.OriginalFullUrl);
});

// ===========================================
// 4. Middleware (стандартные)
// ===========================================
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
