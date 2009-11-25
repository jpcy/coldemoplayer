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

namespace CDP.Gui.View
{
    public partial class Demo : UserControl
    {
        private Core.DemoHandler handler = null;

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
                handler = null;
                playersGridView.Columns.Clear();
                return;
            }

            if (demo.Handler == handler)
            {
                return;
            }

            handler = demo.Handler;
            playersGridView.Columns.Clear();

            if (handler.PlayerColumns != null)
            {
                foreach (Core.DemoHandler.PlayerColumn column in handler.PlayerColumns)
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
