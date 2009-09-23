using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;

namespace CDP
{
    public interface INavigationService
    {
        Window Window { get; set; }
        string CurrentPageTitle { get; }

        void Navigate(Page view, Core.ViewModelBase viewModel);
        void Home();
        void Back();
        void ShowWindow();
        void HideWindow();
        string BrowseForFile(string fileName, string initialPath);
        void Invoke(Action action);
        void Invoke<T>(Action<T> action, T arg);
        void Invoke<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2);
        void Invoke<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3);
        void BeginInvoke<T>(Action<T> action, T arg, bool force);
        void BeginInvoke<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2, bool force);
    }

    [Core.Singleton]
    public class NavigationService : INavigationService
    {
        public Window Window { get; set; }

        public string CurrentPageTitle
        {
            get { return ((Page)Window.Content).Title; }
        }

        private readonly Stack<Page> pages = new Stack<Page>();

        public void Navigate(Page view, Core.ViewModelBase viewModel)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel");
            }

            view.DataContext = viewModel;
            pages.Push(view);
            Window.Content = view;
            viewModel.OnNavigateComplete();
        }

        public void Home()
        {
            if (pages.Count == 0)
            {
                throw new InvalidOperationException("Cannot go to the home page when no pages have been navigated to yet.");
            }

            while (pages.Count > 1)
            {
                pages.Pop();
            }

            Window.Content = pages.Peek();
        }

        public void Back()
        {
            if (pages.Count > 1)
            {
                pages.Pop();
                Window.Content = pages.Peek();
            }
        }

        public void ShowWindow()
        {
            Window.Show();
        }

        public void HideWindow()
        {
            Window.Hide();
        }

        public string BrowseForFile(string fileName, string initialPath)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Browse for \"" + fileName + "\"...",
                InitialDirectory = initialPath,
                Filter = fileName + "|" + fileName,
                RestoreDirectory = true
            };

            if (dialog.ShowDialog(Window) == true)
            {
                return dialog.FileName;
            }

            return null;
        }

        public void Invoke(Action action)
        {
            if (Dispatcher.CurrentDispatcher == Window.Dispatcher)
            {
                action();
            }
            else
            {
                Window.Dispatcher.Invoke(action);
            }
        }

        public void Invoke<T>(Action<T> action, T arg)
        {
            if (Dispatcher.CurrentDispatcher == Window.Dispatcher)
            {
                action(arg);
            }
            else
            {
                Window.Dispatcher.Invoke(action, arg);
            }
        }

        public void Invoke<T1,T2>(Action<T1,T2> action, T1 arg1, T2 arg2)
        {
            if (Dispatcher.CurrentDispatcher == Window.Dispatcher)
            {
                action(arg1, arg2);
            }
            else
            {
                Window.Dispatcher.Invoke(action, arg1, arg2);
            }
        }

        public void Invoke<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            if (Dispatcher.CurrentDispatcher == Window.Dispatcher)
            {
                action(arg1, arg2, arg3);
            }
            else
            {
                Window.Dispatcher.Invoke(action, arg1, arg2, arg3);
            }
        }

        public void BeginInvoke<T>(Action<T> action, T arg, bool force)
        {
            if (!force && Dispatcher.CurrentDispatcher == Window.Dispatcher)
            {
                action(arg);
            }
            else
            {
                Window.Dispatcher.BeginInvoke(action, arg);
            }
        }

        public void BeginInvoke<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2, bool force)
        {
            if (!force && Dispatcher.CurrentDispatcher == Window.Dispatcher)
            {
                action(arg1, arg2);
            }
            else
            {
                Window.Dispatcher.BeginInvoke(action, arg1, arg2);
            }
        }
    }
}
