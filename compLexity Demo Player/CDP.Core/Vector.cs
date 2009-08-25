using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
