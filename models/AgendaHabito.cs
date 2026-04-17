using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using HabitFlow___BackEnd.Enums;

public class AgendaHabito
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonRepresentation(BsonType.String)]
    public Guid HabitoId { get; set; }

    public DiaSemana DiaSemana { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}