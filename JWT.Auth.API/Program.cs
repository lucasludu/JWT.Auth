using JWT.Auth.Data;
using JWT.Auth.Models.Shared.Settings;
using JWT.Auth.Services.Interfaces;
using JWT.Auth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tokenSettings = builder.Configuration.GetSection(nameof(TokenSettings)).Get<TokenSettings>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = tokenSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = tokenSettings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.SecretKey)),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

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

app.UseAuthentication();  // Añadir esto para asegurarse que la autenticación JWT funciona correctamente
app.UseAuthorization();

app.MapControllers();

app.Run();
