using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.Core
{
    public class StringArgumentNullOrEmpty : ArgumentException
    {
        public StringArgumentNullOrEmpty(string paramName)
            : base("String cannot be null or empty.", paramName)
        {
        }
    }
}
