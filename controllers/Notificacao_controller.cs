using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificacaoController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly NotificacaoService _notificacaoService;

    public NotificacaoController(MongoDbContext context, NotificacaoService notificacaoService)
    {
        _context = context;
        _notificacaoService = notificacaoService;
    }

    // =========================
    // 🔐 USER ID
    // =========================
    private string GetUserId()
    {
        return User.FindFirst("id")?.Value;
    }

    // =========================
    // 📌 LISTAR TODAS
    // =========================
    [HttpGet]
    public IActionResult Listar()
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var notificacoes = _context.Notificacoes
            .Find(n => n.UsuarioId == userId)
            .SortByDescending(n => n.DataEnvio)
            .ToList();

        return Ok(notificacoes);
    }

    // =========================
    // 📌 NÃO LIDAS
    // =========================
    [HttpGet("nao-lidas")]
    public IActionResult NaoLidas()
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var notificacoes = _context.Notificacoes
            .Find(n => n.UsuarioId == userId && !n.Lida)
            .SortByDescending(n => n.DataEnvio)
            .ToList();

        return Ok(notificacoes);
    }

    // =========================
    // 📌 MARCAR COMO LIDA
    // =========================
    [HttpPatch("{id}/ler")]
    public IActionResult MarcarComoLida(Guid id)
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var update = Builders<Notificacao>.Update
            .Set(n => n.Lida, true)
            .Set(n => n.DataLeitura, DateTime.UtcNow);

        var result = _context.Notificacoes.UpdateOne(
            n => n.Id == id && n.UsuarioId == userId,
            update
        );

        if (result.MatchedCount == 0)
            return NotFound(new { mensagem = "Notificação não encontrada ❌" });

        return Ok(new { mensagem = "Notificação marcada como lida ✅" });
    }

    // =========================
    // 📌 MARCAR TODAS COMO LIDAS
    // =========================
    [HttpPatch("ler-todas")]
    public IActionResult MarcarTodasComoLidas()
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var update = Builders<Notificacao>.Update
            .Set(n => n.Lida, true)
            .Set(n => n.DataLeitura, DateTime.UtcNow);

        _context.Notificacoes.UpdateMany(
            n => n.UsuarioId == userId && !n.Lida,
            update
        );

        return Ok(new { mensagem = "Todas notificações foram marcadas como lidas ✅" });
    }

    // =========================
    // 📌 DELETAR
    // =========================
    [HttpDelete("{id}")]
    public IActionResult Deletar(Guid id)
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = _context.Notificacoes.DeleteOne(
            n => n.Id == id && n.UsuarioId == userId
        );

        if (result.DeletedCount == 0)
            return NotFound(new { mensagem = "Notificação não encontrada ❌" });

        return Ok(new { mensagem = "Notificação removida ✅" });
    }

    // =========================
    // 🧪 ENDPOINT DE TESTE (OPCIONAL)
    // =========================
    [HttpPost("teste")]
    public IActionResult Teste()
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        _notificacaoService.Criar(
            userId,
            "Teste 🔔",
            "Notificação funcionando!",
            TipoNotificacao.Sistema
        );

        return Ok(new { mensagem = "Notificação criada para teste ✅" });
    }
}