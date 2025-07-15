using Microsoft.OpenApi.Models;
using TrueVote.Contexts;
using Microsoft.EntityFrameworkCore;
using TrueVote.Interfaces;
using TrueVote.Models;
using TrueVote.Service;
using TrueVote.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Serilog.Filters;
using TrueVote.Misc;
using AspNetCoreRateLimit;
using TrueVote.Hubs;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Clinic API", Version = "v1" });
    opt.EnableAnnotations();
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});






builder.Services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                    opts.JsonSerializerOptions.WriteIndented = true;
                });

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;

    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; 
    options.SubstituteApiVersionInUrl = true;
});

// var vaultUrl = builder.Configuration["AzureKeyVault:VaultUrl"];
// var secretClient = new SecretClient(new Uri(vaultUrl), new DefaultAzureCredential());


// KeyVaultSecret dbSecret = await secretClient.GetSecretAsync("DefaultConnection");
// KeyVaultSecret adminSecret = await secretClient.GetSecretAsync("SecretAdminKey");
// KeyVaultSecret jwtSecret = await secretClient.GetSecretAsync("JwtTokenKey");

// builder.Configuration["ConnectionStrings:DefaultConnection"] = dbSecret.Value;
// builder.Configuration["AdminSettings:SecretAdminKey"] = adminSecret.Value;
// builder.Configuration["Keys:JwtTokenKey"] = jwtSecret.Value;

builder.Services.AddDbContext<AppDbContext>(opts =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHttpContextAccessor();

#region Repositories
builder.Services.AddTransient<IRepository<Guid, Moderator>, ModeratorRepository>();
builder.Services.AddTransient<IRepository<string, User>, UserRepository>();
builder.Services.AddTransient<IRepository<Guid, Voter>, VoterRepository>();
builder.Services.AddTransient<IRepository<Guid, Admin>, AdminRepository>();
builder.Services.AddTransient<IRepository<Guid, RefreshToken>, RefreshTokenRepository>();
builder.Services.AddTransient<IRepository<Guid, Poll>, PollRepository>();
builder.Services.AddTransient<IRepository<Guid, PollOption>, PollOptionRepository>();
builder.Services.AddTransient<IRepository<Guid, VoterCheck>, VoterCheckRepository>();
builder.Services.AddTransient<IRepository<Guid, PollVote>, PollVoteRepository>();
builder.Services.AddTransient<IRepository<Guid, AuditLog>, AuditRepository>();
builder.Services.AddTransient<IRepository<string, VoterEmail>, VoterEmailRepository>();
builder.Services.AddTransient<IRepository<Guid, PollFile>, PollFileRepository>();
builder.Services.AddTransient<IRepository<Guid, Message>, MesssageRepository>();
builder.Services.AddTransient<IRepository<Guid, UserMessage>, UserMesssageRepository>();

#endregion

#region Services
builder.Services.AddTransient<IModeratorService, ModeratorService>();
builder.Services.AddTransient<IEncryptionService, EncryptionService>();
builder.Services.AddTransient<IVoterService, VoterService>();
builder.Services.AddTransient<IAdminService, AdminService>();
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
builder.Services.AddTransient<IPollService, PollService>();
builder.Services.AddTransient<IPollFileService, PollFileService>();
builder.Services.AddTransient<IVoteService, VoteService>();
builder.Services.AddTransient<IAuditService, AuditService>();
builder.Services.AddTransient<IMessageService, MessageService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IKeyVaultService, KeyVaultService>();
#endregion

#region AuthenticationFilter
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Keys:JwtTokenKey"]))
                    };
                });
#endregion

#region Logging
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .WriteTo.Console()
        .WriteTo.Logger(lc => lc
            .Filter.ByExcluding(Matching.WithProperty<bool>("IsAudit", p => p))
            .WriteTo.File("Logs/general-log-.txt", rollingInterval: RollingInterval.Day))
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(Matching.WithProperty<bool>("IsAudit", p => p))
            .WriteTo.File("Logs/audit-log-.txt", rollingInterval: RollingInterval.Day));
});

builder.Services.AddTransient<IAuditLogger, AuditLogger>();
#endregion

#region RateLimiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();

builder.Services.AddTransient<IClientResolveContributor, CustomClientResolveContributor>();
builder.Services.AddTransient<IRateLimitConfiguration, RateLimitConfiguration>();
#endregion


// builder.Services.AddCors(options =>
// {
//     options.AddDefaultPolicy(policy =>
//     {
//         policy.WithOrigins("http://127.0.0.1:5500")
//               .AllowAnyHeader()
//               .AllowAnyMethod()
//               .AllowCredentials();
//     });
// });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200")  
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAngularClient", policy =>
//     {
//         policy.WithOrigins("https://tharunstorageaccount.z13.web.core.windows.net'")  
//               .AllowAnyHeader()
//               .AllowAnyMethod()
//               .AllowCredentials(); 
//     });
// });

builder.Services.AddSignalR();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors();
app.UseCors("AllowAngularClient");

app.UseAuthentication();
app.UseAuthorization();
app.UseClientRateLimiting();

app.MapControllers();
app.MapHub<MessageHub>("/messageHub");

app.Run();
