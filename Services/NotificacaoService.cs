public class NotificacaoService
{
    private readonly MongoDbContext _context;

    public NotificacaoService(MongoDbContext context)
    {
        _context = context;
    }

    // =========================
    // 🔔 MÉTODO BASE (GENÉRICO)
    // =========================
    public void Criar(string usuarioId, string titulo, string mensagem, TipoNotificacao tipo, Guid? habitoId = null)
    {
        var notificacao = new Notificacao
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            HabitoId = habitoId,
            Titulo = titulo,
            Mensagem = mensagem,
            Tipo = tipo,
            Lida = false,
            DataEnvio = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notificacoes.InsertOne(notificacao);
    }
    // =========================
    // ✅ CRIAR HÁBITO
    // =========================
    public void NotificarCriacaoHabito(string usuarioId, string nomeHabito, Guid habitoId)
    {
        Criar(
            usuarioId,
            "Novo hábito criado ✅",
            $"Você criou o hábito: {nomeHabito}",
            TipoNotificacao.Sistema,
            habitoId
        );
    }

    // =========================
    // 🏆 CONQUISTA
    // =========================
    public void NotificarConquista(string usuarioId, string nomeConquista, Guid conquistaId)
    {
        Criar(
            usuarioId,
            "Nova conquista desbloqueada 🏆",
            $"Você conquistou: {nomeConquista}",
            TipoNotificacao.Conquista,
            conquistaId
        );
    }

    // =========================
    // 🔥 STREAK
    // =========================
    public void NotificarStreak(string usuarioId, int dias)
    {
        Criar(
            usuarioId,
            "Sequência mantida 🔥",
            $"Você está há {dias} dias consecutivos!",
            TipoNotificacao.Sequencia
        );
    }

    // =========================
    // ⏰ LEMBRETE
    // =========================
    public void NotificarLembrete(string usuarioId, string nomeHabito, Guid habitId)
    {
        Criar(
            usuarioId,
            "Hora do hábito ⏰",
            $"Não esqueça de: {nomeHabito}",
            TipoNotificacao.Lembrete,
            habitId
        );
    }

    // =========================
    // 👥 GRUPO (FUTURO)
    // =========================
    public void NotificarEntradaGrupo(string usuarioId, string nomeUsuario, string nomeGrupo)
    {
        Criar(
            usuarioId,
            "Novo participante 👥",
            $"{nomeUsuario} entrou no grupo {nomeGrupo}",
            TipoNotificacao.Sistema
        );
    }

    // =========================
    // 💬 MENSAGEM (FUTURO)
    // =========================
    public void NotificarMensagem(string usuarioId, string remetente, string mensagem)
    {
        Criar(
            usuarioId,
            "Nova mensagem 💬",
            $"{remetente}: {mensagem}",
            TipoNotificacao.Sistema
        );
    }

    // =========================
    // 💬 Habito Concluido
    // =========================
    public void NotificarHabitoConcluido(string usuarioId, string nomeHabito, Guid habitoId)
    {
        Criar(
            usuarioId,
            "Hábito concluído ✅",
            $"Você concluiu: {nomeHabito}",
            TipoNotificacao.Sistema,
            habitoId
        );
    }

    // =========================
    // 🗑️ REGISTRO DELETADO
    // =========================
    public void NotificarRegistroDeletado(string usuarioId, string nomeHabito, Guid habitoId)
    {
        Criar(
            usuarioId,
            "Progresso removido ⚠️",
            $"Você removeu um registro do hábito: {nomeHabito}",
            TipoNotificacao.Sistema,
            habitoId
        );
    }

    // =========================
    // 🗑️ HÁBITO REMOVIDO
    // =========================
    public void NotificarHabitoRemovido(string usuarioId, string nomeHabito, Guid habitoId)
    {
        Criar(
            usuarioId,
            "Hábito removido 🗑️",
            $"Você removeu o hábito: {nomeHabito}",
            TipoNotificacao.Sistema,
            habitoId
        );
    }
}
