using System;
using Ninject.Core;

namespace CDP.Core
{
    public class NinjectModule : StandardModule
    {
        public override void Load()
        {
            Bind<DemoManager>().ToSelf();
            Bind<IFileSystem>().To<FileSystem>();
        }
    }
}
