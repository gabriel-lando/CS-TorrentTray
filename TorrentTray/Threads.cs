using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using TorrentClient;

namespace TorrentTray
{
    class Threads
    {
        private static bool IS_BITTORRENT_RUNNING = false;
        public void DBWorker()
        {
            ParseConfig config = new ParseConfig();
            DBDriver driver = new DBDriver();
            
            while (SystemTray.isRunning)
            {
                List<string> hashsDB = driver.ReadHashsFromDB();

                if(hashsDB.Count > 0)
                {
                    if (!IS_BITTORRENT_RUNNING)
                        StartTorrentApp(config.GetBittorrentPath(), config.GetBittorrentExec(), config.GetBittorrentArgs());

                    hashsDB.ForEach(delegate (String hash)
                    {
                        if (hash != null)
                        {
                            Threads torrentThread = new Threads();
                            Thread thread = new Thread(() => torrentThread.TorrentWorker(hash));
                            thread.Start();
                        }
                    });
                }

                Thread.Sleep(config.GetPoolingTime() * 100); //60000
            }
        }

        private void TorrentWorker(string hash)
        {
            if (!AddHashAndStartDownload(hash))
                return;

            ParseConfig config = new ParseConfig();

            do {
                Thread.Sleep(config.GetVerifyTorrentTime() * 1000); //60000
            } while (SystemTray.isRunning && VerifyTorrentStatus(hash));
        }

        private bool AddHashAndStartDownload(string hash)
        {
            string magnet = "magnet:?xt=urn:btih:" + hash;

            ParseConfig config = new ParseConfig();

            NameValueCollection outgoingQueryString = HttpUtility.ParseQueryString(String.Empty);
            outgoingQueryString.Add("urls", magnet);
            string postData = outgoingQueryString.ToString();

            byte[] postBytes = Encoding.ASCII.GetBytes(postData);

            try
            {
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
                    DBDriver driver = new DBDriver();
                    driver.UpdateStatusFromDB(hash, DBDriver.DOWNLOADING_STATUS);
                    return true;
                }

                MessageBox.Show("ERROR Adding torrent: " + responseString);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR Acessing API: " + ex.ToString());
                return false;
            }
        }

        private bool VerifyTorrentStatus(string hash) //Return True if is downloading; False if error or finished
        {
            ParseConfig config = new ParseConfig();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(config.GetBittorrentAddr() + config.GetBittorrentCheckURI() + hash);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                dynamic responseParsed = JsonConvert.DeserializeObject(responseString);

                MessageBox.Show("Verify API: " + responseString);

                if (responseParsed[0].status)
                {
                    DBDriver driver = new DBDriver();
                    driver.UpdateStatusFromDB(hash, DBDriver.FINISHED_STATUS);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR Acessing Check API: " + ex.ToString());
            }
            return false;
        }

        private bool StartTorrentApp(string path, string executable, string arguments)
        {
            if (IS_BITTORRENT_RUNNING)
                return true;

            Process[] pname = Process.GetProcessesByName(executable);
            if (pname.Length > 0)
            {
                IS_BITTORRENT_RUNNING = true;
                return true;
            }

            try
            {
                Process.Start(path + executable, arguments);
            }
            catch
            {
                MessageBox.Show("Error starting " + executable);
            }

            IS_BITTORRENT_RUNNING = true;

            Thread.Sleep(10 * 1000);
            return true;
        }
    }
}
