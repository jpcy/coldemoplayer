using System;
using System.Collections.Generic;
using System.Linq;
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
using System.ComponentModel;

namespace CDP.Gui.PartialViews
{
    public partial class Demo : UserControl
    {
        private Core.Plugin plugin = null;

        public Demo()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            // Don't run this code in the designer.
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Core.ObjectCreator.Get<IMediator>().Register<Core.Demo>(Messages.SelectedDemoChanged, SelectedDemoChanged, this);
            }
        }

        private void SelectedDemoChanged(Core.Demo demo)
        {
            if (demo == null)
            {
                plugin = null;
                playersGridView.Columns.Clear();
                return;
            }

            if (demo.Plugin == plugin)
            {
                return;
            }

            plugin = demo.Plugin;
            playersGridView.Columns.Clear();

            if (plugin.PlayerColumns != null)
            {
                foreach (Core.Plugin.PlayerColumn column in plugin.PlayerColumns)
                {
                    playersGridView.Columns.Add(new GridViewColumn
                    {
                        Header = column.Header,
                        DisplayMemberBinding = new Binding(column.DisplayMemberBinding)
                    });
                }
            }
        }
    }
}
