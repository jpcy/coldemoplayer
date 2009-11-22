using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.Source
{
    public class DataTable
    {
        public bool Unknown1 { get; set; }
        public string Name { get; set; }
        public List<Prop> PropDefinitions { get; private set; }

        public DataTable()
        {
            PropDefinitions = new List<Prop>();
        }
    }
}
