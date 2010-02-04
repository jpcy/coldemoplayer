using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CDP.Core.Extensions;

namespace CDP.Gui
{
    internal interface INavigationService
    {
        Window Window { get; set; }
        string CurrentPageTitle { get; }
        bool IsModalWindowActive { get; }

        void Navigate(Page view, Core.ViewModelBase viewModel);
        void Home();
        void Back();
        void ShowWindow();
        void HideWindow();
        string BrowseForFile(string fileName, string initialPath);
        void OpenModalWindow(Page view, Core.ViewModelBase viewModel);
        void CloseModalWindow(Core.ViewModelBase viewModel);
        void ForceCommandRequery();
        void Invoke(Action action);
        void Invoke<T>(Action<T> action, T arg);
        void Invoke<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2);
        void Invoke<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3);
        void BeginInvoke<T>(Action<T> action, T arg, bool force);
        void BeginInvoke<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2, bool force);
    }

    [Core.Singleton]
    internal class NavigationService : INavigationService
    {
        public Window Window { get; set; }

        public string CurrentPageTitle
        {
            get { return ((Page)Window.Content).Title; }
        }

        public bool IsModalWindowActive
        {
            get { return modalWindows.Count > 0; }
        }

        private readonly Stack<Page> pages = new Stack<Page>();
        private readonly List<Window> modalWindows = new List<Window>();

        public void Navigate(Page view, Core.ViewModelBase viewModel)
        {
            if (IsModalWindowActive)
            {
                throw new InvalidOperationException("Cannot navigate within a modal window.");
            }

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
            if (IsModalWindowActive)
            {
                throw new InvalidOperationException("Cannot navigate within a modal window.");
            }

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
            if (IsModalWindowActive)
            {
                throw new InvalidOperationException("Cannot navigate within a modal window.");
            }

            if (pages.Count > 1)
            {
                pages.Pop();
                Window.Content = pages.Peek();
            }
        }

        public void ShowWindow()
        {
            if (IsModalWindowActive)
            {
                throw new InvalidOperationException("Cannot show the main window when a modal window is active.");
            }

            Window.Show();
        }

        public void HideWindow()
        {
            if (IsModalWindowActive)
            {
                throw new InvalidOperationException("Cannot hide the main window when a modal window is active.");
            }

            Window.Hide();
        }

        public string BrowseForFile(string fileName, string initialPath)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = Strings.BrowseForFileTitle.Args(fileName),
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

        public void OpenModalWindow(Page view, Core.ViewModelBase viewModel)
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
            Window window = new ModalWindow();
            window.Owner = Window;
            window.Content = view;
            modalWindows.Add(window);
            window.ShowDialog();
        }

        public void CloseModalWindow(Core.ViewModelBase viewModel)
        {
            Window window = modalWindows.FirstOrDefault(w => ((Page)w.Content).DataContext == viewModel);

            if (window != null)
            {
                modalWindows.Remove(window);
                window.Close();
            }
        }

        public void ForceCommandRequery()
        {
            CommandManager.InvalidateRequerySuggested();
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
