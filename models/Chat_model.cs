using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Chat
{
    [BsonId]
    [BsonRepresentation(BsonType.Binary)]
    public Guid Id { get; set; }

    public TipoChat Tipo { get; set; }

    public string? Nome { get; set; } // null se for privado

    public List<string> Participantes { get; set; } = new List<string>();

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}