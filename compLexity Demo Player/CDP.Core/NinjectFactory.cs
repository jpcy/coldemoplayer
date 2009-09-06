using System;
using Ninject.Core;

namespace CDP.Core
{
    public static class NinjectFactory
    {
        private static IKernel kernel;

        public static IKernel Kernel
        {
            get { return kernel; }
        }

        static NinjectFactory()
        {
            kernel = new StandardKernel();
        }

        public static T Get<T>()
        {
            return kernel.Get<T>();
        }

        public static void LoadModule(IModule module)
        {
            kernel.Load(module);
        }
    }
}
