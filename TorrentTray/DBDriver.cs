using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TorrentClient;

namespace TorrentTray
{
    
    class DBDriver
    {
        static public readonly int WAITING_STATUS = 0;
        static public readonly int DOWNLOADING_STATUS = 1;
        static public readonly int FINISHED_STATUS = 2;


        private static ParseConfig config;
        private static bool IS_CONENCTED = false;
        private static MongoClient client;
        private static IMongoDatabase database;
        public DBDriver()
        {
            config = new ParseConfig();
            ConnectToDB();
        }

        public void ConnectToDB()
        {
            if (!IS_CONENCTED)
            {
                client = new MongoClient(config.GetConnectionString());
                database = client.GetDatabase(config.GetDatabase());
                IS_CONENCTED = true;
            }
        }

        public List<string> ReadHashsFromDB()
        {
            //ConnectToDB();
            IMongoCollection<Magnet> collection = database.GetCollection<Magnet>(config.GetCollection());
            return (from x in collection.AsQueryable<Magnet>() where (x.status == WAITING_STATUS) select x.hash).ToList();
        }
        public bool UpdateStatusFromDB(string hash, int status)
        {
            var filter = Builders<Magnet>.Filter.Eq("hash", hash);
            var update = Builders<Magnet>.Update.Set("status", status);

            UpdateResult result = database.GetCollection<Magnet>(config.GetCollection()).UpdateOne(filter, update);
      
            return result.IsAcknowledged;
        }
    }

    class Magnet
    {
        public string id;
        public string hash;
        public int status; //0 = to be downloaded; 1 = downloading
    }
}
