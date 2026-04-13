public class CreateHabitoDTO
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public string Cor { get; set; }
    public string Icone { get; set; }
    public string Tipo { get; set; }
    public int Frequencia { get; set; }
    public int Meta { get; set; }
    public string Unidade { get; set; }
    public string HoraPreferida { get; set; }
    public bool NotificacaoAtiva { get; set; }
    public string Prioridade { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}