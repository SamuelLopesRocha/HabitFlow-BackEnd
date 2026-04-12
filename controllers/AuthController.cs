using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private string GerarRefreshToken()
    {
        var randomNumber = new byte[64];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }

        return Convert.ToBase64String(randomNumber);
    }

    private readonly MongoDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(MongoDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // 🔐 LOGIN
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDTO login)
    {
        // 🔥 validação básica
        if (string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Senha))
        {
            return BadRequest(new ApiResponse<object>(
                false,
                "Email e senha são obrigatórios",
                null
            ));
        }

        // 🔍 busca usuário
        var usuario = _context.Usuarios
            .Find(u => u.Email == login.Email)
            .FirstOrDefault();

        if (usuario == null)
        {
            return Unauthorized(new ApiResponse<object>(
                false,
                "Email ou senha inválidos",
                null
            ));
        }

        // 🔐 valida senha
        var senhaValida = BCrypt.Net.BCrypt.Verify(login.Senha, usuario.SenhaHash);

        if (!senhaValida)
        {
            return Unauthorized(new ApiResponse<object>(
                false,
                "Email ou senha inválidos",
                null
            ));
        }

        // 🔥 GERAR TOKEN
        var token = GerarToken(usuario);

        return Ok(new ApiResponse<object>(
            true,
            "Login realizado com sucesso",
            new
            {
                token = token,
                usuario = new
                {
                    usuario.Id,
                    usuario.Nome,
                    usuario.Username,
                    usuario.Email
                }
            }
        ));
    }

    // 🔐 GERAR JWT
    private string GerarToken(Usuario usuario)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

        var claims = new[]
        {
            new Claim("id", usuario.Id),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Username)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),

            Expires = DateTime.UtcNow.AddHours(2),

            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}