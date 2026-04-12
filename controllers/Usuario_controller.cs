using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly MongoDbContext _context;

    public UsuarioController(MongoDbContext context)
    {
        _context = context;
    }

    // 🔁 Converter Usuario → DTO
    private UsuarioResponseDTO ToDTO(Usuario u)
    {
        return new UsuarioResponseDTO
        {
            Id = u.Id,
            Nome = u.Nome,
            Username = u.Username,
            Email = u.Email,
            FotoPerfilUrl = u.FotoPerfilUrl,
            DataNascimento = u.DataNascimento,
            TemaPreferido = u.TemaPreferido
        };
    }

    // 🔐 Helper para pegar usuário autenticado
    private Usuario GetUsuarioLogado()
    {
        return HttpContext.Items["Usuario"] as Usuario;
    }

    // 🔐 GET: api/usuario (PROTEGIDO)
    [HttpGet]
    public IActionResult Get()
    {
        var usuarioLogado = GetUsuarioLogado();

        if (usuarioLogado == null)
        {
            return Unauthorized(new ApiResponse<object>(false, "Não autorizado", null));
        }

        var usuarios = _context.Usuarios.Find(_ => true).ToList();

        if (!usuarios.Any())
        {
            return NotFound(new ApiResponse<object>(false, "Nenhum usuário encontrado", null));
        }

        var response = usuarios.Select(u => ToDTO(u));

        return Ok(new ApiResponse<object>(true, "Usuários encontrados", response));
    }

    // 🔐 GET: api/usuario/{id} (PROTEGIDO)
    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var usuarioLogado = GetUsuarioLogado();

        if (usuarioLogado == null)
        {
            return Unauthorized(new ApiResponse<object>(false, "Não autorizado", null));
        }

        var usuario = _context.Usuarios.Find(u => u.Id == id).FirstOrDefault();

        if (usuario == null)
        {
            return NotFound(new ApiResponse<object>(false, "Usuário não encontrado", null));
        }

        return Ok(new ApiResponse<object>(true, "Usuário encontrado", ToDTO(usuario)));
    }

    // POST: api/usuario (PÚBLICO - CADASTRO)
    [HttpPost]
    public IActionResult Create([FromBody] Usuario usuario)
    {
        if (string.IsNullOrEmpty(usuario.Email) ||
            string.IsNullOrEmpty(usuario.Senha) ||
            string.IsNullOrEmpty(usuario.Username))
        {
            return BadRequest(new ApiResponse<object>(
                false,
                "Email, username e senha são obrigatórios",
                null
            ));
        }

        var existeEmail = _context.Usuarios.Find(u => u.Email == usuario.Email).Any();
        var existeUsername = _context.Usuarios.Find(u => u.Username == usuario.Username).Any();

        if (existeEmail)
        {
            return BadRequest(new ApiResponse<object>(false, "Email já está em uso", null));
        }

        if (existeUsername)
        {
            return BadRequest(new ApiResponse<object>(false, "Username já está em uso", null));
        }

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
        usuario.Senha = null;

        usuario.CreatedAt = DateTime.UtcNow;
        usuario.UpdatedAt = DateTime.UtcNow;

        _context.Usuarios.InsertOne(usuario);

        return StatusCode(201, new ApiResponse<object>(
            true,
            "Usuário criado com sucesso",
            ToDTO(usuario)
        ));
    }

    // 🔐 PUT: api/usuario/{id}
    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] Usuario usuarioAtualizado)
    {
        var usuarioLogado = GetUsuarioLogado();

        if (usuarioLogado == null)
        {
            return Unauthorized(new ApiResponse<object>(false, "Não autorizado", null));
        }

        var usuario = _context.Usuarios.Find(u => u.Id == id).FirstOrDefault();

        if (usuario == null)
        {
            return NotFound(new ApiResponse<object>(false, "Usuário não encontrado", null));
        }

        // 🔥 valida email duplicado
        if (!string.IsNullOrEmpty(usuarioAtualizado.Email))
        {
            var emailExiste = _context.Usuarios
                .Find(u => u.Email == usuarioAtualizado.Email && u.Id != id)
                .Any();

            if (emailExiste)
            {
                return BadRequest(new ApiResponse<object>(false, "Email já está em uso", null));
            }
        }

        usuario.Nome = usuarioAtualizado.Nome ?? usuario.Nome;
        usuario.Email = usuarioAtualizado.Email ?? usuario.Email;
        usuario.TemaPreferido = usuarioAtualizado.TemaPreferido ?? usuario.TemaPreferido;
        usuario.FotoPerfilUrl = usuarioAtualizado.FotoPerfilUrl ?? usuario.FotoPerfilUrl;
        usuario.DataNascimento = usuarioAtualizado.DataNascimento ?? usuario.DataNascimento;

        usuario.UpdatedAt = DateTime.UtcNow;

        _context.Usuarios.ReplaceOne(u => u.Id == id, usuario);

        return Ok(new ApiResponse<object>(
            true,
            "Usuário atualizado com sucesso",
            ToDTO(usuario)
        ));
    }

    // 🔐 DELETE: api/usuario/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        var usuarioLogado = GetUsuarioLogado();

        if (usuarioLogado == null)
        {
            return Unauthorized(new ApiResponse<object>(false, "Não autorizado", null));
        }

        var usuario = _context.Usuarios.Find(u => u.Id == id).FirstOrDefault();

        if (usuario == null)
        {
            return NotFound(new ApiResponse<object>(false, "Usuário não encontrado", null));
        }

        _context.Usuarios.DeleteOne(u => u.Id == id);

        return Ok(new ApiResponse<object>(
            true,
            "Usuário removido com sucesso",
            null
        ));
    }

    // 🔐 GET: api/usuario/me
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var usuario = GetUsuarioLogado();

        if (usuario == null)
        {
            return Unauthorized(new ApiResponse<object>(false, "Não autorizado", null));
        }

        return Ok(new ApiResponse<object>(
            true,
            "Usuário autenticado",
            ToDTO(usuario)
        ));
    }
}