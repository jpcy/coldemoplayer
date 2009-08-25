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

        void Navigate(string viewName);
        void Navigate(string viewName, object parameter);
        void Home();
        void Back();
        void Invoke(Action action);
        void Invoke<T>(Action<T> action, T arg);
        void Invoke<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2);
        void Invoke<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3);
    }

    public class NavigationService : INavigationService
    {
        private static readonly NavigationService instance = new NavigationService();
        private Page home;

        public static NavigationService Instance
        {
            get { return instance; }
        }

        public NavigationWindow Window { get; set; }

        private NavigationService()
        {
        }

        public void Navigate(string viewName)
        {
            Navigate(viewName, null);
        }

        public void Navigate(string viewName, object parameter)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Page view = (Page)assembly.CreateInstance("CDP.View." + viewName);

            if (view == null)
            {
                throw new ArgumentException("Cannot find a View with the name \"" + viewName + "\"");
            }

            ViewModelBase viewModel = (ViewModelBase)assembly.CreateInstance("CDP.ViewModel." + viewName);

            if (viewModel == null)
            {
                throw new ArgumentException("Cannot find a ViewModel with the name \"" + viewName + "\"");
            }

            if (home == null)
            {
                home = view;
            }

            if (parameter == null)
            {
                viewModel.Initialise();
            }
            else
            {
                viewModel.Initialise(parameter);
            }

            view.DataContext = viewModel;
            Window.Navigate(view);
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
    }
}
