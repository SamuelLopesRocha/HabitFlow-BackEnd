using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class HabitoRecorde
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonElement("habitId")]
    [BsonRepresentation(BsonType.String)]
    public Guid HabitId { get; set; } // FK do hábito

    [BsonElement("usuarioId")] // 🔥 CORREÇÃO
    public string UserId { get; set; }

    [BsonElement("data")]
    public DateTime Data { get; set; } // DateOnly adaptado para Mongo

    [BsonElement("quantidade")]
    public int Quantidade { get; set; } // ex: 2 (litros)

    [BsonElement("concluido")]
    public bool Concluido { get; set; }

    [BsonElement("observacao")]
    public string Observacao { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}