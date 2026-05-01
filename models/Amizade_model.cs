using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Amizade
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("usuarioId")]
    public string UsuarioId { get; set; } // quem enviou

    [BsonElement("amigoId")]
    public string AmigoId { get; set; } // quem recebeu

    [BsonElement("status")]
    public StatusAmizade Status { get; set; }

    [BsonElement("dataSolicitacao")]
    public DateTime DataSolicitacao { get; set; }

    [BsonElement("dataResposta")]
    public DateTime? DataResposta { get; set; }

    [BsonElement("respondidoPorId")]
    public string? RespondidoPorId { get; set; } // quem aceitou/recusou/bloqueou
}