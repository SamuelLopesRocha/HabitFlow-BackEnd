using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConquistaController : ControllerBase
{
    private readonly MongoDbContext _context;

    public ConquistaController(MongoDbContext context)
    {
        _context = context;
    }

    // =========================
    // 🔐 USER ID
    // =========================
    private string GetUserId()
    {
        return User.FindFirst("id")?.Value;
    }

    // =========================
    // 📌 CRIAR CONQUISTA (CATÁLOGO)
    // =========================
    [HttpPost]
    public IActionResult Criar([FromBody] Conquista conquista)
    {
        if (conquista == null)
            return BadRequest("Dados inválidos ❌");

        if (string.IsNullOrEmpty(conquista.Nome))
            return BadRequest("Nome é obrigatório ❌");

        if (conquista.ValorMeta <= 0)
            return BadRequest("ValorMeta deve ser maior que 0 ❌");

        conquista.Id = Guid.NewGuid();

        _context.Conquistas.InsertOne(conquista);

        return Ok(new
        {
            mensagem = "Conquista criada com sucesso ✅",
            dados = conquista
        });
    }

    // =========================
    // 📌 LISTAR TODAS (CATÁLOGO)
    // =========================
    [HttpGet]
    public IActionResult Listar()
    {
        var conquistas = _context.Conquistas
            .Find(_ => true)
            .ToList();

        return Ok(conquistas);
    }

    // =========================
    // 📌 BUSCAR POR ID
    // =========================
    [HttpGet("{id}")]
    public IActionResult BuscarPorId(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("ID inválido ❌");

        var conquista = _context.Conquistas
            .Find(c => c.Id == id)
            .FirstOrDefault();

        if (conquista == null)
            return NotFound("Conquista não encontrada ❌");

        return Ok(conquista);
    }

    // =========================
    // 📌 ATUALIZAR
    // =========================
    [HttpPut("{id}")]
    public IActionResult Atualizar(Guid id, [FromBody] Conquista dto)
    {
        if (id == Guid.Empty)
            return BadRequest("ID inválido ❌");

        var conquista = _context.Conquistas
            .Find(c => c.Id == id)
            .FirstOrDefault();

        if (conquista == null)
            return NotFound("Conquista não encontrada ❌");

        if (string.IsNullOrEmpty(dto.Nome))
            return BadRequest("Nome é obrigatório ❌");

        if (dto.ValorMeta <= 0)
            return BadRequest("ValorMeta inválido ❌");

        conquista.Nome = dto.Nome;
        conquista.Descricao = dto.Descricao;
        conquista.Icone = dto.Icone;
        conquista.Tipo = dto.Tipo;
        conquista.ValorMeta = dto.ValorMeta;

        _context.Conquistas.ReplaceOne(c => c.Id == id, conquista);

        return Ok(new
        {
            mensagem = "Conquista atualizada com sucesso ✅",
            dados = conquista
        });
    }

    // =========================
    // 📌 DELETAR
    // =========================
    [HttpDelete("{id}")]
    public IActionResult Deletar(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("ID inválido ❌");

        var resultado = _context.Conquistas
            .DeleteOne(c => c.Id == id);

        if (resultado.DeletedCount == 0)
            return NotFound("Conquista não encontrada ❌");

        return Ok(new
        {
            mensagem = "Conquista deletada com sucesso ✅"
        });
    }

    // =========================
    // 🏆 MINHAS CONQUISTAS
    // =========================
    [HttpGet("minhas")]
    public IActionResult MinhasConquistas()
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var minhas = _context.UsuarioConquistas
            .Find(u => u.UserId == userId)
            .ToList();

        var conquistasIds = minhas.Select(m => m.ConquistaId).ToList();

        var conquistas = _context.Conquistas
            .Find(c => conquistasIds.Contains(c.Id))
            .ToList();

        return Ok(new
        {
            total = conquistas.Count,
            conquistas = conquistas
        });
    }
}