public class Notificacao
{
    public Guid Id { get; set; }

    public string UsuarioId { get; set; } = string.Empty;

    public Guid? HabitoId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Mensagem { get; set; } = string.Empty;

    public TipoNotificacao Tipo { get; set; }

    public bool Lida { get; set; } = false;

    public DateTime DataEnvio { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DataLeitura { get; set; }
}