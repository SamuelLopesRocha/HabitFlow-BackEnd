using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;

    public ChatController(ChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost]
    public IActionResult Criar([FromBody] CreateChatDTO dto)
    {
        // 🔴 VALIDAÇÕES BÁSICAS
        if (dto == null)
            return BadRequest("Dados inválidos");

        if (dto.Usernames == null || dto.Usernames.Count == 0)
            return BadRequest("Informe ao menos um Usernames para o chat");

        // pegar userId do token (igual vc já usa)
        var userId = User.FindFirst("id")?.Value;
        
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var chat = _chatService.Criar(dto, userId);
            return Ok(chat);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public IActionResult BuscarMeusChats()
    {
        var userId = User.FindFirst("id")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var chats = _chatService.BuscarPorUsuario(userId);

        return Ok(chats);
    }

    [HttpPost("{chatId}/sair")]
    public IActionResult SairDoChat(Guid chatId)
    {
        var userId = User.FindFirst("id")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            _chatService.SairDoChat(chatId, userId);
            return Ok("Você saiu do chat");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{chatId}")]
    public IActionResult DeletarChat(Guid chatId)
    {
        var userId = User.FindFirst("id")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            _chatService.DeletarChat(chatId, userId);
            return Ok("Chat deletado com sucesso");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}