using NLog;
using System;
using System.Windows.Forms;

namespace TorrentTray
{
    static class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [STAThread]
        static void Main()
        {
            logger.Info("Starting TorrentTray.");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SystemTray());
        }
    }
}
