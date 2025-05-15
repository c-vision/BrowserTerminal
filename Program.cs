using Microsoft.AspNetCore.Authentication.JwtBearer; // Middleware per l'autenticazione tramite JWT.
using Microsoft.IdentityModel.Tokens; // Libreria per la gestione e validazione dei token JWT.
using System.Text; // Necessario per codificare la chiave segreta JWT in un array di byte.
using AspNetCoreRateLimit; // Libreria per implementare il rate limiting nelle API.
// Per leggere e serializzare file JSON.
using dotenv.net; // Libreria per gestire le variabili d'ambiente da un file .env.
using FluentValidation; // Libreria per la validazione avanzata degli input.
using Microsoft.IdentityModel.Logging; // Necessario per abilitare il logging PII

//using Microsoft.AspNetCore.Antiforgery; // Middleware per la protezione CSRF (Cross-Site Request Forgery).

var builder = WebApplication.CreateBuilder(args); // Configura l'applicazione ASP.NET Core.

// Aggiunge configurazioni specifiche per l'ambiente attuale
// Legge la configurazione da appsettings.json (settings comuni a tutti gli ambienti)
// Sovrascrive i valori con quelli con la stessa chiave dal file di $"appsettings.{builder.Environment.EnvironmentName}.json"
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory()) // Imposta il percorso base
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // File principale
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true) // File specifico per l'ambiente
    .AddEnvironmentVariables(); // Aggiunge variabili d'ambiente

const string websiteUrl = "https://example.com";        // production website url
const string reactAddress = "http://localhost:3000";    // typical react development url
const string angularAddress = "http://localhost:4200";  // typical angular development url

// Recupera l'indirizzo configurato per Kestrel dall' appsettings
// Legge l'URL configurato in appsettings
var localAddress = builder.Configuration["Kestrel:Endpoints:Http:Url"] ?? "http://localhost:5046"; // asp.net core local url

// Configurazione per la sessione
// Determina l'ambiente utilizzando la variabile ASPNETCORE_ENVIRONMENT
if (builder.Environment.IsDevelopment()) // Verifica se l'ambiente è di sviluppo
{
    // Configurazione per l'ambiente locale di sviluppo
    builder.WebHost.UseUrls(localAddress); // Imposta l'URL per l'ambiente di sviluppo locale.
    IdentityModelEventSource.ShowPII = true; // Abilita il logging delle informazioni personali per il debug
}
else // Configurazione per l'ambiente di produzione
{
    // Configurazione per l'ambiente di produzione
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(80); // Porta HTTP per produzione
        options.ListenAnyIP(443, listenOptions => listenOptions.UseHttps()); // Porta HTTPS per produzione
    });
}
// Configurazione per la sessione
builder.Services.AddDistributedMemoryCache(); // Aggiunge un provider di cache in memoria per memorizzare i dati della sessione.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // La sessione scade dopo 30 minuti di inattività.
    options.Cookie.HttpOnly = true; // Protegge il cookie della sessione contro attacchi XSS.
    options.Cookie.IsEssential = true; // Specifica che il cookie è essenziale per l'applicazione.
});

DotEnv.Load(); // Carica tutte le variabili d'ambiente dal file .env.

var secretKey = builder.Configuration["Jwt:SecretKey"]; // Recupera la chiave segreta dal file di configurazione
if (string.IsNullOrEmpty(secretKey)) // Se la chiave segreta non è presente o vuota, usa un fallback.
    secretKey = Environment.GetEnvironmentVariable("SECRET_KEY");
// Lancia un'eccezione se nessuna chiave è disponibile.
if (string.IsNullOrEmpty(secretKey)) // Controlla che la chiave non sia vuota.
    throw new Exception("No secret key found. Ensure app settings or .env is configured correctly.");

// Configurazione per l'autenticazione tramite JWT.
var key = Encoding.ASCII.GetBytes(secretKey); // Converte la chiave segreta in un array di byte.

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Specifica JWT come schema predefinito.
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Specifica JWT per le sfide di autenticazione.
}).AddJwtBearer(options =>
{
    // Risorse Generali per JWT
    // https://jwt.io/
    // https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters
    // https://datatracker.ietf.org/doc/html/rfc7519
    options.RequireHttpsMetadata = true; // Richiede che i token vengano inviati tramite HTTPS.
    options.SaveToken = true; // Salva il token nelle proprietà di autenticazione.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // ValidateIssuerSigningKey
        // Descrizione: Abilita la validazione della firma del token JWT. Quando è impostato su true, il sistema verifica che il token sia stato
        // firmato correttamente utilizzando la chiave specificata in IssuerSigningKey.
        // Valori possibili:
        // true: Consigliato. Abilita la validazione della firma del token.
        // false: Disabilita la validazione della firma. Non consigliato per motivi di sicurezza.
        // Dove ottenere la chiave per la firma?
        // La chiave utilizzata per firmare il token deve essere segreta e condivisa solo tra il server di autenticazione (emittente) e il sistema
        // che convalida i token.
        // Può essere generata manualmente o utilizzando strumenti come JWT.io. https://jwt.io/
        // Utilizza una chiave sufficientemente complessa per evitare attacchi di brute force.
        // Documentazione ufficiale: Microsoft Docs - TokenValidationParameters
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters.validatesignature
        ValidateIssuerSigningKey = true, // Valida la firma del token per garantire che il token non sia stato manomesso.

        // IssuerSigningKey
        // Descrizione: Specifica la chiave utilizzata per firmare il token JWT. Questa chiave viene utilizzata per verificare la validità del token.
        // Valori consigliati:
        // Usa una chiave simmetrica (come mostrato nell'esempio SymmetricSecurityKey).
        // La chiave deve essere generata utilizzando caratteri alfanumerici casuali e deve essere almeno di 256 bit (32 caratteri) se utilizzi l'algoritmo
        // HMAC-SHA256.
        // Dove configurare la chiave?
        // Può essere memorizzata in un file .env, in una variabile d'ambiente o in un servizio di gestione delle chiavi come Azure Key Vault o AWS Secrets
        // Manager.
        // Esempio di generazione manuale:
        // var secretKey = Encoding.ASCII.GetBytes("UnaChiaveMoltoSegretaCon32Caratteri"); // Lunghezza minima 32 caratteri
        // Documentazione ufficiale: Microsoft Docs - SymmetricSecurityKey
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.symmetricsecuritykey
        IssuerSigningKey = new SymmetricSecurityKey(key), // Specifica la chiave utilizzata per firmare e convalidare il token.

        // ValidateIssuer
        // Descrizione: Abilita la validazione dell' issuer del token JWT. L' issuer è l'entità che emette il token e dovrebbe essere un valore specifico e noto.
        // Valori possibili:
        // true: Consigliato. Abilita la validazione per assicurarsi che il token provenga da un emittente valido.
        // false: Disabilita la validazione. Non consigliato, a meno che tu non abbia un caso d'uso molto specifico.
        // Valori consigliati per ValidIssuer:
        // Specifica un URL o un identificatore univoco per il server di autenticazione (ad esempio, "https://my-auth-server.com" o "MyIssuer").
        // Deve essere lo stesso valore utilizzato durante la generazione del token JWT.
        // Documentazione ufficiale: Microsoft Docs - ValidateIssuer
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters.validateissuer
        ValidateIssuer = true, // Valida l' issuer (emittente) del token.

        // ValidIssuer
        // Descrizione: Specifica l' issuer (emittente) valido per i token JWT. Questo valore viene confrontato con il campo iss (issuer) del payload
        // del token JWT.
        // Valori consigliati:
        // Usa un URL o un nome univoco che identifichi il server di autenticazione.
        // Esempio: "https://auth.example.com" o "MyCustomIssuer".
        // Dove configurarlo?
        // Assicurati che questo valore corrisponda esattamente all' issuer configurato nel server che emette i token JWT.
        // Documentazione ufficiale: Microsoft Docs - ValidIssuer
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters.validissuer
        ValidIssuer = builder.Configuration["Jwt:Issuer"], // Configura l' issuer valido dal file di configurazione.

        // ValidateAudience
        // Descrizione: Abilita la validazione dell'audience (destinatario) del token JWT. L'audience è un identificatore che specifica chi è
        // autorizzato a utilizzare il token.
        // Valori possibili:
        // true: Consigliato. Abilita la validazione per assicurarsi che il token sia destinato alla tua applicazione.
        // false: Disabilita la validazione. Non consigliato, a meno che tu non abbia un caso d'uso molto specifico.
        // Dove configurarlo?
        // Configura l'audience nel server di autenticazione durante la generazione del token JWT.
        // Deve corrispondere al valore di ValidAudience.
        // Documentazione ufficiale: Microsoft Docs - ValidateAudience
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters.validateaudience
        ValidateAudience = true, // Valida l'audience (destinatario previsto) del token.

        // ValidAudience
        // Descrizione: Specifica l'audience (destinatario) valido per i token JWT. Questo valore viene confrontato con il campo aud del
        // payload del token JWT.
        // Valori consigliati:
        // Usa un identificatore univoco che rappresenta la tua applicazione o API.
        // Esempio: "https://api.example.com" o "MyApplication".
        // Dove configurarlo?
        // Deve essere configurato sia nel server che emette i token JWT che nell'applicazione che li valida.
        // Documentazione ufficiale: Microsoft Docs - ValidAudience
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters.validaudience
        ValidAudience = builder.Configuration["Jwt:Audience"], // Configura l'audience valida dal file di configurazione.

        ValidateLifetime = true, // Assicurati che il token non sia scaduto

        // ClockSkew
        // Descrizione: Specifica una tolleranza temporale per la validazione del token JWT. Normalmente, i token JWT hanno un tempo di
        // scadenza (exp), e questa tolleranza consente di gestire differenze minime di orario tra server.
        // Valori consigliati:
        // TimeSpan.Zero: Rimuove la tolleranza. Consigliato se i server sono sincronizzati tramite NTP (Network Time Protocol).
        // Default: 5 minuti. Se non configurato, il valore predefinito è 5 minuti.
        // Dove configurarlo?
        // Configura ClockSkew a TimeSpan.Zero se il tuo sistema è ben sincronizzato e non tollera token scaduti.
        // Usa un valore maggiore (ad esempio, TimeSpan.FromMinutes(1)) se prevedi differenze temporali tra i sistemi.
        // Documentazione ufficiale: Microsoft Docs - ClockSkew
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters.clockskew
        ClockSkew = TimeSpan.Zero // Rimuove la tolleranza temporale per la validazione del token. Default è 5 minuti.
        //ClockSkew = TimeSpan.FromMinutes(5), // Tolleranza per differenze di tempo tra server (default: 5 minuti)

        // Valida l'algoritmo della firma
        // RequireSignedTokens = true, // Richiede che il token sia firmato

        // Configurazioni opzionali per il token
        // ValidateActor = false, // (Opzionale) Non valida l'attore specificato nel token
        // ValidateTokenReplay = false, // (Opzionale) Non verifica se il token è stato riutilizzato
        // RequireExpirationTime = true, // Richiede che il token abbia una data di scadenza (claim "exp")

        // Configura la gestione del nome dell'utente (opzionale)
        // NameClaimType = "name", // Specifica il nome del claim usato per identificare l'utente
        // RoleClaimType = "role", // Specifica il nome del claim usato per identificare il ruolo dell'utente

        // Configura opzioni personalizzate per il token (se necessario)
        // TokenDecryptionKey = null // (Opzionale) Chiave per decrittare token criptati (se utilizzati)
    };
    // Per capire se il middleware JWT sta ricevendo e validando correttamente il token, aggiungiamo un log personalizzato.
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            if (context.Request.Headers.TryGetValue("Authorization", out var value))
            {
                Console.WriteLine($"Received Token: {value}");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = _ =>
        {
            Console.WriteLine("Token validated successfully");
            return Task.CompletedTask;
        }
    };
});

// Memorizza la chiave segreta nella sessione per renderla accessibile in altre parti dell'app.
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Aggiunge il supporto per accedere al contesto HTTP.

// I token JWT sono stateless per natura. Non hanno bisogno di essere associati a una sessione lato server.
// La chiave segreta non dovrebbe essere salvata nella sessione.
/*
builder.Services.AddScoped(provider =>
{
    var httpContext = provider.GetService<IHttpContextAccessor>()?.HttpContext; // Recupera il contesto HTTP attuale.
    httpContext?.Session.SetString("JWT_SECRET", secretKey); // Salva la chiave segreta nella sessione.
    return secretKey; // Ritorna la chiave segreta per altri servizi che potrebbero utilizzarla.
});
*/

// Configura i controller e altri servizi.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Aggiunge la configurazione Swagger per la documentazione dell'API.

// Configurazione CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment()) // Ambiente di sviluppo
    {
        options.AddPolicy("AllowSpecificOrigins", policy =>
        {
            policy.WithOrigins(localAddress, reactAddress, angularAddress) // Origini per sviluppo locale (React, Angular, ecc.)
                .AllowAnyHeader() // Consente tutti gli header.
                .AllowAnyMethod() // Consente tutti i metodi HTTP (GET, POST, ecc.).
                .AllowCredentials(); // Necessario se si inviano cookie o credenziali
        });
    }
    else // Ambiente di produzione
    {
        options.AddPolicy("AllowSpecificOrigins", policy =>
        {
            policy.WithOrigins(websiteUrl) // Sostituisci con il dominio di produzione autorizzato.
                .AllowAnyHeader() // Consente tutti gli header.
                .AllowAnyMethod() // Consente tutti i metodi HTTP.
                .AllowCredentials(); // Necessario se si inviano cookie o credenziali
        });
    }
});

// Configurazione del rate limiting per proteggere l'API da abusi.
builder.Services.AddMemoryCache(); // Aggiunge la memoria cache necessaria per il rate limiting.
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules =
    [
        new RateLimitRule
        {
            Endpoint = "/api/login", // Applica limitazioni solo all' endpoint di login.
            Limit = 5, // Permette massimo 5 richieste per minuto.
            Period = "1m" // Periodo di tempo di 1 minuto.
        },

        new RateLimitRule
        {
            Endpoint = "*", // Applica una regola globale per tutti gli endpoint.
            Limit = 100, // Permette massimo 100 richieste per minuto.
            Period = "1m" // Periodo di tempo di 1 minuto.
        }
    ];
});
builder.Services.AddInMemoryRateLimiting(); // Aggiunge il middleware per limitare le richieste in memoria.
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>(); // Configura la gestione delle regole.

// Configurazione per la protezione CSRF.
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN"; // Specifica il nome dell' header per il token CSRF.
});

// Configurazione HTTPS Strict Transport Security (HSTS).
builder.Services.AddHsts(options =>
{
    options.Preload = true; // Abilita il preload HSTS.
    options.IncludeSubDomains = true; // Applica HSTS anche ai sotto domini.
    options.MaxAge = TimeSpan.FromDays(365); // Imposta il periodo massimo a 1 anno.
});

// Configurazione di FluentValidation per la validazione degli input.
builder.Services.AddValidatorsFromAssemblyContaining<Program>(); // Scansiona l' assembly per i validator.

var app = builder.Build(); // Costruisce l'applicazione.

if (builder.Environment.IsDevelopment()) // Configurazioni per l'ambiente di sviluppo.
{
    app.UseSwagger(); // Abilita Swagger per la documentazione dell'API.
    app.UseSwaggerUI(); // Abilita l'interfaccia utente di Swagger.
}

// Abilita il middleware per servire file statici.
app.UseDefaultFiles(); // Cerca automaticamente un file chiamato `index.html` nella cartella wwwroot.
app.UseStaticFiles(); // Serve i file statici (es. CSS, JS).

//app.UseHttpsRedirection(); // Reindirizza tutte le richieste HTTP a HTTPS.
//app.UseHsts(); // Abilita HSTS per forzare l'uso di HTTPS.

app.UseSession(); // Abilita il middleware per la gestione della sessione.
app.UseCors("AllowSpecificOrigins"); // Applica la politica CORS definita.
app.UseCors("AllowLocalhost");
app.UseIpRateLimiting(); // Abilita il middleware per il rate limiting.

/*
app.Use(async (context, next) => // Middleware per la protezione CSRF.
{
    if (context.Request.Method != "POST") return;
    var antiforgery = context.RequestServices.GetService<IAntiforgery>();
    antiforgery?.ValidateRequestAsync(context).Wait();
    await next();
});
*/

// Log nella middleware di autenticazione per verificare il token ricevuto:
// Il middleware può essere utile per debugging, ma non è necessario in produzione.
// Rimuoverlo per evitare potenziali problemi di sicurezza.
app.Use(async (context, next) =>
{
    var token = context.Request.Headers.Authorization.ToString();
    Console.WriteLine($"Received Authorization Header: {token}");
    await next();
});

app.Use(async (context, next) => // Middleware per la Content Security Policy (CSP).
{
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; connect-src 'self';");
    await next();
});

app.UseAuthentication(); // Abilita l'autenticazione.
app.UseAuthorization(); // Abilita l'autorizzazione.

app.MapControllers(); // Mappa i controller definiti nell'applicazione.

app.Run(); // Avvia il server.
