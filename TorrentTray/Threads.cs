using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace TorrentTray
{
    class Threads
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static bool IS_BITTORRENT_RUNNING = false;

        private static readonly int ONE_HOUR_IN_MS = 1000; //60000

        public void DBWorker()
        {
            ParseConfig config = new ParseConfig();
            DBDriver driver = new DBDriver();

            List<string> hashesDownloading = driver.ReadDownloadingHashsFromDB();

            if (hashesDownloading.Count > 0)
            {
                if (!IS_BITTORRENT_RUNNING)
                    StartTorrentApp(config.GetBittorrentPath(), config.GetBittorrentExec());

                hashesDownloading.ForEach(delegate (String hash)
                {
                    if (hash != null)
                    {
                        logger.Debug($"Downloading: {hash}. Tracking.");
                        Threads torrentThread = new Threads();
                        Thread thread = new Thread(() => torrentThread.VerifyTorrentWorker(hash));
                        thread.Start();
                    }
                });
            }

            while (SystemTray.isRunning)
            {
                List<string> hashesWaiting = driver.ReadNewHashsFromDB();

                if(hashesWaiting.Count > 0)
                {
                    if (!IS_BITTORRENT_RUNNING)
                        StartTorrentApp(config.GetBittorrentPath(), config.GetBittorrentExec());

                    hashesWaiting.ForEach(delegate (String hash)
                    {
                        if (hash != null)
                        {
                            logger.Debug($"New hash: {hash}. Adding.");
                            Threads torrentThread = new Threads();
                            Thread thread = new Thread(() => torrentThread.AddTorrentWorker(hash));
                            thread.Start();
                        }
                    });
                }

                Thread.Sleep(config.GetPoolingTime() * ONE_HOUR_IN_MS);
            }
        }

        private void AddTorrentWorker(string hash)
        {
            if (!AddHashAndStartDownload(hash))
                return;

            Threads torrentThread = new Threads();
            Thread thread = new Thread(() => torrentThread.VerifyTorrentWorker(hash));
            thread.Start();
        }

        private void VerifyTorrentWorker(string hash)
        {
            ParseConfig config = new ParseConfig();

            do {
                Thread.Sleep(config.GetVerifyTorrentTime() * ONE_HOUR_IN_MS);
            } while (SystemTray.isRunning && VerifyTorrentStatus(hash));
        }

        private bool AddHashAndStartDownload(string hash)
        {
            try
            {
                string magnet = "magnet:?xt=urn:btih:" + hash;

                ParseConfig config = new ParseConfig();

                NameValueCollection outgoingQueryString = HttpUtility.ParseQueryString(String.Empty);
                outgoingQueryString.Add("urls", magnet);
                string postData = outgoingQueryString.ToString();

                byte[] postBytes = Encoding.ASCII.GetBytes(postData);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(config.GetBittorrentAddr() + config.GetBittorrentAddURI());

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postBytes.Length;

                Stream postStream = request.GetRequestStream();
                postStream.Write(postBytes, 0, postBytes.Length);
                postStream.Flush();
                postStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                if (responseString == "Ok.")
                {
                    logger.Info($"Adding hash: {hash}.");
                    DBDriver driver = new DBDriver();
                    driver.UpdateStatusFromDB(hash, DBDriver.DOWNLOADING_STATUS);
                    return true;
                }

                logger.Error($"Error adding hash {hash}. Error: {responseString}.");
                return false;
            }
            catch (Exception ex)
            {
                logger.Error($"Error adding hash {hash}. Error acessing API: {ex.ToString()}.");
                return false;
            }
        }

        private bool VerifyTorrentStatus(string hash) //Return True if is downloading; False if error or finished
        {
            try
            {
                ParseConfig config = new ParseConfig();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(config.GetBittorrentAddr() + config.GetBittorrentCheckURI() + hash.ToLower());
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                dynamic responseParsed = JsonConvert.DeserializeObject(responseString);

                bool downloading = false;
                foreach (var item in responseParsed)
                {
                    if (item.is_seed == false)
                    {
                        logger.Debug($"Hash {hash} is still downloading.");
                        downloading = true;
                    }
                }

                if (downloading == false)
                {
                    logger.Info($"Hash {hash} finished :)");
                    DBDriver driver = new DBDriver();
                    driver.UpdateStatusFromDB(hash, DBDriver.FINISHED_STATUS);
                }

                return downloading;
            }
            catch (WebException we)
            {
                int statusCode = (int)((HttpWebResponse)we.Response).StatusCode;

                if (statusCode == 404)
                {
                    logger.Info($"Hash {hash} removed from qBittorrent. Removing from DB.");
                    DBDriver driver = new DBDriver();
                    driver.RemoveHashFromDB(hash);
                    return false;
                }
                else
                {
                    logger.Error($"Error verifying hash {hash}. Error acessing API: {we.ToString()}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error verifying hash {hash}. Error acessing API: {ex.ToString()}.");
                return false;
            }
        }

        private bool StartTorrentApp(string path, string executable)
        {
            if (IS_BITTORRENT_RUNNING)
                return true;

            Process[] pname = Process.GetProcessesByName(executable.Replace(".exe", ""));
            if (pname.Length > 0)
            {
                logger.Info("BitTorrent is already running.");
                IS_BITTORRENT_RUNNING = true;
                return true;
            }

            try
            {
                Process.Start(path + executable);
                logger.Info("BitTorrent is starting.");
                IS_BITTORRENT_RUNNING = true;

                Thread.Sleep(10 * 1000); // Sleep X seconds
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"Error starting BitTorrent. Error: {ex.ToString()}.");
                return false;
            }
        }
    }
}
