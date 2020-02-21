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
        //private static Config configsParsed;
        private static bool WAS_READ = false;

        private static dynamic configsParsed;

        public ParseConfig()
        {
            if(!WAS_READ)
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
        public int GetPoolingTime()
        {
            return configsParsed.poolingTime;
        }
        public string GetBittorrentPath()
        {
            return configsParsed.bittorrent_path;
        }
        public string GetBittorrentExec()
        {
            return configsParsed.bittorrent_exec;
        }
        public string GetBittorrentArgs()
        {
            return configsParsed.bittorrent_args;
        }
        public string GetBittorrentAddr()
        {
            return configsParsed.bittorrent_addr;
        }
        public string GetBittorrentAddURI()
        {
            return configsParsed.bittorrent_add_uri;
        }
        public string GetBittorrentCheckURI()
        {
            return configsParsed.bittorrent_check_uri;
        }
        public int GetVerifyTorrentTime()
        {
            return configsParsed.verifyTorrentTime;
        }

        private void LoadJson()
        {
            using (StreamReader r = new StreamReader("Config.json"))
            {
                string json = r.ReadToEnd();
                //configsParsed = JsonConvert.DeserializeObject<Config>(json);
                configsParsed = JsonConvert.DeserializeObject(json);
                //configsJSON = JsonConvert.DeserializeObject(json);
            }
            WAS_READ = true;
        }
    }

    class Config
    {
        public string connectionString;
        public string database;
        public string collection;
        public int poolingTime;
        public string bittorrent_path;
        public string bittorrent_exec;
        public string bittorrent_args;
        public string bittorrent_addr;
        public string bittorrent_add_uri;
        public string bittorrent_check_uri;
        public int verifyTorrentTime;
    }
}
