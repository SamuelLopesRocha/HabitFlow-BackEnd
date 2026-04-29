public class CreateHabitoCompartilhadoDTO
{
    public Guid HabitId { get; set; }
    public string Nome { get; set; }
    public int MetaCompartilhada { get; set; }
    public TipoHabitoCompartilhado Tipo { get; set; }
    public PeriodoHabito Periodo { get; set; }
}