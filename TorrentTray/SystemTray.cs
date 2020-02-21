using System;
using System.Threading;
using System.Windows.Forms;
using TorrentTray;
using TorrentTray.Properties;

namespace TorrentClient
{
    public class SystemTray : ApplicationContext
    {
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

            Threads databaseThread = new Threads();
            Thread thread = new Thread(new ThreadStart(databaseThread.DBWorker));
            thread.Start();
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;
            isRunning = false;

            Application.Exit();
        }
    }
}