using System;
using System.Windows.Forms;
using TorrentTray.Properties;

namespace TorrentClient
{
    public class SystemTray : ApplicationContext
    {
        private NotifyIcon trayIcon;

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
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;

            Application.Exit();
        }
    }
}