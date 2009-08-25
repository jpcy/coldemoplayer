using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.Core
{
    /// <summary>
    /// Represents a desciption for how a certain demo type should be handled by the demo player.
    /// </summary>
    public abstract class DemoHandler
    {
        public abstract string FullName { get; }
        public abstract string Name { get; }
        public abstract string[] Extensions { get; } // e.g. "dem".

        public abstract bool IsValidDemo(Stream stream);

        // TODO: a lot
    }
}
