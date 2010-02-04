using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CDP.Core.Extensions;

namespace CDP.HalfLife
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
                throw new ApplicationException("Delta entry {0} not found.".Args(name));
            }

            entry.Value = value;
        }

        public void SetEntryValue(int index, object value)
        {
            entries[index].Value = value;
        }
    }
}
