using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public AuthMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task Invoke(HttpContext context, MongoDbContext db)
    {
        var token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(" ").Last();

        if (token != null)
        {
            AttachUserToContext(context, db, token);
        }

        await _next(context);
    }

    private void AttachUserToContext(HttpContext context, MongoDbContext db, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                ValidateIssuer = false,
                ValidateAudience = false,

                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            var userId = jwtToken.Claims.First(x => x.Type == "id").Value;

            // 🔥 busca usuário no banco
            var usuario = db.Usuarios.Find(u => u.Id == userId).FirstOrDefault();

            // 🔥 injeta no contexto
            context.Items["Usuario"] = usuario;
        }
        catch
        {
            // 🔒 token inválido → não faz nada (rota decide)
        }
    }
}