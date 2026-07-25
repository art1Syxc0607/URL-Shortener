

using Data;
using Data.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System.IO;
using WebApi.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Extension(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Инициализация при старте
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync(); // или EnsureCreated()
}

app.MapPost("/create", async (CreateShortUrlDto dto, AppDbContext dbContext, HttpContext context) =>
{
    var scheme = context.Request.Scheme;            // https
    var host = context.Request.Host.Value;          // localhost:5001
    //var path = context.Request.Path;                // /info

    var baseUrl = $"{scheme}://{host}";

    if (await dbContext.Ulrs.AnyAsync(u => u.Pseudonym == dto.Pseudonym)) throw new
        Exception("This Pseudonym is already taken");

    var url = new Url
    {
        OriginalFullUrl = dto.OriginalUrl,
        NewFullUrl = baseUrl + $"/{dto.Pseudonym}", // fullUrl 
        Pseudonym = dto.Pseudonym,
        Password = dto.Password
    };

    await dbContext.Ulrs.AddAsync(url);
    await dbContext.SaveChangesAsync();

    return Results.Ok(url.NewFullUrl);
}
);

app.MapGet("/{newPath}", async (string newPath, AppDbContext dbContext, HttpContext context) =>
{
    var scheme = context.Request.Scheme;            // https
    var host = context.Request.Host.Value;          // localhost:5001
    //var path = context.Request.Path;                // /info

    var baseUrl = $"{scheme}://{host}";

    var url = await dbContext.Ulrs.FirstOrDefaultAsync(u => u.NewFullUrl == baseUrl
    + $"/{newPath}");

    if (url == null) throw new Exception("The url not found");

    if (url.Password != null) throw new Exception("The url requires a password");

    return Results.Redirect($"{url.OriginalFullUrl}");

});

app.MapPost("/password", async (GotoOriginalUrlWithPassword dto, AppDbContext dbContext, HttpContext context) =>
{
    var scheme = context.Request.Scheme;            // https
    var host = context.Request.Host.Value;          // localhost:5001
    //var path = context.Request.Path;                // /info

    var baseUrl = $"{scheme}://{host}";

    var url = await dbContext.Ulrs.FirstOrDefaultAsync(u => u.NewFullUrl == dto.NewFullUrl);

    if (url == null) throw new Exception("The url not found");

    if (url.Password != dto.Password) throw new Exception("Wrong password");

    return Results.Redirect($"{url.OriginalFullUrl}");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
