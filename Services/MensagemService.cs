using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

public class MensagemService
{
    private readonly MongoDbContext _context;
    private readonly NotificacaoService _notificacaoService;

    private readonly IHubContext<ChatHub> _hub;

    public MensagemService(MongoDbContext context, NotificacaoService notificacaoService, IHubContext<ChatHub> hub)
    {
        _context = context;
        _notificacaoService = notificacaoService;
        _hub = hub;
    }

    public async Task<Mensagem> Enviar (Guid chatId, string usuarioId, string conteudo)
    {
        var chat = _context.Chats.Find(c => c.Id == chatId).FirstOrDefault();

        if (chat == null)
            throw new Exception("Chat não encontrado");

        if (!chat.Participantes.Contains(usuarioId))
            throw new Exception("Você não participa desse chat");

        var mensagem = new Mensagem
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            UsuarioId = usuarioId,
            Conteudo = conteudo,
            Tipo = TipoMensagem.Texto,
            Lida = false,
            CriadoEm = DateTime.UtcNow
        };

        _context.Mensagens.InsertOne(mensagem);

        // 👇 COLOCA AQUI
        await _hub.Clients.Group(chatId.ToString())
            .SendAsync("ReceberMensagem", mensagem);

        // 🔍 nome do remetente
        var remetente = _context.Usuarios
            .Find(u => u.Id == usuarioId)
            .FirstOrDefault()?.Username ?? "Alguém";

        // 🔔 NOTIFICAR OUTROS
        foreach (var participanteId in chat.Participantes)
        {
            if (participanteId == usuarioId) continue;

            _notificacaoService.Criar(
                participanteId,
                "Nova mensagem 💬",
                $"{remetente}: {conteudo}",
                TipoNotificacao.Sistema
            );
        }

        return mensagem;
    }
}