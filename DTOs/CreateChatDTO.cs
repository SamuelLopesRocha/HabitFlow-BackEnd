public class CreateChatDTO
{
    public TipoChat Tipo { get; set; }

    public string? Nome { get; set; }

    // USERNAMES dos participantes, o backend vai converter para IDs
    public List<string> Usernames { get; set; }
}