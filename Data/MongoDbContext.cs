using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDBSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
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
}