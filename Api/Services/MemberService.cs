using Api.Interfaces;
using Api.Paramters;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Api.Services
{
    public class MemberService : IMember
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<BsonDocument> _memberCols;
        public MemberService(IConfiguration configuration)
        {
            _configuration = configuration;
            IMongoClient mongoClient = new MongoClient(_configuration["MongoConfig:ConnectionString"]);
            IMongoDatabase mongoDb = mongoClient.GetDatabase(_configuration["MongoConfig:DbName"]);
            _memberCols = mongoDb.GetCollection<BsonDocument>(_configuration["MongoConfig:Cols:Member"]);
            // get a list of databases
            var databases = mongoClient.ListDatabases().ToList();

            // check if the client is connected to MongoDB by checking the count of databases
            if (databases.Count > 0)
            {
                // Console.WriteLine(cols.FirstOrDefault().ToJson());
                Console.WriteLine("Connected to MongoDB");
            }
            else
            {
                Console.WriteLine("Not connected to MongoDB");
            }
        }
        public string GetAllMember(MemberParam param)
        { 
            // var filter = Builders<BsonDocument>.Filter.Eq("_id", "3d7400d214d54cbeb7833fd277d65e95");
            var members = _memberCols.Find(_ => true)
                .ToEnumerable()
                .Take(param.Limit);
            // long count = _memberCols.CountDocuments(_ => true);
            // Console.WriteLine($"{members.ToJson()}, {count}");
            return members.ToJson();
        }
    }
}