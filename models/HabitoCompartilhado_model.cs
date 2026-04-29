using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class HabitoCompartilhado
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonElement("habitId")]
    [BsonRepresentation(BsonType.String)]
    public Guid HabitId { get; set; }

    [BsonElement("nome")]
    public string Nome { get; set; }

    [BsonElement("criadoPorUsuarioId")]
    public string CriadoPorUsuarioId { get; set; }

    [BsonElement("metaCompartilhada")]
    public int MetaCompartilhada { get; set; }

    [BsonElement("tipo")]
    public TipoHabitoCompartilhado Tipo { get; set; }

    [BsonElement("periodo")]
    public PeriodoHabito Periodo { get; set; }

    [BsonElement("criadoEm")]
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    [BsonElement("ativo")]
    public bool Ativo { get; set; } = true;
}