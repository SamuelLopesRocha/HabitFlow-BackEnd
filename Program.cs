using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 🔐 CHAVE JWT
var key = "9F8A7B6C5D4E3F2G1H0J9K8L7M6N5B4V3C2X1Z";
builder.Configuration["Jwt:Key"] = key;

// 🔥 MongoDB
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDB")
);
builder.Services.AddSingleton<MongoDbContext>();

// 🔐 CONFIGURAÇÃO DE AUTENTICAÇÃO JWT (ESSENCIAL)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key)
        )
    };
});

// 🔥 AUTORIZAÇÃO
builder.Services.AddAuthorization();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ConquistaService>();
builder.Services.AddScoped<NotificacaoService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<MensagemService>();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>

{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseRouting();

// 🔐 ORDEM CORRETA (MUITO IMPORTANTE)
app.UseAuthentication();   
app.UseAuthorization();    
app.MapHub<ChatHub>("/chatHub");
// ⚠️ SEU MIDDLEWARE (opcional agora)
app.UseMiddleware<AuthMiddleware>();

app.MapControllers();

app.Run();