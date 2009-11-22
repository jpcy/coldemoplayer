using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.Source
{
    public class Prop
    {
        public enum Types : uint
        {
            DPT_Int = 0,
            DPT_Float,
            DPT_Vector,
            DPT_String,
            DPT_Array,	// An array of the base types (can't be of datatables).
            DPT_DataTable
        }

        [Flags]
        public enum FlagBits : uint
        {
            Unsigned = (1<<0),
            Coord = (1<<1),
            NoScale = (1<<2),
            RoundDown = (1<<3),
            RoundUp = (1<<4),
            Normal = (1<<5),
            Exclude = (1<<6),
            XYZE = (1<<7),
            InsideArray = (1<<8),
            ProxyAlwaysYes = (1<<9),
            ChangesOften = (1<<10),
            IsAVectorElement = (1<<11),
            Collapsible = (1<<12)
        }

        public string Name { get; set; }
        public Types Type { get; set; }
        public FlagBits Flags { get; set; }

        // Type specific.
        public string ExcludeProp { get; set; } // FlagBits.Exclude
        public uint NumElements { get; set; } // DPT_Array
        public uint nBits { get; set; }
        public float Low { get; set; }
        public float High { get; set; }
        public string DataTableName { get; set; } // DPT_DataTable

        // TODO: count and stride for DPT_Array
    }
}
