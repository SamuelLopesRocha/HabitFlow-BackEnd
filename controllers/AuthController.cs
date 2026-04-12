using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly IConfiguration _config;
    private readonly EmailService _emailService;

    public AuthController(MongoDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
        _emailService = new EmailService();
    }

    // =========================
    // 📌 REGISTER
    // =========================
    [HttpPost("register")]
    public IActionResult Register([FromBody] Usuario usuario)
    {
        if (string.IsNullOrEmpty(usuario.Email) || string.IsNullOrEmpty(usuario.Senha))
        {
            return BadRequest("Email e senha são obrigatórios");
        }

        var existe = _context.Usuarios.Find(u => u.Email == usuario.Email).FirstOrDefault();

        if (existe != null)
        {
            return BadRequest("Email já cadastrado");
        }

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);

        var random = new Random();
        var codigo = random.Next(100000, 999999).ToString();

        usuario.CodigoConfirmacaoEmail = codigo;
        usuario.CodigoConfirmacaoExpira = DateTime.UtcNow.AddMinutes(10);
        usuario.EmailConfirmado = false;

        _context.Usuarios.InsertOne(usuario);

        _emailService.EnviarCodigo(usuario.Email, codigo);

        return Ok(new
        {
            message = "Usuário cadastrado. Verifique seu email."
        });
    }

    // =========================
    // 🔐 LOGIN
    // =========================
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDTO login)
    {
        if (string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Senha))
        {
            return BadRequest(new ApiResponse<object>(
                false,
                "Email e senha são obrigatórios",
                null
            ));
        }

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

        if (!usuario.EmailConfirmado)
        {
            return Unauthorized(new ApiResponse<object>(
                false,
                "Confirme seu email antes de fazer login",
                null
            ));
        }

        var senhaValida = BCrypt.Net.BCrypt.Verify(login.Senha, usuario.SenhaHash);

        if (!senhaValida)
        {
            return Unauthorized(new ApiResponse<object>(
                false,
                "Email ou senha inválidos",
                null
            ));
        }

        var token = GerarToken(usuario);

        return Ok(new ApiResponse<object>(
            true,
            "Login realizado com sucesso",
            new
            {
                token,
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

    // =========================
    // 📧 CONFIRMAR EMAIL
    // =========================
    [HttpPost("confirmar-email")]
    public IActionResult ConfirmarEmail([FromBody] ConfirmarEmailDTO dto)
    {
        var usuario = _context.Usuarios
            .Find(u => u.Email == dto.Email)
            .FirstOrDefault();

        if (usuario == null)
        {
            return BadRequest("Usuário não encontrado");
        }

        if (usuario.EmailConfirmado)
        {
            return BadRequest("Email já confirmado");
        }

        if (usuario.CodigoConfirmacaoEmail != dto.Codigo)
        {
            return BadRequest("Código inválido");
        }

        if (usuario.CodigoConfirmacaoExpira < DateTime.UtcNow)
        {
            return BadRequest("Código expirado");
        }

        usuario.EmailConfirmado = true;
        usuario.CodigoConfirmacaoEmail = null;
        usuario.CodigoConfirmacaoExpira = null;

        _context.Usuarios.ReplaceOne(u => u.Id == usuario.Id, usuario);

        return Ok("Email confirmado com sucesso!");
    }

    // =========================
    // 🔁 REENVIAR CÓDIGO
    // =========================
    [HttpPost("reenviar-codigo")]
    public IActionResult ReenviarCodigo([FromBody] ReenviarCodigoDTO dto)
    {
        var usuario = _context.Usuarios
            .Find(u => u.Email == dto.Email)
            .FirstOrDefault();

        if (usuario == null)
        {
            return BadRequest("Usuário não encontrado");
        }

        if (usuario.EmailConfirmado)
        {
            return BadRequest("Email já confirmado");
        }

        // 🔢 gerar novo código
        var random = new Random();
        var novoCodigo = random.Next(100000, 999999).ToString();

        usuario.CodigoConfirmacaoEmail = novoCodigo;
        usuario.CodigoConfirmacaoExpira = DateTime.UtcNow.AddMinutes(10);

        _context.Usuarios.ReplaceOne(u => u.Id == usuario.Id, usuario);

        // 📧 envia email novamente
        _emailService.EnviarCodigo(usuario.Email, novoCodigo);

        return Ok(new
        {
            message = "Novo código enviado para o email."
        });
    }

    // =========================
    // 🔐 GERAR JWT
    // =========================
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