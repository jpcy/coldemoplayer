using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CDP.Core.Extensions;

namespace CDP.Core
{
    public class Vector
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector()
        {
        }

        public Vector(float[] v)
        {
            if (v.Length != 3)
            {
                throw new ArgumentException("Array length must be 3.", "v");
            }

            X = v[0];
            Y = v[1];
            Z = v[2];
        }

        public float[] ToArray()
        {
            return new float[] { X, Y, Z };
        }

        public override string ToString()
        {
            return "{0} {1} {2}".Args(X, Y, Z);
        }
    }
}
