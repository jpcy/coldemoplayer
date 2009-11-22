using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows;

namespace QuickZip.Framework
{
    public class PresenterBase<T> : Notifier
    {
        protected readonly T _view;
        protected readonly Dispatcher _dispatcher;

        public PresenterBase(T view)
        {
            _view = view;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public T View
        {
            get { return _view; }
        }

        public virtual void RegisterCommands()
        {

        }
    }
}
