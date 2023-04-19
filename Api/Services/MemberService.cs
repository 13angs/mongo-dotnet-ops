using Api.Interfaces;
using Api.Paramters;
using Api.Stores;
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
            var pipeline = new BsonDocument[]{};
            if(param.Type == MemberQueryStore.ByChannel)
            {
                pipeline = new BsonDocument[]
                {
                    new BsonDocument("$match", new BsonDocument("channels.channel_id", param.ChannelId)),
                    new BsonDocument{
                        { 
                            "$lookup", new BsonDocument{ 
                                {"from", "wn_omni_channels"}, 
                                { "localField", "channels.channel_id"}, 
                                { "foreignField", "_id" }, 
                                { "as", "channel_info" } 
                            } 
                        }
                    },
                    new BsonDocument{
                        { "$unwind", "$channel_info" }
                    },
                    BsonDocument.Parse("{ $addFields: { 'channels': [ { 'channel_id': '$channel_info._id', 'channel_name': '$channel_info.channel_name', 'client_id': '$channel_info.client_id' } ] } }"),
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "member_id", "$_id" },
                        { "_id", 0 },
                        { "channels", 1 },
                        { "created_date", new BsonDocument("$dateToString", new BsonDocument
                            {
                                { "format", "%Y-%m-%dT%H:%M:%S.%LZ" },
                                { "date", "$created_date" }
                            })
                        },
                        { "modified_date", new BsonDocument("$dateToString", new BsonDocument
                            {
                                { "format", "%Y-%m-%dT%H:%M:%S.%LZ" },
                                { "date", "$modified_date" }
                            })
                        },
                        { "member_name", "$member.name" },
                        { "line_profile", new BsonDocument{
                            { "follow_date", new BsonDocument("$dateToString", new BsonDocument
                                {
                                    { "format", "%Y-%m-%dT%H:%M:%S.%LZ" },
                                    { "date", "$created_date" }
                                })
                            }
                        } },
                    }),
                    new BsonDocument("$limit", param.Limit) // add limit here
                };
            }
            if(param.Type == MemberQueryStore.ByProvider)
            {
                pipeline = new BsonDocument[]
                {
                    new BsonDocument{
                        { 
                            "$lookup", new BsonDocument{ 
                                { "from", "wn_omni_channels" }, 
                                { "localField", "channels.channel_id"}, 
                                { "foreignField", "_id" }, 
                                { "as", "channel" } 
                            } 
                        }
                    },
                    new BsonDocument{
                        { 
                            "$lookup", new BsonDocument{ 
                                { "from", "wn_omni_providers" }, 
                                { "localField", "channel.provider_id"}, 
                                { "foreignField", "_id" }, 
                                { "as", "provider" } 
                            } 
                        }
                    },
                    new BsonDocument("$match", new BsonDocument("provider._id", param.ProviderId)),
                    new BsonDocument{
                        { "$unwind", "$channel" }
                    },
                    BsonDocument.Parse("{ $addFields: { 'channels': [ { 'channel_id': '$channel._id', 'channel_name': '$channel.channel_name', 'client_id': '$channel.client_id' } ] } }"),
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "member_id", "$_id" },
                        { "_id", 0 },
                        { "channels", 1 },
                        { "member_name", 1 },
                        { "created_date", new BsonDocument("$dateToString", new BsonDocument
                            {
                                { "format", "%Y-%m-%dT%H:%M:%S.%LZ" },
                                { "date", "$created_date" }
                            })
                        },
                        { "modified_date", new BsonDocument("$dateToString", new BsonDocument
                            {
                                { "format", "%Y-%m-%dT%H:%M:%S.%LZ" },
                                { "date", "$modified_date" }
                            })
                        },
                        { "line_profile", new BsonDocument{
                            { "follow_date", new BsonDocument("$dateToString", new BsonDocument
                                {
                                    { "format", "%Y-%m-%dT%H:%M:%S.%LZ" },
                                    { "date", "$created_date" }
                                })
                            }
                        } },
                    }),
                    new BsonDocument("$limit", param.Limit) // add limit here
                };
            }

            List<BsonDocument> pResults = _memberCols.Aggregate<BsonDocument>(pipeline).ToList();
            // var members = _memberCols.Find(_ => true)
            //     .ToEnumerable()
            //     .Take(param.Limit);
            return pResults.ToJson();
        }
    }
}