using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;
using CDP.Core;
using System.Threading;

namespace CDP.DemoDiagnostic
{
    public partial class MainWindow : Window
    {
        public class Loggable
        {
            public string Name { get; set; }
            public bool IsSelected { get; set; }
        }

        public class HalfLifeMessage : Loggable
        {
            public bool IsEngineMessage { get; set; }
            public byte EngineMessageId { get; set; }
        }

        public class SourceFrame : Loggable
        {
            public byte Id { get; set; }
        }

        public class SourceMessage : Loggable
        {
            public byte Id { get; set; }
        }

        public class IdTech3Command : Loggable
        {
            public IdTech3.CommandIds Id { get; set; }
        }

        private readonly IDemoManager demoManager = ObjectCreator.Get<IDemoManager>();
        private readonly ObservableCollection<Loggable> loggables = new ObservableCollection<Loggable>();
        private Demo demo;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loggablesListBox.ItemsSource = loggables;
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

                if (s.Length == 1) // A single file.
                {
                    e.Effects = DragDropEffects.Link;
                }
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (s.Length != 1)
            {
                return;
            }

            OpenDemo(s[0]);
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Open Demo"
            };

            if (openFileDialog.ShowDialog(this) != true)
            {
                return;
            }

            OpenDemo(openFileDialog.FileName);
        }

        private void OpenDemo(string fileName)
        {
            demo = demoManager.CreateDemo(fileName);
            loggables.Clear();
            diagnoseProgress.Value = 0;

            if (demo == null)
            {
                MessageBox.Show("Not a valid demo for any of the loaded plugins.");
                fileNameTextBox.Text = string.Empty;
                pluginTextBlock.Text = "-";
                logFileTextBlock.Text = "-";
                return;
            }

            fileNameTextBox.Text = demo.FileName;
            pluginTextBlock.Text = demo.Handler.FullName;
            logFileTextBlock.Text = Path.ChangeExtension(Path.GetFileName(demo.FileName), "log");

            // Half-Life/Counter-Strike messages
            HalfLife.Demo halfLifeDemo = demo as HalfLife.Demo;

            if (halfLifeDemo != null)
            {
                foreach (byte id in Enum.GetValues(typeof(HalfLife.EngineMessageIds)))
                {
                    loggables.Add(new HalfLifeMessage
                    {
                        Name = Enum.GetName(typeof(HalfLife.EngineMessageIds), id).Replace("_", "__"),
                        EngineMessageId = id,
                        IsEngineMessage = true
                    });
                }

                // TODO: user messages
            }

            // Source.
            Source.Demo sourceDemo = demo as Source.Demo;

            if (sourceDemo != null)
            {
                Type frameIdType;
                Type messageIdType;

                // HACK: read the header to get the network protocol.
                using (Core.FastFileStream stream = new Core.FastFileStream(demo.FileName, Core.FastFileAccess.Read))
                {
                    Source.Header header = new Source.Header();
                    header.Read(stream.ReadBytes(Source.Header.SizeInBytes));

                    if (header.NetworkProtocol >= 36)
                    {
                        frameIdType = typeof(Source.FrameIds_Protocol36);
                        messageIdType = typeof(Source.MessageIds_Protocol36);
                    }
                    else
                    {
                        frameIdType = typeof(Source.FrameIds);
                        messageIdType = typeof(Source.MessageIds);
                    }
                }

                // Frames.
                foreach (byte id in Enum.GetValues(frameIdType))
                {
                    loggables.Add(new SourceFrame
                    {
                        Name = "Frame: " + Enum.GetName(frameIdType, id),
                        Id = id
                    });
                }

                // Messages.
                foreach (byte id in Enum.GetValues(messageIdType))
                {
                    loggables.Add(new SourceMessage
                    {
                        Name = "Message: " + Enum.GetName(messageIdType, id).Replace("_", "__"),
                        Id = id
                    });
                }
            }

            // id Tech 3
            IdTech3.Demo idTech3Demo = demo as IdTech3.Demo;

            if (idTech3Demo != null)
            {
                foreach (IdTech3.CommandIds id in Enum.GetValues(typeof(IdTech3.CommandIds)))
                {
                    loggables.Add(new IdTech3Command
                    {
                        Name = Enum.GetName(typeof(IdTech3.CommandIds), id).Replace("_", "__"),
                        Id = id
                    });
                }
            }
        }

        private void selectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Loggable l in loggables)
            {
                l.IsSelected = true;
            }

            loggablesListBox.Items.Refresh();
        }

        private void selectNoneButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Loggable l in loggables)
            {
                l.IsSelected = false;
            }

            loggablesListBox.Items.Refresh();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (demo == null)
            {
                return;
            }

            startButton.IsEnabled = false;
            demo.OperationErrorEvent += demo_OperationErrorEvent;
            demo.ProgressChangedEvent += demo_ProgressChangedEvent;
            demo.OperationCancelledEvent += demo_OperationCancelledEvent;
            demo.OperationCompleteEvent += demo_OperationCompleteEvent;

            string logFileName = Path.ChangeExtension(demo.FileName, "log");
            var selectedLoggables = from l in loggables
                                    where l.IsSelected
                                    select l;

            HalfLife.Demo halfLifeDemo = demo as HalfLife.Demo;

            if (halfLifeDemo != null)
            {
                var engineMessages = from l in selectedLoggables
                                     where ((HalfLifeMessage)l).IsEngineMessage
                                     select ((HalfLifeMessage)l).EngineMessageId;

                ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
                {
                    halfLifeDemo.RunDiagnostic(engineMessages, null, logFileName);
                }));
            }

            Source.Demo sourceDemo = demo as Source.Demo;

            if (sourceDemo != null)
            {
                var frames = from l in selectedLoggables
                             where l is SourceFrame
                             select ((SourceFrame)l).Id;

                var messages = from l in selectedLoggables
                               where l is SourceMessage
                               select ((SourceMessage)l).Id;

                ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
                {
                    sourceDemo.RunDiagnostic(logFileName, frames, messages);
                }));
            }

            IdTech3.Demo idTech3Demo = demo as IdTech3.Demo;

            if (idTech3Demo != null)
            {
                var commands = from c in selectedLoggables
                               select ((IdTech3Command)c).Id;

                ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
                {
                    idTech3Demo.RunDiagnostic(logFileName, commands);
                }));
            }
        }

        void demo_ProgressChangedEvent(object sender, Demo.ProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                diagnoseProgress.Value = e.Progress;
            }));
        }

        void demo_OperationErrorEvent(object sender, Demo.OperationErrorEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                MessageBox.Show(string.Format("Message: {0}\r\nException: {1}", e.ErrorMessage, e.Exception));
                OnDiagnoseComplete(false);
            }));
        }

        void demo_OperationCancelledEvent(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                OnDiagnoseComplete(false);
            }));
        }

        void demo_OperationCompleteEvent(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                OnDiagnoseComplete(true);
            }));
        }

        private void OnDiagnoseComplete(bool success)
        {
            if (success)
            {
                diagnoseProgress.Value = 100;
            }

            startButton.IsEnabled = true;
            demo.OperationErrorEvent -= demo_OperationErrorEvent;
            demo.ProgressChangedEvent -= demo_ProgressChangedEvent;
            demo.OperationCancelledEvent -= demo_OperationCancelledEvent;
            demo.OperationCompleteEvent -= demo_OperationCompleteEvent;
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            if (demo == null)
            {
                return;
            }

            demo.CancelOperation();
        }
    }
}