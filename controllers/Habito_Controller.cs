using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HabitosController : ControllerBase
{
    private readonly MongoDbContext _context;

    public HabitosController(MongoDbContext context)
    {
        _context = context;
    }

    // =========================
    // 🔧 MÉTODO AUXILIAR
    // =========================
    private string GetUserId()
    {
        return User.FindFirst("id")?.Value;
    }

    // =========================
    // 📌 CRIAR
    // =========================
    [HttpPost]
    public IActionResult Criar([FromBody] CreateHabitoDTO dto)
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { mensagem = "Usuário não autenticado ❌" });

        if (dto == null)
            return BadRequest(new { mensagem = "Body vazio ❌" });

        var habito = new Habito
        {
            UserId = userId, // ✅ STRING
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Cor = dto.Cor,
            Icone = dto.Icone,
            Tipo = dto.Tipo,
            Frequencia = dto.Frequencia,
            Meta = dto.Meta,
            Unidade = dto.Unidade,
            HoraPreferida = dto.HoraPreferida,
            NotificacaoAtiva = dto.NotificacaoAtiva,
            Prioridade = dto.Prioridade,
            DataInicio = dto.DataInicio,
            CriadoEm = DateTime.UtcNow
        };

        _context.Habitos.InsertOne(habito);

        return Created("", habito);
    }

    // =========================
    // 📌 LISTAR
    // =========================
    [HttpGet]
    public IActionResult Listar()
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var habitos = _context.Habitos
            .Find(h => h.UserId == userId) // ✅ STRING
            .ToList();

        return Ok(habitos);
    }

    // =========================
    // 📌 BUSCAR POR ID
    // =========================
    [HttpGet("{id}")]
    public IActionResult BuscarPorId(string id)
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!Guid.TryParse(id, out var habitoId))
            return BadRequest(new { mensagem = "ID inválido ❌" });

        var habito = _context.Habitos
            .Find(h => h.Id == habitoId && h.UserId == userId)
            .FirstOrDefault();

        if (habito == null)
            return NotFound(new { mensagem = "Hábito não encontrado ❌" });

        return Ok(habito);
    }

    // =========================
    // 📌 ATUALIZAR
    // =========================
    [HttpPut("{id}")]
    public IActionResult Atualizar(string id, [FromBody] CreateHabitoDTO dto)
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!Guid.TryParse(id, out var habitoId))
            return BadRequest();

        var habito = _context.Habitos
            .Find(h => h.Id == habitoId && h.UserId == userId)
            .FirstOrDefault();

        if (habito == null)
            return NotFound(new { mensagem = "Hábito não encontrado ❌" });

        habito.Nome = dto.Nome;
        habito.Descricao = dto.Descricao;
        habito.Cor = dto.Cor;
        habito.Icone = dto.Icone;
        habito.Tipo = dto.Tipo;
        habito.Frequencia = dto.Frequencia;
        habito.Meta = dto.Meta;
        habito.Unidade = dto.Unidade;
        habito.HoraPreferida = dto.HoraPreferida;
        habito.NotificacaoAtiva = dto.NotificacaoAtiva;
        habito.Prioridade = dto.Prioridade;
        habito.DataInicio = dto.DataInicio;
        habito.UpdatedAt = DateTime.UtcNow;

        _context.Habitos.ReplaceOne(h => h.Id == habitoId, habito);

        return Ok(habito);
    }

    // =========================
    // 📌 DELETAR
    // =========================
    [HttpDelete("{id}")]
    public IActionResult Deletar(string id)
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!Guid.TryParse(id, out var habitoId))
            return BadRequest();

        var resultado = _context.Habitos
            .DeleteOne(h => h.Id == habitoId && h.UserId == userId);

        if (resultado.DeletedCount == 0)
            return NotFound(new { mensagem = "Hábito não encontrado ❌" });

        return Ok(new { mensagem = "Hábito removido com sucesso ✅" });
    }
}