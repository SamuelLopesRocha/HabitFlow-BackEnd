using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class Habito
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserId { get; set; }

    public string Nome { get; set; }
    public string Descricao { get; set; }
    public string Cor { get; set; }
    public string Icone { get; set; }

    public string Tipo { get; set; } // Diario, Semanal, Mensal

    public int Frequencia { get; set; }
    public int Meta { get; set; }

    public string Unidade { get; set; }

    public string HoraPreferida { get; set; }

    public DateTime CriadoEm { get; set; }
    public bool NotificacaoAtiva { get; set; }

    public string Prioridade { get; set; } // Baixa, Média, Alta

    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }

    public int StreakAtual { get; set; } = 0;
    public int MelhorStreak { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool Ativo { get; set; } = true;
}