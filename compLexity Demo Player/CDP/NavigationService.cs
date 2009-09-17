using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Navigation;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CDP
{
    public interface INavigationService
    {
        NavigationWindow Window { get; set; }
        string CurrentPageTitle { get; }

        void Navigate(Page view, Core.ViewModelBase viewModel);
        void Home();
        void Back();
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
        public NavigationWindow Window { get; set; }

        public string CurrentPageTitle
        {
            get { return ((Page)Window.NavigationService.Content).Title; }
        }

        private Page home;

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

            if (home == null)
            {
                home = view;
            }

            view.DataContext = viewModel;
            Window.Navigate(view);
            viewModel.OnNavigateComplete();
        }

        public void Home()
        {
            if (home == null)
            {
                throw new InvalidOperationException("Cannot go to the home page when no pages have been navigated to yet.");
            }

            Window.NavigationService.Navigate(home);

            // clear the history
            while (Window.NavigationService.RemoveBackEntry() != null)
            {
            }
        }

        public void Back()
        {
            Window.NavigationService.GoBack();
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
