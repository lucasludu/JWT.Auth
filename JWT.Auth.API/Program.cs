using JWT.Auth.Data;
using JWT.Auth.Models.Shared.Settings;
using JWT.Auth.Services;
using JWT.Auth.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<Context>(Options =>
{
    Options.UseSqlServer(builder.Configuration.GetConnectionString("connectionJwt"),
        sqlOptions => sqlOptions.MigrationsAssembly("JWT.Auth.Data"));
});

builder.Services.AddScoped<IUserServices, UserServices>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .WithOrigins("https://localhost:7089") // URL de tu aplicación Blazor WebAssembly
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); // Permite las credenciales si son necesarias
});

builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection(nameof(TokenSettings)));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
