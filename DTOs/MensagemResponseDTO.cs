public class MensagemResponseDTO
{
    public Guid Id { get; set; }

    public Guid ChatId { get; set; }

    public string UsuarioId { get; set; }

    public string Username { get; set; }

    public string Conteudo { get; set; }

    public TipoMensagem Tipo { get; set; }

    public bool Lida { get; set; }

    public DateTime CriadoEm { get; set; }
}