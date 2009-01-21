using System;
using System.Collections.Generic;
using System.Text;

namespace Update
{
    public class Program
    {
        [STAThread]
        static void Main(String[] args)
        {
            // TODO: only allow one instance to run

            App app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }

    // Func
    public delegate TResult Function<T, TResult>(T arg);
    public delegate TResult Function<T1, T2, TResult>(T1 arg1, T2 arg2);
    public delegate TResult Function<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult Function<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    // Action
    public delegate void Procedure();
    public delegate void Procedure<T1>(T1 arg);
    public delegate void Procedure<T1, T2>(T1 arg1, T2 arg2);
    public delegate void Procedure<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Procedure<T1, T2, T3, T4>(T1 arg1, T2 arg2, T4 arg4);
}
