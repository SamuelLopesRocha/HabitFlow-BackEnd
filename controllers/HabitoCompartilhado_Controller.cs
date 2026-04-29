using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;

[ApiController]
[Route("api/habitos-compartilhados")]
[Authorize]
public class HabitoCompartilhadoController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly NotificacaoService _notificacao;

    public HabitoCompartilhadoController(
        MongoDbContext context,
        NotificacaoService notificacao
    )
    {
        _context = context;
        _notificacao = notificacao;
    }

    // =========================
    // 🔐 USER ID
    // =========================
    private string GetUserId()
    {
        return User.FindFirst("id")?.Value;
    }

    // =========================
    // 📌 CRIAR HÁBITO COMPARTILHADO
    // =========================
    [HttpPost]
    public IActionResult Criar([FromBody] CreateHabitoCompartilhadoDTO dto)
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (dto == null)
            return BadRequest("Body inválido");

        if (dto.HabitId == Guid.Empty)
            return BadRequest("HabitId inválido");

        if (dto.MetaCompartilhada <= 0)
            return BadRequest("Meta deve ser maior que zero");

        // 🔥 AQUI ESTAVA O ERRO PRINCIPAL
        var habito = _context.Habitos
            .Find(h => h.Id == dto.HabitId)
            .FirstOrDefault();

        if (habito == null)
            return NotFound("Hábito não encontrado");

        var compartilhado = new HabitoCompartilhado
        {
            Id = Guid.NewGuid(),
            HabitId = dto.HabitId,
            Nome = dto.Nome ?? habito.Nome,
            CriadoPorUsuarioId = userId,
            MetaCompartilhada = dto.MetaCompartilhada,
            Tipo = dto.Tipo,
            Periodo = dto.Periodo,
            CriadoEm = DateTime.UtcNow,
            Ativo = true
        };

        _context.HabitosCompartilhados.InsertOne(compartilhado);

        // 🔥 Criador vira participante
        var participante = new ParticipanteHabitoCompartilhado
        {
            Id = Guid.NewGuid(),
            HabitoCompartilhadoId = compartilhado.Id,
            UsuarioId = userId,
            Papel = PapelParticipante.Criador,
            EntrouEm = DateTime.UtcNow
        };

        _context.ParticipantesHabitoCompartilhado.InsertOne(participante);

        return Ok(compartilhado);
    }

    // =========================
    // 📌 CONVIDAR
    // =========================
    [HttpPost("{id}/convidar")]
    public IActionResult Convidar(Guid id, [FromBody] ConvidarDTO dto)
    {
        var adminId = GetUserId();

        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        var usuario = _context.Usuarios
            .Find(u => u.Username == dto.Username)
            .FirstOrDefault();

        if (usuario == null)
            return NotFound("Usuário não encontrado");

        var admin = _context.ParticipantesHabitoCompartilhado
            .Find(p => p.HabitoCompartilhadoId == id &&
                       p.UsuarioId == adminId &&
                       p.Papel == PapelParticipante.Criador)
            .FirstOrDefault();

        if (admin == null)
            return StatusCode(403, "Apenas o criador pode convidar");

        var conviteExistente = _context.ConvitesHabitoCompartilhado
            .Find(c => c.HabitoCompartilhadoId == id &&
                       c.UsuarioConvidadoId == usuario.Id &&
                       c.Status == StatusConvite.Pendente)
            .FirstOrDefault();

        if (conviteExistente != null)
            return BadRequest("Já existe convite pendente");

        var jaParticipa = _context.ParticipantesHabitoCompartilhado
            .Find(p => p.HabitoCompartilhadoId == id &&
                       p.UsuarioId == usuario.Id)
            .FirstOrDefault();

        if (jaParticipa != null)
            return BadRequest("Usuário já está no hábito");

        var convite = new ConviteHabitoCompartilhado
        {
            Id = Guid.NewGuid(),
            HabitoCompartilhadoId = id,
            UsuarioConvidadoId = usuario.Id,
            UsuarioConvidadorId = adminId,
            Status = StatusConvite.Pendente,
            CriadoEm = DateTime.UtcNow
        };

        _context.ConvitesHabitoCompartilhado.InsertOne(convite);

        _notificacao.Criar(
            usuario.Id,
            "Convite 🤝",
            "Você recebeu um convite para um hábito compartilhado",
            TipoNotificacao.Sistema
        );

        return Ok("Convidado com sucesso");
    }

    // =========================
    // 📌 ACEITAR CONVITE
    // =========================
    [HttpPost("convite/{id}/aceitar")]
    public IActionResult Aceitar(Guid id)
    {
        var userId = GetUserId();

        var convite = _context.ConvitesHabitoCompartilhado
            .Find(c => c.Id == id && c.UsuarioConvidadoId == userId)
            .FirstOrDefault();

        if (convite == null || convite.Status != StatusConvite.Pendente)
            return BadRequest("Convite inválido");

        var usuario = _context.Usuarios
            .Find(u => u.Id == userId)
            .FirstOrDefault();

        if (usuario == null)
            return NotFound("Usuário não encontrado");

        convite.Status = StatusConvite.Aceito;
        convite.RespondidoEm = DateTime.UtcNow;

        _context.ConvitesHabitoCompartilhado.ReplaceOne(c => c.Id == convite.Id, convite);

        // 🔥 adiciona participante
        var participante = new ParticipanteHabitoCompartilhado
        {
            Id = Guid.NewGuid(),
            HabitoCompartilhadoId = convite.HabitoCompartilhadoId,
            UsuarioId = userId,
            Papel = PapelParticipante.Participante,
            EntrouEm = DateTime.UtcNow
        };

        _context.ParticipantesHabitoCompartilhado.InsertOne(participante);

        // 🔔 NOTIFICA TODOS DO GRUPO
        var participantes = _context.ParticipantesHabitoCompartilhado
            .Find(p => p.HabitoCompartilhadoId == convite.HabitoCompartilhadoId)
            .ToList();

        foreach (var p in participantes)
        {
            if (p.UsuarioId == userId)
                continue;

            _notificacao.Criar(
                p.UsuarioId,
                "Novo participante 🎉",
                $"{usuario.Username} entrou no hábito compartilhado",
                TipoNotificacao.Sistema
            );
        }

        // 🔔 NOTIFICA ADMIN
        _notificacao.Criar(
            convite.UsuarioConvidadorId,
            "Convite aceito ✅",
            $"{usuario.Username} aceitou o convite",
            TipoNotificacao.Sistema
        );
        return Ok("Convite aceito");
    }

    // =========================
    // 📌 RECUSAR CONVITE
    // =========================
    [HttpPost("convite/{id}/recusar")]
    public IActionResult Recusar(Guid id)
    {
        var userId = GetUserId();

        var convite = _context.ConvitesHabitoCompartilhado
            .Find(c => c.Id == id && c.UsuarioConvidadoId == userId)
            .FirstOrDefault();

        if (convite == null || convite.Status != StatusConvite.Pendente)
            return BadRequest("Convite inválido");

        var usuario = _context.Usuarios
            .Find(u => u.Id == userId)
            .FirstOrDefault();

        // ✅ ADICIONA ISSO AQUI
        if (usuario == null)
            return NotFound("Usuário não encontrado");

        convite.Status = StatusConvite.Recusado;
        convite.RespondidoEm = DateTime.UtcNow;

        _context.ConvitesHabitoCompartilhado.ReplaceOne(c => c.Id == convite.Id, convite);

        // 🔔 NOTIFICA ADMIN
        _notificacao.Criar(
            convite.UsuarioConvidadorId,
            "Convite recusado ❌",
            $"{usuario.Username} recusou o convite",
            TipoNotificacao.Sistema
        );

        return Ok("Convite recusado");
    }

    // =========================
    // 📌 LISTAR CONVITES
    // =========================
    [HttpGet("convites")]
    public IActionResult ListarConvites()
    {
        var userId = GetUserId();

        var convites = _context.ConvitesHabitoCompartilhado
            .Find(c => c.UsuarioConvidadoId == userId && c.Status == StatusConvite.Pendente)
            .ToList();

        return Ok(convites);
    }

    // =========================
    // 📌 PROGRESSO
    // =========================
    [HttpGet("{id}/progresso")]
    public IActionResult Progresso(Guid id)
    {
        var habitoCompartilhado = _context.HabitosCompartilhados
            .Find(h => h.Id == id)
            .FirstOrDefault();

        if (habitoCompartilhado == null)
            return NotFound("Hábito não encontrado");

        // 🔥 pega só participantes do grupo
        var participantes = _context.ParticipantesHabitoCompartilhado
            .Find(p => p.HabitoCompartilhadoId == id)
            .ToList()
            .Select(p => p.UsuarioId)
            .ToList();

        var registros = _context.HabitoRecordes
            .Find(r => r.HabitId == habitoCompartilhado.HabitId &&
                    participantes.Contains(r.UserId))
            .ToList();

        var agrupado = registros
            .GroupBy(r => r.UserId)
            .Select(g => new {
                userId = g.Key,
                total = g.Sum(x => x.Quantidade)
            })
            .OrderByDescending(x => x.total)
            .ToList();

        return Ok(agrupado);
    }
}