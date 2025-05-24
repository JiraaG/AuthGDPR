using AuthGDPR.Application;
using AuthGDPR.Infrastructure.Persistance;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog.Events;
using Serilog;
using AuthGDPR.Infrastructure.Logging;
using AuthGDPR.Domain;
using AuthGDPR.Domain.Entities.Auth;
using System.IdentityModel.Tokens.Jwt;
using AuthGDPR.Application.Interfaces;
using AuthGDPR.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

// 0) Log
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    // Direttiva per i log su console
    .WriteTo.Console()
    // Log su file con rolling giornaliero
    .WriteTo.File("Logs/auditlog-.txt", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Information)
    .CreateLogger();

// JWT
// Disabilita il mapping automatico dei claim in ingresso
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);
// Imposta il percorso base e carica sempre il file specificato
builder.Configuration
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
       .AddEnvironmentVariables();          // Per eventuali variabili d'ambiente

// Se l'ambiente è Development, carica esclusivamente appsettings.Development.json
//if (builder.Environment.IsDevelopment())
//{
//    // Rimuove tutte le configurazioni predefinite
//    builder.Configuration.Sources.Clear();
//
//    // Imposta il percorso base e carica il file di sviluppo
//    builder.Configuration
//           .SetBasePath(Directory.GetCurrentDirectory())
//           .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
//           .AddEnvironmentVariables();
//}
//else
//{
//    // In altri ambienti (Production, Staging, ecc.) carica le configurazioni standard
//    builder.Configuration
//           .SetBasePath(Directory.GetCurrentDirectory())
//           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//           //.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
//           .AddEnvironmentVariables();
//}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 1) DbContexts
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(connectionString));

// 2) Configurazione di Identity senza gli endpoint API di default
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    // Configura qui le opzioni (ad es. policy password, lockout, ecc.)
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// 3) IdentityServer con EF Configuration & Operational stores
builder.Services.AddIdentityServer(options =>
{
    // Imposta la generazione statica degli audience claim per i JWT
    options.EmitStaticAudienceClaim = true;

    // Attiva i vari eventi per facilitare il debug
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
})
    .AddAspNetIdentity<ApplicationUser>()
    .AddConfigurationStore(cfg =>
    {
        cfg.ConfigureDbContext = b =>
            b.UseNpgsql(connectionString,
                sql => sql.MigrationsAssembly(typeof(Program).Assembly.FullName));
    })
    .AddOperationalStore(opt =>
    {
        opt.ConfigureDbContext = b =>
            b.UseNpgsql(connectionString,
                sql => sql.MigrationsAssembly(typeof(Program).Assembly.FullName));
        opt.EnableTokenCleanup = true;
        opt.TokenCleanupInterval = 3600;
    })
    .AddDeveloperSigningCredential();   // Per ambiente di sviluppo (non usare in produzione) = usare: certificato per la firma dei token

// 4) Configurazione di Authentication: aggiungiamo Cookie e JWT Bearer
// Leggi i percorsi dal configuration
var loginPath = builder.Configuration["Authentication:LoginPath"];
var logoutPath = builder.Configuration["Authentication:LogoutPath"];

builder.Services.AddAuthentication(options =>
{
    // Impostiamo il cookie come scheme di default:
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // Se vuoi, puoi impostare anche il DefaultChallengeScheme come quello dei cookie
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        // Nome del cookie di autenticazione
        options.Cookie.Name = "auth-gdpr-cookie";
        // Impedisce l’accesso al cookie da JavaScript (protezione XSS)
        options.Cookie.HttpOnly = true;
        // Richiede che il cookie sia trasmesso solo su HTTPS
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        // Impone che il cookie sia inviato solo in richieste di tipo same-site (protezione CSRF)
        options.Cookie.SameSite = SameSiteMode.Strict;
        // Durata massima del cookie (8 ore)
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        // Abilita la scadenza "sliding" (il cookie si rinnova ad ogni richiesta)
        options.SlidingExpiration = true;
        // Percorso della pagina di login e logout (per redirect automatici)
        //options.LoginPath = loginPath;
        //options.LogoutPath = logoutPath;
    })
    .AddJwtBearer("Bearer", options =>
    {
        // URL dell'IdentityServer che emette i token (issuer)
        options.Authority = builder.Configuration["IdentityServer:Authority"]
                            ?? "https://localhost:5001";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Verifica che il token sia stato emesso dal server atteso
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            // Richiede che il token abbia una data di scadenza
            RequireExpirationTime = true,
            // Verifica che il token sia destinato all'audience corretta
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            // Richiede che il token sia firmato
            RequireSignedTokens = true,
        };
        // Salva il token validato nel contesto dell'autenticazione
        options.SaveToken = true;
    });

// 5) DI vario
    // Caching per OTP
builder.Services.AddMemoryCache();
    // Registrazione del servizio per la gestione della crittografia delle password
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, Argon2PasswordHasher<ApplicationUser>>();
    // Registrazione del servizio RefreshTokenService
builder.Services.AddScoped<RefreshTokenService>();
    // Registra il servizio di Account
builder.Services.AddScoped<IAccountService, AccountService>();
    // Registra il servizio di AuditLog
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
    // Registra il servizio di Email
builder.Services.AddScoped<IEmailCustomSender, EmailCustomSender>();
    // Registrazione per servizio delle policy
builder.Services.AddScoped<IConsentPolicyService, ConsentPolicyService>();
    // Registrazione per servizio dei consensi utente
builder.Services.AddScoped<IUserConsentService, UserConsentService>();
    // Registrazione per servizio della gestione delle richieste personali utente
builder.Services.AddScoped<IDataSubjectRequestService, DataSubjectRequestService>();
    // Registrazione per servizio di autenticazione a 2 fattori
builder.Services.AddScoped<IOtpChallengeService, OtpChallengeService>();
    // Registra la configurazione per la pseudonimizzazione
builder.Services.Configure<PseudonymizationOptions>(builder.Configuration.GetSection("Pseudonymization"));
    // Registra il servizio di pseudonimizzazione
builder.Services.AddScoped<PseudonymizerService>();
    // Default controller
builder.Services.AddControllers();

// Configurazione dei controller con gestione globale degli errori di validazione
// Viene gestito l'errore globale generato di default dall'ApiController di ASP.NET Core
// ASP.NET Core gestisce con un copro standard (classe ValidationProblemDetails) per l'errore con codice status 400
// Con questo metodo, intercettiamo l'errore causato dallo status code 400 
// E gestiamo noi l'errore da inviare al client, con una struttura già definita ed uguale per tutti gli altri status code
/*
    L’obiettivo è garantire che ogni errore di validazione ritorni un formato uniforme e standardizzato,
    centralizzando la gestione e mantenendo il codice DRY (Don't Repeat Yourself).
 */
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var validationDetails = new ValidationProblemDetails(context.ModelState)
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.HttpContext.Request.Path
            };
            // Aggiungi il traceId nella sezione Extensions
            validationDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

            return new BadRequestObjectResult(validationDetails);
        };
    });

// 6) Swagger + OAuth2
builder.Services.AddSwaggerGen(c =>
{
    // Definizione della documentazione OpenAPI
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GDPR API v1", Version = "v1" });

    // Definizione dello schema di sicurezza OAuth2 (Authorization Code Flow)
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{builder.Configuration["IdentityServer:Authority"]}/connect/authorize"),
                TokenUrl = new Uri($"{builder.Configuration["IdentityServer:Authority"]}/connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID Connect" },
                    { "profile", "User profile" },
                    { "api1", "GDPR API" },
                    { "offline_access", "Refresh token" }
                }
            }
        }
    });

    // Definizione dello schema di sicurezza JWT Bearer per autenticazione tramite header Authorization
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    // Richiesta di sicurezza: le API richiedono uno dei due schemi (OAuth2 o Bearer)
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            new[] { "openid", "profile", "api1", "offline_access" }
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Configurazione CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocal", policy =>
    {
        // Recupera gli origin consentiti dalla configurazione (separati da ;)
        var origins = builder.Configuration["Cors:AllowedOrigins"]?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        // Consente solo gli origin specificati, tutti gli header e tutti i metodi HTTP
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Consenti invio di cookie/autenticazione cross-origin (solo se necessario)
    });
});
//builder.Services.AddCors(opt =>
//    opt.AddPolicy("AllowLocal", p =>
//        p.WithOrigins(builder.Configuration["Cors:AllowedOrigins"]?.Split(';') ?? Array.Empty<string>())
//         .AllowAnyHeader()
//         .AllowAnyMethod()
//    )
//);

var app = builder.Build();

// Seeding dei dati IdentityServer (Clients, ApiResources, ApiScopes, IdentityResources)
using (var scope = app.Services.CreateScope())
{
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("SeedData");
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    await AuthGDPR.Infrastructure.SeedData.EnsureSeedDataAsync(app, logger, env);
    logger.LogInformation("Seed completato");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GDPR API v1");
        c.OAuthClientId("swagger-ui");
        c.OAuthUsePkce();
        c.OAuth2RedirectUrl("https://localhost:7040/swagger/oauth2-redirect.html");
        c.OAuthScopeSeparator(" ");
        c.OAuthAppName("Swagger UI");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowLocal");

// Abilita il routing
app.UseRouting();

app.UseIdentityServer();

// Errori non gestiti
/*
    Il middleware configurato con UseStatusCodePages intercetta le risposte che hanno uno status code diverso dal 200 e che non hanno già un corpo di risposta. 
    In questo modo, code come 401, 403, 404 vengono riconvertite in una risposta di tipo ProblemDetails.
    Trasforma l'errore in un JSON standardizzato
 */
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    if (!response.HasStarted && response.StatusCode != StatusCodes.Status200OK)
    {
        var request = context.HttpContext.Request;

        // Costruisco un dizionario di errori con la chiave "generic"
        var errors = new Dictionary<string, string[]>
        {
            { "Generic", new [] { $"Si è verificato un errore con codice {response.StatusCode}." } }
        };
        var problemDetails = new ValidationProblemDetails(errors)
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc9110",
            Title = $"Errore {response.StatusCode}",
            Status = response.StatusCode,
            Instance = request.Path,
        };
        // Aggiungi il traceId nella sezione Extensions
        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        response.ContentType = "application/json";
        var json = JsonSerializer.Serialize(problemDetails);
        await response.WriteAsync(json);
    }
});

// Il middleware cattura tutte le eccezioni non gestite che si verificano durante l'esecuzione della pipeline HTTP.
/*
    Questo middleware è fondamentale perché copre i casi in cui va in errore il codice a runtime (errori imprevisti o bug) 
    e trasforma tali eccezioni in una risposta JSON formattata come ProblemDetails (solitamente con status 500), 
    eseguendo contemporaneamente eventuali operazioni di logging, per esempio tramite il servizio AuditLogService
 */
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
