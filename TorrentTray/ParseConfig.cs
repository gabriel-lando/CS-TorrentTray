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
        private static Config configsParsed;

        public ParseConfig()
        {
            LoadJson();
        }
        public string GetConnectionString()
        {
            return configsParsed.connectionString;
        }
        public string GetDatabase()
        {
            return configsParsed.database;
        }
        public string GetCollection()
        {
            return configsParsed.collection;
        }
        public int GetRefreshTime()
        {
            return configsParsed.refreshTime;
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
        public string connectionString;
        public string database;
        public string collection;
        public int refreshTime;
    }
}
