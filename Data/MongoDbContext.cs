using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDBSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);

        // 🔥 CRIAR INDEX ÚNICO PARA USERNAME
        var indexKeys = Builders<Usuario>.IndexKeys.Ascending(u => u.Username);
        var indexOptions = new CreateIndexOptions
        {
            Unique = true,
            Collation = new Collation("en", strength: CollationStrength.Secondary)
        };

        Usuarios.Indexes.CreateOne(
            new CreateIndexModel<Usuario>(indexKeys, indexOptions)
        );
    }

    public IMongoCollection<Usuario> Usuarios =>
        _database.GetCollection<Usuario>("Usuarios");

    public IMongoCollection<Habito> Habitos =>
        _database.GetCollection<Habito>("Habitos");

    public IMongoCollection<HabitoRecorde> HabitoRecordes =>
        _database.GetCollection<HabitoRecorde>("HabitoRecordes");

    public IMongoCollection<AgendaHabito> AgendaHabitos =>
        _database.GetCollection<AgendaHabito>("AgendaHabitos");

    public IMongoCollection<Conquista> Conquistas =>
        _database.GetCollection<Conquista>("Conquistas");

    public IMongoCollection<UsuarioConquista> UsuarioConquistas =>
        _database.GetCollection<UsuarioConquista>("UsuarioConquistas");

    public IMongoCollection<Notificacao> Notificacoes =>
        _database.GetCollection<Notificacao>("Notificacoes");

    public IMongoCollection<Chat> Chats =>
        _database.GetCollection<Chat>("Chats");
}