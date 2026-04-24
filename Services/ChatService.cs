using MongoDB.Driver;

public class ChatService
{
    private readonly MongoDbContext _context;
    private readonly NotificacaoService _notificacaoService;

    public ChatService(MongoDbContext context, NotificacaoService notificacaoService)
    {
        _context = context;
        _notificacaoService = notificacaoService;
    }

    public Chat Criar(CreateChatDTO dto, string userId)
    {
        // 🔍 buscar usuários pelo username
        var usuarios = _context.Usuarios
            .Find(u => dto.Usernames.Contains(u.Username))
            .ToList();

        if (usuarios.Count != dto.Usernames.Count)
            throw new Exception("Digite um username válido para cada participante");

        // 🔥 converter para IDs
        var participantesIds = usuarios.Select(u => u.Id).ToList();

        // garantir que quem criou está incluso
        if (!participantesIds.Contains(userId))
            participantesIds.Add(userId);

        participantesIds = participantesIds.Distinct().ToList();

        // 🔴 CHAT PRIVADO
        if (dto.Tipo == TipoChat.Privado)
        {
            if (participantesIds.Count != 2)
                throw new Exception("Chat privado precisa de 2 participantes");

            var existente = _context.Chats.Find(c =>
                c.Tipo == TipoChat.Privado &&
                c.Participantes.Count == 2 &&
                c.Participantes.All(p => participantesIds.Contains(p))
            ).FirstOrDefault();

            if (existente != null)
                throw new Exception("Chat privado já existe");
        }

        // 🔴 CHAT GRUPO
        if (dto.Tipo == TipoChat.Grupo)
        {
            if (string.IsNullOrEmpty(dto.Nome))
                throw new Exception("Grupo precisa de nome");
        }

        // 🔴 CRIAR CHAT
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Tipo = dto.Tipo,
            Nome = dto.Nome,
            Participantes = participantesIds, // ✅ agora correto
            CriadoEm = DateTime.UtcNow
        };

        _context.Chats.InsertOne(chat);

        // 🔍 pegar nome de quem criou
        var criador = _context.Usuarios
            .Find(u => u.Id == userId)
            .FirstOrDefault()?.Username ?? "Alguém";

        // 🔔 NOTIFICAR PARTICIPANTES
        foreach (var participanteId in chat.Participantes)
        {
            if (participanteId == userId) continue;

            var titulo = chat.Tipo == TipoChat.Privado 
                ? "Nova conversa 💬" 
                : "Novo grupo 👥";

            var mensagem = chat.Tipo == TipoChat.Privado
                ? $"{criador} iniciou uma conversa com você"
                : $"{criador} te adicionou ao grupo \"{chat.Nome ?? "Sem nome"}\"";

            _notificacaoService.Criar(
                participanteId,
                titulo,
                mensagem,
                TipoNotificacao.Sistema
            );
        }
        return chat;
    }

    public List<Chat> BuscarPorUsuario(string userId)
    {
        return _context.Chats
            .Find(c => c.Participantes.Contains(userId)) // ✅ corrigido
            .ToList();
    }

    // =========================
    // 🚪 SAIR DO CHAT
    // =========================
    public void SairDoChat(Guid chatId, string userId)
    {
        var chat = _context.Chats.Find(c => c.Id == chatId).FirstOrDefault();

        if (chat == null)
            throw new Exception("Chat não encontrado");

        if (!chat.Participantes.Contains(userId))
            throw new Exception("Você não participa desse chat");

        // remover usuário
        chat.Participantes.Remove(userId);

        // atualizar no banco
        _context.Chats.ReplaceOne(c => c.Id == chatId, chat);

        // pegar nome de quem saiu
        var usuario = _context.Usuarios
            .Find(u => u.Id == userId)
            .FirstOrDefault()?.Username ?? "Alguém";

        // notificar outros
        foreach (var participanteId in chat.Participantes)
        {
            var mensagem = chat.Tipo == TipoChat.Privado
                ? $"{usuario} saiu da conversa"
                : $"{usuario} saiu do grupo \"{chat.Nome ?? "Sem nome"}\"";

            _notificacaoService.Criar(
                participanteId,
                "Atualização no chat 📢",
                mensagem,
                TipoNotificacao.Sistema
            );
        }
    }

    // =========================
    // 🗑️ DELETAR CHAT
    // =========================
    public void DeletarChat(Guid chatId, string userId)
    {
        var chat = _context.Chats.Find(c => c.Id == chatId).FirstOrDefault();

        if (chat == null)
            throw new Exception("Chat não encontrado");

        // opcional: só quem criou pode deletar (se quiser depois)
        
        // pegar nome de quem deletou
        var usuario = _context.Usuarios
            .Find(u => u.Id == userId)
            .FirstOrDefault()?.Username ?? "Alguém";

        // notificar antes de deletar
        foreach (var participanteId in chat.Participantes)
        {
            if (participanteId == userId) continue;

            _notificacaoService.Criar(
                participanteId,
                "Chat removido 🗑️",
                $"{usuario} deletou o chat \"{chat.Nome ?? "Sem nome"}\"",
                TipoNotificacao.Sistema
            );
        }

        _context.Chats.DeleteOne(c => c.Id == chatId);
    }
}