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
        //private MongoClient client;
        private static ParseConfig config;
        private static bool isConnected = false;
        private static MongoClient client;
        private static IMongoDatabase database;
        public DBDriver()
        {
            config = new ParseConfig();
        }

        public void ConnectToDB()
        {
            if (!isConnected)
            {
                client = new MongoClient(config.GetConnectionString());
                database = client.GetDatabase(config.GetDatabase());
            }
        }

        public List<string> ReadDataFromDB()
        {
            ConnectToDB();
            IMongoCollection<Magnet> collection = database.GetCollection<Magnet>(config.GetCollection());
            return (from x in collection.AsQueryable<Magnet>() where (x.status == 0) select x.url).ToList();
        }

        public int GetRefreshTime()
        {
            return config.GetRefreshTime();
        }
    }

    class Magnet
    {
        public string id;
        public string url;
        public int status;
    }
}
