public class Mensagem
{
    public Guid Id { get; set; }

    public Guid ChatId { get; set; }        // FK
    public string UsuarioId { get; set; }   // FK

    public string Conteudo { get; set; }
    public TipoMensagem Tipo { get; set; }  // Texto, Imagem, Sistema

    public bool Lida { get; set; }

    public DateTime CriadoEm { get; set; }
}