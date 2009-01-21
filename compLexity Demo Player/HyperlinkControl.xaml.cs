using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics; // Process

namespace compLexity_Demo_Player
{
    public partial class HyperlinkControl : UserControl
    {
        private String text = null;
        private String url = null;

        public String Url
        {
            get
            {
                return url;
            }

            set
            {
                url = value;

                if (text == null)
                {
                    uiTextBlock.Text = url;
                }
            }
        }

        public String Text
        {
            get
            {
                return text;
            }

            set
            {
                text = value;
                uiTextBlock.Text = text;
            }
        }

        public HyperlinkControl()
        {
            InitializeComponent();
        }

        private void uiTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(url);
        }

        private void uiTextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void uiTextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }
    }
}
