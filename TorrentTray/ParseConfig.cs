using Newtonsoft.Json;
using NLog;
using System;
using System.IO;

namespace TorrentTray
{
    class ParseConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static bool JSON_WAS_READ = false;

        private static dynamic configsParsed;

        public ParseConfig()
        {
            if(!JSON_WAS_READ)
                LoadJson();
        }

        public string GetConnectionString() { return configsParsed.connectionString; }
        public string GetDatabase() { return configsParsed.database; }
        public string GetCollection() { return configsParsed.collection; }
        public int GetPoolingTime() { return configsParsed.poolingTime; }
        public string GetBittorrentPath() { return configsParsed.bittorrent_path; }
        public string GetBittorrentExec() { return configsParsed.bittorrent_exec; }
        public string GetBittorrentAddr() { return configsParsed.bittorrent_addr; }
        public string GetBittorrentAddURI() { return configsParsed.bittorrent_add_uri; }
        public string GetBittorrentCheckURI() { return configsParsed.bittorrent_check_uri; }
        public int GetVerifyTorrentTime() { return configsParsed.verifyTorrentTime; }

        private void LoadJson()
        {
            try
            {
                using (StreamReader r = new StreamReader("Config.json"))
                {
                    string json = r.ReadToEnd();
                    configsParsed = JsonConvert.DeserializeObject(json);
                    logger.Info("JSON File parsed successfully.");
                }
                JSON_WAS_READ = true;
            }
            catch (Exception ex)
            {
                JSON_WAS_READ = false;
                logger.Error($"Error parsing JSON File. Error: {ex.ToString()}.");
            }
        }
    }
}
