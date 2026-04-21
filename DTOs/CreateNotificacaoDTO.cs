public class CriarNotificacaoDto
{
    public Guid? HabitoId { get; set; }

    public string Titulo { get; set; }

    public string Mensagem { get; set; }

    public TipoNotificacao Tipo { get; set; }
}