using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AmizadeController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly NotificacaoService _notificacaoService;
    private readonly ChatService _chatService;

    public AmizadeController(
        MongoDbContext context,
        NotificacaoService notificacaoService,
        ChatService chatService
    )
    {
        _context = context;
        _notificacaoService = notificacaoService;
        _chatService = chatService;
    }

    private string? GetUserId()
    {
        return User.FindFirst("id")?.Value;
    }

    // =========================
    // 📌 ENVIAR SOLICITAÇÃO
    // =========================
    [HttpPost]
    public IActionResult Enviar([FromBody] CreateAmizadeDTO dto)
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (dto == null || string.IsNullOrEmpty(dto.Username))
            return BadRequest(new { mensagem = "Username inválido ❌" });

        // 🔍 busca o usuário pelo username
        var amigo = _context.Usuarios
            .Find(u => u.Username == dto.Username)
            .FirstOrDefault();

        if (amigo == null)
            return NotFound(new { mensagem = "Usuário não encontrado ❌" });

        if (amigo.Id == userId)
            return BadRequest(new { mensagem = "Você não pode adicionar a si mesmo ❌" });

        // 🔥 evita duplicidade (ID ↔ ID)
        var existe = _context.Amizades.Find(a =>
            (a.UsuarioId == userId && a.AmigoId == amigo.Id) ||
            (a.UsuarioId == amigo.Id && a.AmigoId == userId)
        ).FirstOrDefault();

        if (existe != null)
            return BadRequest(new { mensagem = "Amizade já existe ou pendente ❌" });

        var amizade = new Amizade
        {
            UsuarioId = userId,
            AmigoId = amigo.Id,
            Status = StatusAmizade.Pendente,
            DataSolicitacao = DateTime.UtcNow
        };

        _context.Amizades.InsertOne(amizade);

        // 🔔 NOTIFICAÇÃO (aqui sim pode usar username)
        _notificacaoService.Criar(
            amigo.Id,
            "Nova solicitação de amizade 👥",
            "Você recebeu um pedido de amizade",
            TipoNotificacao.Amizade,
            null
        );

        return Ok(new { mensagem = "Solicitação enviada ✅" });
    }

    // =========================
    // 📌 RESPONDER (ACEITAR / RECUSAR)
    // =========================
    [HttpPost("responder")]
    public IActionResult Responder([FromBody] ResponderAmizadeDTO dto)
    {
        if (dto == null || string.IsNullOrEmpty(dto.AmizadeId))
            return BadRequest(new { mensagem = "Dados inválidos ❌" });

        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var amizade = _context.Amizades
            .Find(a => a.Id == dto.AmizadeId)
            .FirstOrDefault();

        if (amizade == null)
            return NotFound(new { mensagem = "Solicitação não encontrada ❌" });

        // 🔥 só quem recebeu pode responder
        if (amizade.AmigoId != userId)
            return Forbid();

        if (amizade.Status != StatusAmizade.Pendente)
            return BadRequest(new { mensagem = "Solicitação já respondida ❌" });

        amizade.Status = dto.Aceitar
            ? StatusAmizade.Aceito
            : StatusAmizade.Recusado;

        if (dto.Aceitar)
        {
            // 🔍 converter ID → Username
            var usuario1 = _context.Usuarios
                .Find(u => u.Id == amizade.UsuarioId)
                .FirstOrDefault();

            var usuario2 = _context.Usuarios
                .Find(u => u.Id == amizade.AmigoId)
                .FirstOrDefault();

            if (usuario1 == null || usuario2 == null)
                return BadRequest("Erro ao criar chat");

            // 🔥 evitar duplicar chat
            var chatExistente = _chatService.ExisteChatPrivado(
                amizade.UsuarioId,
                amizade.AmigoId
            );

            if (!chatExistente)
            {
                var dtoChat = new CreateChatDTO
                {
                    Tipo = TipoChat.Privado,
                    Usernames = new List<string>
                    {
                        usuario1.Username,
                        usuario2.Username
                    }
                };

                _chatService.Criar(dtoChat, amizade.UsuarioId);
            }
        }

        amizade.DataResposta = DateTime.UtcNow;
        amizade.RespondidoPorId = userId;

        _context.Amizades.ReplaceOne(a => a.Id == amizade.Id, amizade);

        // 🔔 NOTIFICAÇÃO
        _notificacaoService.Criar(
            amizade.UsuarioId,
            dto.Aceitar
                ? "Pedido aceito ✅"
                : "Pedido recusado ❌",
            dto.Aceitar
                ? "Seu pedido de amizade foi aceito"
                : "Seu pedido de amizade foi recusado",
            TipoNotificacao.Amizade,
            null
        );

        return Ok(new { mensagem = "Resposta registrada ✅" });
    }

    // =========================
    // 📌 BLOQUEAR
    // =========================
    [HttpPost("bloquear/{amizadeId}")]
    public IActionResult Bloquear(string amizadeId)
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var amizade = _context.Amizades
            .Find(a => a.Id == amizadeId)
            .FirstOrDefault();

        if (amizade == null)
            return NotFound();

        if (amizade.UsuarioId != userId && amizade.AmigoId != userId)
            return Forbid();

        amizade.Status = StatusAmizade.Bloqueado;
        amizade.DataResposta = DateTime.UtcNow;
        amizade.RespondidoPorId = userId;

        _context.Amizades.ReplaceOne(a => a.Id == amizade.Id, amizade);

        return Ok(new { mensagem = "Usuário bloqueado 🚫" });
    }

    // =========================
    // 📌 LISTAR AMIGOS
    // =========================
    [HttpGet("amigos")]
    public IActionResult ListarAmigos()
    {
        var userId = GetUserId();

        var amigos = _context.Amizades
            .Find(a =>
                (a.UsuarioId == userId || a.AmigoId == userId) &&
                a.Status == StatusAmizade.Aceito
            )
            .ToList();

        return Ok(amigos);
    }

    // =========================
    // 📌 LISTAR PENDENTES RECEBIDOS
    // =========================
    [HttpGet("pendentes")]
    public IActionResult Pendentes()
    {
        var userId = GetUserId();

        var pendentes = _context.Amizades
            .Find(a =>
                a.AmigoId == userId &&
                a.Status == StatusAmizade.Pendente
            )
            .ToList();

        return Ok(pendentes);
    }

    // =========================
    // 📌 LISTAR ENVIADOS
    // =========================
    [HttpGet("enviados")]
    public IActionResult Enviados()
    {
        var userId = GetUserId();

        var enviados = _context.Amizades
            .Find(a =>
                a.UsuarioId == userId &&
                a.Status == StatusAmizade.Pendente
            )
            .ToList();

        return Ok(enviados);
    }
}