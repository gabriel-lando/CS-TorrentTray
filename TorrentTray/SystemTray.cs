using NLog;
using System;
using System.Threading;
using System.Windows.Forms;
using TorrentTray.Properties;

namespace TorrentTray
{
    public class SystemTray : ApplicationContext
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private NotifyIcon trayIcon;

        static public bool isRunning = true;

        public SystemTray()
        {
            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = Resources.AppIcon,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };

            logger.Debug("Starting DB thread.");

            Threads databaseThread = new Threads();
            Thread thread = new Thread(new ThreadStart(databaseThread.DBWorker));
            thread.Start();
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;
            isRunning = false;

            logger.Info("Shutting down TorrentTray.");
            Application.Exit();
        }
    }
}