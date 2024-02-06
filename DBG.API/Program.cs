#region

using System.Text;
using DBG.Infrastructure;
using DBG.Infrastructure.Clients;
using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Helper;
using DBG.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

#endregion

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging(logging =>
{
    _ = logging.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Scheme = "Bearer",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddDbContext<DatabaseContext>(
    options =>
    {
        _ = options.UseSqlServer(Environment.GetEnvironmentVariable("DbConnectionString") ??
                                 builder.Configuration.GetConnectionString("ConnectionString") ??
                                 throw new Exception("Database connection string is not set."));
    });
VaultServiceConfiguration vaultConfig = new();
builder.Configuration.Bind("Vault", vaultConfig);
vaultConfig.Token = Environment.GetEnvironmentVariable("VAULTTOKEN") ?? vaultConfig.Token ?? throw new Exception("Vault token is not set.");
vaultConfig.Address = Environment.GetEnvironmentVariable("VAULTADDR") ?? vaultConfig.Address ?? throw new Exception("Vault address is not set.");
builder.Services.AddScoped<IVaultServiceConfiguration>(_ => vaultConfig);
var pca = Environment.GetEnvironmentVariable("POSTGRESCLIENTADDR") ?? builder.Configuration["ServiceAddresses:Postgres"]
    ?? throw new Exception("Postgres worker address is not set.");
var mca = Environment.GetEnvironmentVariable("MSSQLCLIENTADDR") ?? builder.Configuration["ServiceAddresses:MSSQL"]
    ?? throw new Exception("MSSQL worker address is not set.");
var lca = Environment.GetEnvironmentVariable("LINUXCLIENTADDR") ?? builder.Configuration["ServiceAddresses:Linux"]
    ?? throw new Exception("SSH worker address is not set.");;
builder.Services.AddScoped<IPostgresClient>(_ => new PostgresClient(pca));
builder.Services.AddScoped<IMssqlClient>(_ => new MssqlClient(mca));
builder.Services.AddScoped<ILinuxClient>(_ => new LinuxClient(lca));
builder.Services.AddScoped<IPersistenceService, PersistenceService>();
builder.Services.AddScoped<IDbSystemService, DbSystemService>();
builder.Services.AddScoped<IOsSystemService, OsSystemService>();
JwtConfiguration jwtConfig = new();
builder.Configuration.Bind("Jwt", jwtConfig);
jwtConfig.Secret = Environment.GetEnvironmentVariable("JWTSECRET") ?? jwtConfig.Secret ?? throw new Exception("Jwt secret is not set.");
builder.Services.AddScoped<IJwtConfiguration>(_ => jwtConfig);
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(jwt =>
    {
        var key = Encoding.ASCII.GetBytes(jwtConfig.Secret);
        jwt.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireClaim("Role", "Admin"))
    .AddPolicy("Viewer", policy => policy.RequireClaim("Role", "Admin", "Viewer"));
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(s =>
    {
        _ = s.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseCors();
app.Run();