using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TorrentClient;

namespace TorrentTray
{
    class Threads
    {
        public void DBWorker()
        {
            DBDriver driver = new DBDriver();
            while (SystemTray.isRunning)
            {
                List<string> dataDB = driver.ReadDataFromDB();

                dataDB.ForEach(delegate (String data)
                {
                    if (data != null)
                    {
                        Threads torrentThread = new Threads();
                        Thread thread = new Thread(() => torrentThread.TorrentWorker(data));
                        thread.Start();
                    }
                });

                /*using (StreamWriter outputFile = new StreamWriter("WriteLines.txt"))
                {
                    dataDB.ForEach(delegate (String data)
                    {
                        if (data != null)
                            outputFile.WriteLine(data);
                    });
                }*/
                Thread.Sleep(driver.GetRefreshTime() * 100); //60000
            }
        }

        public void TorrentWorker(string magnetLink)
        {
            MessageBox.Show(magnetLink);
        }
    }
}
