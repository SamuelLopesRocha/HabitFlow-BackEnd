using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;

[ApiController]
[Route("api/mensagem")]
public class MensagemController : ControllerBase
{
    private readonly MensagemService _mensagemService;
    private readonly MongoDbContext _context;

    public MensagemController(MensagemService mensagemService, MongoDbContext context)
    {
        _mensagemService = mensagemService;
        _context = context;
    }

    [HttpPost("{chatId}")]
    public async Task<IActionResult> Enviar(Guid chatId, [FromBody] string conteudo)
    {
        var userId = User.FindFirst("id")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var msg = await _mensagemService.Enviar(chatId, userId, conteudo);
            return Ok(msg);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // 🔥 ADICIONA ISSO
    [HttpGet("{chatId}")]
    public IActionResult BuscarMensagens(Guid chatId)
    {
        try
        {
            var mensagens = _mensagemService.BuscarPorChat(chatId);

            var response = mensagens.Select(m =>
            {
                var usuario = _context.Usuarios
                    .Find(u => u.Id == m.UsuarioId)
                    .FirstOrDefault();

                return new MensagemResponseDTO
                {
                    Id = m.Id,
                    ChatId = m.ChatId,
                    UsuarioId = m.UsuarioId,
                    Username = usuario?.Username,
                    Conteudo = m.Conteudo,
                    Tipo = m.Tipo,
                    Lida = m.Lida,
                    CriadoEm = m.CriadoEm
                };
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}