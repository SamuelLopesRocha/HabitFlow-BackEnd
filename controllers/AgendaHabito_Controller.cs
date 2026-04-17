using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using HabitFlow___BackEnd.Enums;

[ApiController]
[Route("api/[controller]")]
public class AgendaHabitoController : ControllerBase
{
    private readonly MongoDbContext _context;

    public AgendaHabitoController(MongoDbContext context)
    {
        _context = context;
    }

    // =========================
    // 📌 CRIAR AGENDA
    // =========================
    [HttpPost]
    public IActionResult Criar(AgendaHabito agenda)
    {
        if (agenda.HabitoId == Guid.Empty)
            return BadRequest("HabitoId é obrigatório");

        // 🔍 Verifica se o hábito existe
        var habitoExiste = _context.Habitos
            .Find(h => h.Id == agenda.HabitoId)
            .FirstOrDefault();

        if (habitoExiste == null)
            return NotFound("Hábito não encontrado");

        // evitar duplicidade
        var existe = _context.AgendaHabitos
            .Find(a => a.HabitoId == agenda.HabitoId && a.DiaSemana == agenda.DiaSemana)
            .FirstOrDefault();

        if (existe != null)
            return Conflict("Esse dia já foi cadastrado para esse hábito");

        agenda.Id = Guid.NewGuid();

        _context.AgendaHabitos.InsertOne(agenda);

        return Ok(new
        {
            mensagem = "Agenda criada com sucesso",
            dados = agenda
        });
    }

    // =========================
    // 📌 LISTAR POR HABITO
    // =========================
    [HttpGet("{habitoId}")]
    public IActionResult Listar(Guid habitoId)
    {
        if (habitoId == Guid.Empty)
            return BadRequest("HabitoId inválido");

        var lista = _context.AgendaHabitos
            .Find(a => a.HabitoId == habitoId)
            .ToList();

        return Ok(lista);
    }

    // =========================
    // 📌 DELETAR
    // =========================
    [HttpDelete("{habitoId}/{diaSemana}")]
    public IActionResult Deletar(Guid habitoId, DiaSemana diaSemana)
    {
        var resultado = _context.AgendaHabitos
            .DeleteOne(a => a.HabitoId == habitoId && a.DiaSemana == diaSemana);

        if (resultado.DeletedCount == 0)
            return NotFound("Registro não encontrado");

        return Ok("Registro deletado com sucesso");
    }
    
    // =========================
    // 📌 Multiplos dias
    // =========================
    [HttpPost("multiplos")]
    public IActionResult CriarMultiplos(Guid habitoId, List<DiaSemana> diasSemana)
    {
        if (habitoId == Guid.Empty)
            return BadRequest("HabitoId é obrigatório");

        var habitoExiste = _context.Habitos
            .Find(h => h.Id == habitoId)
            .FirstOrDefault();

        if (habitoExiste == null)
            return NotFound("Hábito não encontrado");

        var agendasCriadas = new List<AgendaHabito>();

        foreach (var dia in diasSemana)
        {
            var existe = _context.AgendaHabitos
                .Find(a => a.HabitoId == habitoId && a.DiaSemana == dia)
                .FirstOrDefault();

            if (existe == null)
            {
                var novaAgenda = new AgendaHabito
                {
                    Id = Guid.NewGuid(),
                    HabitoId = habitoId,
                    DiaSemana = dia,
                    Ativo = true
                };

                _context.AgendaHabitos.InsertOne(novaAgenda);
                agendasCriadas.Add(novaAgenda);
            }
            else
            {
                var update = Builders<AgendaHabito>.Update
                    .Set(a => a.Ativo, true)
                    .Set(a => a.UpdatedAt, DateTime.UtcNow);

                _context.AgendaHabitos.UpdateOne(
                    a => a.Id == existe.Id,
                    update
                );        
            }
        }
        return Ok(new
        {
            mensagem = "Agendas criadas com sucesso",
            total = agendasCriadas.Count,
            dias = agendasCriadas.Select(a => a.DiaSemana)
        });
    }

    // =========================
    // 📌 TOGGLE AUTOMÁTICO
    // =========================
    [HttpPut("toggle")]
    public IActionResult Toggle(Guid habitoId, DiaSemana diaSemana)
    {
        if (habitoId == Guid.Empty)
            return BadRequest("HabitoId é obrigatório");

        // 🔍 Verifica se o hábito existe
        var habitoExiste = _context.Habitos
            .Find(h => h.Id == habitoId)
            .FirstOrDefault();

        if (habitoExiste == null)
            return NotFound("Hábito não encontrado");

        var filtro = Builders<AgendaHabito>.Filter.Where(a =>
            a.HabitoId == habitoId && a.DiaSemana == diaSemana);

        var agendaExistente = _context.AgendaHabitos
            .Find(filtro)
            .FirstOrDefault();

        if (agendaExistente == null)
            return NotFound("Agenda não encontrada para esse dia");

        // 🔥 AQUI ESTÁ O TOGGLE AUTOMÁTICO
        var novoStatus = !agendaExistente.Ativo;

        var update = Builders<AgendaHabito>.Update
            .Set(a => a.Ativo, novoStatus)
            .Set(a => a.UpdatedAt, DateTime.UtcNow);

        _context.AgendaHabitos.UpdateOne(filtro, update);

        return Ok(new
        {
            mensagem = "Status alternado com sucesso",
            antes = agendaExistente.Ativo,
            agora = novoStatus,
            diaSemana = diaSemana
        });
    }
}