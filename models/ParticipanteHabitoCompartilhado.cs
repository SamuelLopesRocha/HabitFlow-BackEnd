using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ParticipanteHabitoCompartilhado
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonElement("habitoCompartilhadoId")]
    [BsonRepresentation(BsonType.String)]
    public Guid HabitoCompartilhadoId { get; set; }

    [BsonElement("usuarioId")]
    public string UsuarioId { get; set; }

    [BsonElement("papel")]
    public PapelParticipante Papel { get; set; }

    [BsonElement("entrouEm")]
    public DateTime EntrouEm { get; set; } = DateTime.UtcNow;
}