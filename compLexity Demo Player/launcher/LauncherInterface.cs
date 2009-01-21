using System;
using System.Collections.Generic;
using System.Text;

namespace compLexity_Demo_Player
{
    public interface ILauncher
    {
        void LaunchedProcessFound();
        void LaunchedProcessClosed();
    }
}
