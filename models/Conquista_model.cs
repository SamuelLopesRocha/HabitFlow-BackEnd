public class Conquista
{
    public Guid Id { get; set; }

    public string Nome { get; set; }

    public string Descricao { get; set; }

    public string Icone { get; set; }

    public TipoConquista Tipo { get; set; }

    public int ValorMeta { get; set; }
}