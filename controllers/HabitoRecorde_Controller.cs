using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HabitoRecordeController : ControllerBase
{
    private readonly MongoDbContext _context;

    private readonly ConquistaService _conquistaService;

    public HabitoRecordeController(MongoDbContext context, ConquistaService conquistaService)
    {
        _context = context;
        _conquistaService = conquistaService;
    }

    // =========================
    // 🔐 USER ID
    // =========================
    private string GetUserId()
    {
        return User.FindFirst("id")?.Value;
    }

    // =========================
    // 🔥 FUNÇÃO STREAK (ADICIONADO)
    // =========================
    private (int streakAtual, int melhorStreak) CalcularStreak(Guid habitId)
    {
        var registros = _context.HabitoRecordes
            .Find(r => r.HabitId == habitId && r.Concluido == true)
            .SortByDescending(r => r.Data)
            .ToList();

        if (registros.Count == 0)
            return (0, 0);

        int streakAtual = 0;
        int melhorStreak = 0;
        int streakTemp = 0;

        DateTime? dataAnterior = null;

        foreach (var r in registros)
        {
            if (dataAnterior == null)
            {
                streakTemp = 1;
            }
            else
            {
                var diff = (dataAnterior.Value.Date - r.Data.Date).Days;

                if (diff == 1)
                    streakTemp++;
                else
                    streakTemp = 1;
            }

            if (streakTemp > melhorStreak)
                melhorStreak = streakTemp;

            dataAnterior = r.Data;
        }

        var hoje = DateTime.UtcNow.Date;
        var dias = registros
            .Select(r => r.Data.Date)
            .ToHashSet();

        streakAtual = 0;

        while (dias.Contains(hoje.Date))
        {
            streakAtual++;
            hoje = hoje.AddDays(-1);
        }

        return (streakAtual, melhorStreak);
    }

    // =========================
    // 📌 CRIAR REGISTRO
    // =========================
    [HttpPost]
    public IActionResult Criar([FromBody] CreateHabitoRecordeDTO dto)
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { mensagem = "Usuário não autenticado ❌" });

        if (dto == null)
            return BadRequest(new { mensagem = "Body inválido ❌" });

        if (dto.HabitId == Guid.Empty)
            return BadRequest(new { mensagem = "HabitId inválido ❌" });

        if (dto.Quantidade < 0)
            return BadRequest(new { mensagem = "Quantidade não pode ser negativa ❌" });

        var habito = _context.Habitos
            .Find(h => h.Id == dto.HabitId && h.UserId == userId)
            .FirstOrDefault();

        if (habito == null)
            return NotFound(new { mensagem = "Hábito não encontrado ❌" });

        var hoje = DateTime.UtcNow.Date;

        var jaExiste = _context.HabitoRecordes
            .Find(r => r.HabitId == dto.HabitId && r.Data == hoje)
            .FirstOrDefault();

        if (jaExiste != null)
            return BadRequest(new { mensagem = "Hábito já registrado hoje ❌" });

        if (dto.Concluido && dto.Quantidade <= 0)
            return BadRequest(new { mensagem = "Quantidade inválida para hábito concluído ❌" });

        var recorde = new HabitoRecorde
        {
            HabitId = dto.HabitId,
            Data = hoje,
            Quantidade = dto.Quantidade,
            Concluido = dto.Concluido,
            Observacao = dto.Observacao,
            CreatedAt = DateTime.UtcNow
        };

        _context.HabitoRecordes.InsertOne(recorde);

        // 🔥 ADICIONADO (USANDO FUNÇÃO)
        var (streakAtual, melhorStreak) = CalcularStreak(dto.HabitId);

        var update = Builders<Habito>.Update
            .Set(h => h.StreakAtual, streakAtual)
            .Set(h => h.MelhorStreak, melhorStreak);

        _context.Habitos.UpdateOne(h => h.Id == dto.HabitId, update);

        // 🔥 DESBLOQUEIO DE CONQUISTAS
        _conquistaService.VerificarConquistas(userId, dto.HabitId);
        
        if (dto.Concluido)
        {
            _conquistaService.VerificarConquistas(userId, dto.HabitId);
        }

        return Created("", recorde);
    }

    // =========================
    // 📌 LISTAR POR HÁBITO
    // =========================
    [HttpGet("{habitId}")]
    public IActionResult Listar(Guid habitId)
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (habitId == Guid.Empty)
            return BadRequest(new { mensagem = "HabitId inválido ❌" });

        var habito = _context.Habitos
            .Find(h => h.Id == habitId && h.UserId == userId)
            .FirstOrDefault();

        if (habito == null)
            return NotFound(new { mensagem = "Hábito não encontrado ❌" });

        var registros = _context.HabitoRecordes
            .Find(r => r.HabitId == habitId)
            .SortByDescending(r => r.Data)
            .ToList();

        return Ok(registros);
    }

    // =========================
    // 📅 CALENDÁRIO (GITHUB STYLE)
    // =========================
    [HttpGet("{habitId}/calendario")]
    public IActionResult Calendario(Guid habitId)
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var habito = _context.Habitos
            .Find(h => h.Id == habitId && h.UserId == userId)
            .FirstOrDefault();

        if (habito == null)
            return NotFound();

        var inicio = DateTime.UtcNow.Date.AddDays(-90);

        var registros = _context.HabitoRecordes
            .Find(r => r.HabitId == habitId && r.Data >= inicio)
            .ToList();

        var resultado = Enumerable.Range(0, 90)
            .Select(i =>
            {
                var dia = inicio.AddDays(i);

                var registro = registros
                    .FirstOrDefault(r => r.Data.Date == dia);

                return new
                {
                    data = dia,
                    concluido = registro?.Concluido ?? false
                };
            });

        return Ok(resultado);
    }

    // =========================
    // 📌 BUSCAR POR ID
    // =========================
    [HttpGet("registro/{id}")]
    public IActionResult BuscarPorId(Guid id)
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (id == Guid.Empty)
            return BadRequest(new { mensagem = "ID inválido ❌" });

        var registro = _context.HabitoRecordes
            .Find(r => r.Id == id)
            .FirstOrDefault();

        if (registro == null)
            return NotFound(new { mensagem = "Registro não encontrado ❌" });

        var habito = _context.Habitos
            .Find(h => h.Id == registro.HabitId && h.UserId == userId)
            .FirstOrDefault();

        if (habito == null)
            return Forbid();

        return Ok(registro);
    }

    // =========================
    // 📌 ATUALIZAR
    // =========================
    [HttpPut("{id}")]
    public IActionResult Atualizar(Guid id, [FromBody] CreateHabitoRecordeDTO dto)
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (id == Guid.Empty)
            return BadRequest();

        var registro = _context.HabitoRecordes
            .Find(r => r.Id == id)
            .FirstOrDefault();

        if (registro == null)
            return NotFound(new { mensagem = "Registro não encontrado ❌" });

        var habito = _context.Habitos
            .Find(h => h.Id == registro.HabitId && h.UserId == userId)
            .FirstOrDefault();

        if (habito == null)
            return Forbid();

        if (dto.Quantidade < 0)
            return BadRequest(new { mensagem = "Quantidade inválida ❌" });

        registro.Quantidade = dto.Quantidade;
        registro.Concluido = dto.Concluido;
        registro.Observacao = dto.Observacao;

        _context.HabitoRecordes.ReplaceOne(r => r.Id == id, registro);

        // 🔥 ADICIONADO
        var (streakAtual, melhorStreak) = CalcularStreak(registro.HabitId);

        var update = Builders<Habito>.Update
            .Set(h => h.StreakAtual, streakAtual)
            .Set(h => h.MelhorStreak, melhorStreak);

        _context.Habitos.UpdateOne(h => h.Id == registro.HabitId, update);

        _conquistaService.VerificarConquistas(userId, registro.HabitId);

        return Ok(registro);
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

        if (id == Guid.Empty)
            return BadRequest();

        var registro = _context.HabitoRecordes
            .Find(r => r.Id == id)
            .FirstOrDefault();

        if (registro == null)
            return NotFound(new { mensagem = "Registro não encontrado ❌" });

        var habito = _context.Habitos
            .Find(h => h.Id == registro.HabitId && h.UserId == userId)
            .FirstOrDefault();

        if (habito == null)
            return Forbid();

        _context.HabitoRecordes.DeleteOne(r => r.Id == id);

        // 🔥 ADICIONADO
        var (streakAtual, melhorStreak) = CalcularStreak(registro.HabitId);

        var update = Builders<Habito>.Update
            .Set(h => h.StreakAtual, streakAtual)
            .Set(h => h.MelhorStreak, melhorStreak);

        _context.Habitos.UpdateOne(h => h.Id == registro.HabitId, update);

        return Ok(new { mensagem = "Registro deletado com sucesso ✅" });
    }
}