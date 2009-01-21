using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace compLexity_Demo_Player
{
    public class SystemTrayIcon
    {
        /// <summary>
        /// Sets the tray icon tooltip text. Should be set only after ShowIcon is called.
        /// </summary>
        public static String Text
        {
            set
            {
                Debug.Assert(notifyIcon != null);
                notifyIcon.Text = value;
            }
        }

        public static Boolean HasContextMenuStrip
        {
            get
            {
                return notifyIcon.ContextMenuStrip != null;
            }
        }

        public static Boolean IsVisible
        {
            get
            {
                if (notifyIcon == null)
                {
                    return false;
                }

                return notifyIcon.Visible;
            }
        }

        private static Icon icon;
        private static NotifyIcon notifyIcon;

        public static void Show(EventHandler iconClick)
        {
            notifyIcon = new NotifyIcon();
            
            if (iconClick != null)
            {
                notifyIcon.Click += iconClick;
            }

            if (icon == null)
            {
                icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.GetCommandLineArgs()[0]);
            }

            notifyIcon.Icon = icon;
            notifyIcon.Visible = true;
        }

        public static void Hide()
        {
            Debug.Assert(notifyIcon != null);
            notifyIcon.Visible = false;
            SetContextMenuStrip(null);
            notifyIcon.Dispose();
        }

        public static void SetContextMenuStrip(ContextMenuStrip menu)
        {
            if (menu != null)
            {
                notifyIcon.ContextMenuStrip = menu;
            }
            else
            {
                if (notifyIcon.ContextMenuStrip != null)
                {
                    // FIXME: necessary? disposing of the contextmenustrip probably does this
                    /*foreach (ToolStripItem item in notifyIcon.ContextMenuStrip.Items)
                    {
                        item.Dispose();
                    }*/

                    notifyIcon.ContextMenuStrip.Dispose();
                    notifyIcon.ContextMenuStrip = null; // FIXME: necessary?
                }
            }
        }
    }
}
