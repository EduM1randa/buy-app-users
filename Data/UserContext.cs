using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class UserContext {
  private readonly IMongoDatabase _database;

  public UserContext(IOptions<MongoDBSettings> settings) {
    var client = new MongoClient(settings.Value.ConnectionString);
    _database = client.GetDatabase(settings.Value.DatabaseName);  
  }

  public IMongoCollection<User> Users => _database.GetCollection<User>("users");
}