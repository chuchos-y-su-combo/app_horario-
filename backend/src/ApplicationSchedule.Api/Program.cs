using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Application.Services;
using ApplicationSchedule.Infrastructure.Data;
using ApplicationSchedule.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

// APP_CONNECTION_STRING is set by Electron (main.cjs) so the DB lives in the
// user-writable AppData folder rather than next to the executable.
// Falls back to appsettings.json for regular development usage.
string connectionString =
    Environment.GetEnvironmentVariable("APP_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión DefaultConnection.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(connectionString);
});
var jwtSettings = builder.Configuration.GetSection("Jwt");

var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("Falta la SecretKey del Jwt");
var issuer = jwtSettings["Issuer"] ?? "ApplicationSchedule";
var audience = jwtSettings["Audience"] ?? "ApplicationScheduleClients";

var keyBytes = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

        ValidateIssuer = true,
        ValidIssuer = issuer,

        ValidateAudience = true,
        ValidAudience = audience,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<IAsignaturaService, AsignaturaService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IProfesorService, ProfesorService>();
builder.Services.AddScoped<IAsignacionService, AsignacionService>();
builder.Services.AddScoped<ICurriculoDocenteService, CurriculoDocenteService>();
builder.Services.AddScoped<IGeneradorHorarioService, GeneradorHorarioService>();
builder.Services.AddScoped<ICalendarioSemanalService, CalendarioSemanalService>();

builder.Services.AddScoped<IHorarioExportService, HorarioExportService>();

builder.Services.AddScoped<IReporteCargaService, ReporteCargaService>();
builder.Services.AddScoped<IConflictoAsignacionService, ConflictoAsignacionService>();
builder.Services.AddScoped<IBloqueoFranjaAsignaturaService, BloqueoFranjaAsignaturaService>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ApplicationSchedule.Api",
        Version = "v1",
        Description = "API para la gestión de horarios académicos, usuarios, profesores y asignaturas."
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendLocal", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors("FrontendLocal");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Crea el esquema y aplica el seed de HasData() si la BD no existe.
    context.Database.EnsureCreated();

    // ── Garantizar usuario administrador ────────────────────────────────────
    // Se ejecuta en cada arranque para corregir instalaciones que tengan
    // un hash obsoleto (p.ej., creadas con una versión anterior del instalador).
    // Hash BCrypt de "admin123" con cost=11.
    const string AdminId   = "11111111-1111-1111-1111-111111111111";
    const string AdminEmail = "admin@universidad.edu";
    const string AdminHash  = "$2a$11$hx92ucPuKW8dEX/Zc3/XqeEhVT8QG.iUVbIIscRANbx7Ynr5PZy7m";

    var admin = await context.Usuarios.FindAsync(AdminId);

    if (admin is null)
    {
        // Primera ejecución o DB corrompida: crear el registro admin.
        context.Usuarios.Add(new ApplicationSchedule.Domain.Entities.Usuario
        {
            IdUsuario      = AdminId,
            IdRol          = 1,
            Correo         = AdminEmail,
            PasswordHash   = AdminHash,
            NombreCompleto = "Administrador Principal",
        });
        await context.SaveChangesAsync();
    }
    else if (!BCrypt.Net.BCrypt.Verify("admin123", admin.PasswordHash))
    {
        // Hash incorrecto (instalación con binario antiguo): corregir.
        admin.PasswordHash = AdminHash;
        admin.Correo       = AdminEmail;
        await context.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ApplicationSchedule.Api v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors("FrontendLocal");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();