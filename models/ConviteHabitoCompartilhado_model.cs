using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ConviteHabitoCompartilhado
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonElement("habitoCompartilhadoId")]
    [BsonRepresentation(BsonType.String)]
    public Guid HabitoCompartilhadoId { get; set; }

    [BsonElement("usuarioConvidadoId")]
    public string UsuarioConvidadoId { get; set; }

    [BsonElement("usuarioConvidadorId")]
    public string UsuarioConvidadorId { get; set; }

    [BsonElement("status")]
    public StatusConvite Status { get; set; }

    [BsonElement("criadoEm")]
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    [BsonElement("respondidoEm")]
    public DateTime? RespondidoEm { get; set; }
}