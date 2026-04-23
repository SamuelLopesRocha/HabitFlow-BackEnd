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

        // 🔔 NOTIFICAR PARTICIPANTES
        foreach (var participanteId in chat.Participantes)
        {
            if (participanteId == userId) continue;

            _notificacaoService.NotificarEntradaGrupo(
                participanteId,
                "Novo chat",
                chat.Nome ?? "Chat"
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
}