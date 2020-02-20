using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentTray
{
    class ParseConfig
    {
        private string connectionString = null;
        private string collection = null;
        private int refreshTime = 0;
        private Config configsParsed;

        public ParseConfig()
        {
            LoadJson();
            connectionString = $"mongodb://{configsParsed.username}:{configsParsed.password}@{configsParsed.mongoServer}/{configsParsed.database}";
            collection = configsParsed.collection;
            refreshTime = configsParsed.refreshTime;
        }
        public string getConnectionString()
        {
            return connectionString;
        }
        public string getCollection()
        {
            return collection;
        }
        public int getRefreshTime()
        {
            return refreshTime;
        }

        private void LoadJson()
        {
            using (StreamReader r = new StreamReader("Config.json"))
            {
                string json = r.ReadToEnd();
                configsParsed = JsonConvert.DeserializeObject<Config>(json);
            }
        }
    }

    class Config
    {
        public string mongoServer;
        public string database;
        public string collection;
        public string username;
        public string password;
        public int refreshTime;
    }
}
