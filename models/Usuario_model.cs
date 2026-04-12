using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Usuario
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string? Nome { get; set; }

    [Required]
    public string Username { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Senha { get; set; } = null!; // 🔥 vem do front

    public string? SenhaHash { get; set; } // 🔐 salvo no banco

    public string? FotoPerfilUrl { get; set; }

    public DateTime? DataNascimento { get; set; }

    public string Idioma { get; set; } = "pt-BR";

    public string FusoHorario { get; set; } = "America/Sao_Paulo";

    public string TemaPreferido { get; set; } = "dark";

    public string? ResetPasswordToken { get; set; }

    public string? EmailConfirmationToken { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string? CodigoConfirmacaoEmail { get; set; }

    public DateTime? CodigoConfirmacaoExpira { get; set; }

    public bool EmailConfirmado { get; set; } = false;
}