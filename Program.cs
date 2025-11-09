using BrainAPI.Models.Settings;
using BrainAPI.Services;
using BrainAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurações e Cache
builder.Services.AddMemoryCache();
builder.Services.Configure<OllamaSettings>(builder.Configuration.GetSection("OllamaSettings"));

// 2. Conexão com Banco de Dados (para RAG)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DataAiContext>(options =>
    options.UseSqlServer(connectionString));

// 3. HttpClient para Ollama
var ollamaSettings = builder.Configuration.GetSection("OllamaSettings").Get<OllamaSettings>();
if (ollamaSettings != null)
{
    // Corrigido: Apenas um bloco AddHttpClient
    builder.Services.AddHttpClient("Ollama", client =>
    {
        client.BaseAddress = new Uri(ollamaSettings.BaseUrl);
        client.Timeout = TimeSpan.FromMinutes(30);  // Mantido o de 30 minutos
    });
}


// 4. Novos Serviços de IA
builder.Services.AddScoped<OllamaClientService>(); 
builder.Services.AddScoped<PersonaService>(); 
builder.Services.AddScoped<DataIngestionService>();
builder.Services.AddScoped<QueryService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<FileProcessorService>(); 
builder.Services.AddScoped<ITokenService, JwtService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ******* ADICIONANDO O CORS AQUI *******
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});
// ***************************************

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<DataAiContext>()
.AddDefaultTokenProviders();



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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // DESATIVADO TEMPORARIAMENTE PARA O TESTE HTTP

// ******* USANDO O CORS AQUI *******
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAnyOrigin");
// **********************************

app.MapControllers();
app.Run();

// Corrigido: O '}' extra foi removido