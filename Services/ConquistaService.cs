using MongoDB.Driver;

public class ConquistaService
{
    private readonly MongoDbContext _context;
    private readonly NotificacaoService _notificacaoService;

    public ConquistaService(
        MongoDbContext context,
        NotificacaoService notificacaoService
    )
    {
        _context = context;
        _notificacaoService = notificacaoService;
    }

    public void VerificarConquistas(string userId, Guid habitId)
    {
        var habito = _context.Habitos
            .Find(h => h.Id == habitId)
            .FirstOrDefault();

        if (habito == null) return;

        var (streakAtual, melhorStreak) = CalcularStreak(habitId);

        var totalConclusoes = _context.HabitoRecordes
            .CountDocuments(r => r.HabitId == habitId && r.Concluido);

        var conquistas = _context.Conquistas.Find(_ => true).ToList();

        foreach (var conquista in conquistas)
        {
            bool desbloquear = false;

            switch (conquista.Tipo)
            {
                case TipoConquista.Sequencia:
                    if (melhorStreak >= conquista.ValorMeta)
                        desbloquear = true;
                    break;

                case TipoConquista.Consistencia:
                    if (streakAtual >= conquista.ValorMeta)
                        desbloquear = true;
                    break;

                case TipoConquista.Volume:
                    if (totalConclusoes >= conquista.ValorMeta)
                        desbloquear = true;
                    break;
            }

            if (!desbloquear) continue;

            var jaTem = _context.UsuarioConquistas
                .Find(u => u.UserId == userId && u.ConquistaId == conquista.Id)
                .FirstOrDefault();

            if (jaTem != null) continue;

            var nova = new UsuarioConquista
            {
                UserId = userId,
                ConquistaId = conquista.Id
            };

            _context.UsuarioConquistas.InsertOne(nova);

            // 🔔 NOTIFICAÇÃO DE CONQUISTA
            _notificacaoService.NotificarConquista(
                userId,
                conquista.Nome,
                conquista.Id
            );
        }
    }

    // 🔥 Copia a mesma lógica que você já tem
    private (int streakAtual, int melhorStreak) CalcularStreak(Guid habitId)
    {
        var registros = _context.HabitoRecordes
            .Find(r => r.HabitId == habitId && r.Concluido)
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
        var dias = registros.Select(r => r.Data.Date).ToHashSet();

        while (dias.Contains(hoje))
        {
            streakAtual++;
            hoje = hoje.AddDays(-1);
        }

        return (streakAtual, melhorStreak);
    }
}