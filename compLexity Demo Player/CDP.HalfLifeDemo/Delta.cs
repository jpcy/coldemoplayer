using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.HalfLifeDemo
{
    public class Delta
    {
        private class Entry
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }

        private List<Entry> entries;

        public Delta(int nEntries)
        {
            entries = new List<Entry>(nEntries);
        }

        public void AddEntry(string name)
        {
            entries.Add(new Entry
            {
                Name = name,
                Value = null
            });
        }

        public object FindEntryValue(string name)
        {
            Entry entry = entries.FirstOrDefault(e => e.Name == name);

            if (entry == null)
            {
                return null;
            }

            return entry.Value;
        }

        public void SetEntryValue(string name, object value)
        {
            Entry entry = entries.FirstOrDefault(e => e.Name == name);

            if (entry == null)
            {
                throw new ApplicationException(string.Format("Delta entry {0} not found.", name));
            }

            entry.Value = value;
        }

        public void SetEntryValue(int index, object value)
        {
            entries[index].Value = value;
        }
    }
}
