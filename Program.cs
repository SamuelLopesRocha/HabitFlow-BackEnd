using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 🔐 CHAVE JWT (pode ir pro appsettings depois)
builder.Configuration["Jwt:Key"] = "9F8A7B6C5D4E3F2G1H0J9K8L7M6N5B4V3C2X1Z";
// 🔥 Configuração do MongoDB
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDB")
);

// 🔥 CONTEXTO
builder.Services.AddSingleton<MongoDbContext>();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 🔥 ORDEM CORRETA DO PIPELINE
app.UseRouting();

// 🔐 MIDDLEWARE JWT (AQUI É O MAIS IMPORTANTE)
app.UseMiddleware<AuthMiddleware>();

// (opcional, mas pode deixar)
app.UseAuthorization();

app.MapControllers();

app.Run();