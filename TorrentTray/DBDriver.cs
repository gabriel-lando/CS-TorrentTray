using MongoDB.Driver;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TorrentTray
{
    
    class DBDriver
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static public readonly int ERROR_STATUS = -1;
        static public readonly int WAITING_STATUS = 0;
        static public readonly int DOWNLOADING_STATUS = 1;
        static public readonly int FINISHED_STATUS = 2;
        static public readonly int REMOVED_STATUS = 3;

        private static ParseConfig config;
        private static bool IS_CONENCTED = false;
        private static MongoClient client;
        private static IMongoDatabase database;
        public DBDriver()
        {
            ConnectToDB();
        }

        public void ConnectToDB()
        {
            if (!IS_CONENCTED)
            {
                try
                {
                    config = new ParseConfig();
                    client = new MongoClient(config.GetConnectionString());
                    database = client.GetDatabase(config.GetDatabase());
                    IS_CONENCTED = true;
                    logger.Info("Connected to MongoDB.");
                }
                catch (Exception ex)
                {
                    logger.Error($"Error connecting to MongoDB. ERROR: {ex.ToString()}");
                }
            }
        }

        public List<string> ReadNewHashsFromDB()
        {
            try
            {
                IMongoCollection<Magnet> collection = database.GetCollection<Magnet>(config.GetCollection());
                logger.Debug("Getting new hashes from MongoDB.");
                return (from x in collection.AsQueryable<Magnet>() where (x.status == WAITING_STATUS) select x.hash).ToList();
            }
            catch (Exception ex)
            {
                logger.Error($"Error reading new hashes from MongoDB. ERROR: {ex.ToString()}");
                return null;
            }
        }

        public List<string> ReadDownloadingHashsFromDB()
        {
            try
            {
                IMongoCollection<Magnet> collection = database.GetCollection<Magnet>(config.GetCollection());
                logger.Debug("Getting downloading hashes from MongoDB.");
                return (from x in collection.AsQueryable<Magnet>() where (x.status == DOWNLOADING_STATUS) select x.hash).ToList();
            }
            catch (Exception ex)
            {
                logger.Error($"Error reading downloading hashes from MongoDB. ERROR: {ex.ToString()}");
                return null;
            }
        }

        public List<string> ReadFinishedHashsFromDB()
        {
            try
            {
                IMongoCollection<Magnet> collection = database.GetCollection<Magnet>(config.GetCollection());
                logger.Debug("Getting finished hashes from MongoDB.");
                return (from x in collection.AsQueryable<Magnet>() where (x.status == FINISHED_STATUS) select x.hash).ToList();
            }
            catch (Exception ex)
            {
                logger.Error($"Error reading finished hashes from MongoDB. ERROR: {ex.ToString()}");
                return null;
            }
        }

        public bool UpdateStatusFromDB(string hash, int status)
        {
            try
            {
                var filter = Builders<Magnet>.Filter.Eq("hash", hash);
                var update = Builders<Magnet>.Update.Set("status", status);

                UpdateResult result = database.GetCollection<Magnet>(config.GetCollection()).UpdateOne(filter, update);

                logger.Debug("Updating hashes status in MongoDB.");

                return result.IsAcknowledged;
            }
            catch (Exception ex)
            {
                logger.Error($"Error updating hashes status in MongoDB. ERROR: {ex.ToString()}");
                return false;
            } 
        }

        public bool RemoveHashFromDB(string hash)
        {
            try
            {
                var deleteFilter = Builders<Magnet>.Filter.Eq("hash", hash);

                DeleteResult result = database.GetCollection<Magnet>(config.GetCollection()).DeleteOne(deleteFilter);

                logger.Debug($"Removing hash {hash} from MongoDB.");

                return result.IsAcknowledged;
            }
            catch (Exception ex)
            {
                logger.Error($"Error removing hash {hash} from MongoDB. ERROR: {ex.ToString()}");
                return false;
            }
        }
    }

    class Magnet
    {
        public int _id;
        public string hash;
        public int status;
        public string title;
    }
}
