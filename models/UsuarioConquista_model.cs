using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class UsuarioConquista
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserId { get; set; }

    public Guid ConquistaId { get; set; }

    public DateTime DataDesbloqueio { get; set; } = DateTime.UtcNow;
}