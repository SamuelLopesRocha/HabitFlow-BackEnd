using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;

[ApiController]
[Route("api/mensagem")]
public class MensagemController : ControllerBase
{
    private readonly MensagemService _mensagemService;

    public MensagemController(MensagemService mensagemService)
    {
        _mensagemService = mensagemService;
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
}