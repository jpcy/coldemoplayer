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
        public class Message
        {
            public string Name { get; set; }
            public bool IsSelected { get; set; }
        }

        public class HalfLifeMessage : Message
        {
            public bool IsEngineMessage { get; set; }
            public byte EngineMessageId { get; set; }
        }

        private readonly IDemoManager demoManager = ObjectCreator.Get<IDemoManager>();
        private readonly ObservableCollection<Message> messages = new ObservableCollection<Message>();
        private Demo demo;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            messagesListBox.ItemsSource = messages;
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
            messages.Clear();
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
                    messages.Add(new HalfLifeMessage
                    {
                        Name = Enum.GetName(typeof(HalfLife.EngineMessageIds), id).Replace("_", "__"),
                        EngineMessageId = id,
                        IsEngineMessage = true
                    });
                }

                // TODO: user messages
            }
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

            HalfLife.Demo halfLifeDemo = demo as HalfLife.Demo;

            if (halfLifeDemo != null)
            {
                var engineMessages = from m in messages
                                     where m.IsSelected && ((HalfLifeMessage)m).IsEngineMessage
                                     select ((HalfLifeMessage)m).EngineMessageId;

                ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
                {
                    halfLifeDemo.RunDiagnostic(engineMessages, null, Path.ChangeExtension(demo.FileName, "log"));
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
                OnDiagnoseComplete();
            }));
        }

        void demo_OperationCancelledEvent(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                OnDiagnoseComplete();
            }));
        }

        void demo_OperationCompleteEvent(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                OnDiagnoseComplete();
            }));
        }

        private void OnDiagnoseComplete()
        {
            startButton.IsEnabled = true;
            diagnoseProgress.Value = 100;
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
