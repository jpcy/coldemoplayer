using System;
using System.Collections.Generic;
using System.Linq;
using CDP.Core.Extensions;

namespace CDP.HalfLife
{
    /// <summary>
    /// Stores delta structure entry parameters, as well as handling the creation and decoding of delta compressed data.
    /// </summary>
    public class DeltaStructure
    {
        [Flags]
        public enum EntryFlags
        {
            Byte = (1 << 0),
            Short = (1 << 1),
            Float = (1 << 2),
            Integer = (1 << 3),
            Angle = (1 << 4),
            TimeWindow8 = (1 << 5),
            TimeWindowBig = (1 << 6),
            String = (1 << 7),
            Signed = (1 << 31)
        }

        public class Entry
        {
            public string Name { get; set; }
            public uint nBits { get; set; }
            public float Divisor { get; set; }
            public EntryFlags Flags { get; set; }
            public float PreMultiplier { get; set; }
        }

        private string name;
        private List<Entry> entries;

        public string Name
        {
            get { return name; }
        }

        public int NumEntries
        {
            get { return entries.Count; }
        }

        public DeltaStructure(string name)
        {
            this.name = name;
            entries = new List<Entry>();
        }

        /// <summary>
        /// Adds an entry. Delta is assumed to be delta_description_t. Should only need to be called when parsing svc_deltadescription.
        /// </summary>
        /// <param name="delta"></param>
        public void AddEntry(Delta delta)
        {
            string name = (string)delta.FindEntryValue("name");
            uint nBits = (uint)delta.FindEntryValue("nBits");
            float divisor = (float)delta.FindEntryValue("divisor");
            EntryFlags flags = (EntryFlags)((uint)delta.FindEntryValue("flags"));
            //Single preMultiplier = (Single)delta.FindEntryValue("preMultiplier");
            AddEntry(name, nBits, divisor, flags);
        }

        /// <summary>
        /// Adds an entry manually. Should only need to be called when creating a delta_description_t structure.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nBits"></param>
        /// <param name="divisor"></param>
        /// <param name="flags"></param>
        public void AddEntry(string name, uint nBits, float divisor, EntryFlags flags)
        {
            Entry entry = entries.FirstOrDefault(e => e.Name == name);

            if (entry == null)
            {
                entry = new Entry();
                entries.Add(entry);
            }

            entry.Name = name;
            entry.nBits = nBits;
            entry.Divisor = divisor;
            entry.Flags = flags;
            entry.PreMultiplier = 1.0f;
        }

        public Delta CreateDelta()
        {
            Delta delta = new Delta(entries.Count);

            // create delta structure with the same entries as the delta decoder, but no data
            foreach (Entry e in entries)
            {
                delta.AddEntry(e.Name);
            }

            return delta;
        }

        public void SkipDelta(BitReader buffer)
        {
            uint nBitmaskBytes = buffer.ReadUBits(3);
            // TODO: error check nBitmaskBytes against nEntries

            if (nBitmaskBytes == 0)
            {
                return;
            }

            byte[] bitmaskBytes = new byte[nBitmaskBytes];

            for (int i = 0; i < nBitmaskBytes; i++)
            {
                bitmaskBytes[i] = buffer.ReadByte();
            }

            for (int i = 0; i < nBitmaskBytes; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int index = j + i * 8;

                    if (index == entries.Count)
                    {
                        return;
                    }

                    if ((bitmaskBytes[i] & (1 << j)) != 0)
                    {
                        SkipEntry(buffer, entries[index]);
                    }
                }
            }
        }

        public void ReadDelta(BitReader buffer, Delta delta)
        {
            byte[] bitmaskBytes;
            ReadDelta(buffer, delta, out bitmaskBytes);
        }

        public void ReadDelta(BitReader buffer, Delta delta, out byte[] bitmaskBytes)
        {
            // read bitmask
            uint nBitmaskBytes = buffer.ReadUBits(3);
            // TODO: error check nBitmaskBytes against nEntries

            if (nBitmaskBytes == 0)
            {
                bitmaskBytes = null;
                return;
            }

            bitmaskBytes = new byte[nBitmaskBytes];

            for (int i = 0; i < nBitmaskBytes; i++)
            {
                bitmaskBytes[i] = buffer.ReadByte();
            }

            for (int i = 0; i < nBitmaskBytes; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int index = j + i * 8;

                    if (index == entries.Count)
                    {
                        return;
                    }

                    if ((bitmaskBytes[i] & (1 << j)) != 0)
                    {
                        object value = ParseEntry(buffer, entries[index]);

                        if (delta != null)
                        {
                            delta.SetEntryValue(index, value);
                        }
                    }
                }
            }
        }

        public byte[] CreateDeltaBitmask(Delta delta)
        {
            // Find the index of the last entry with a value.
            int? lastEntryWithValueIndex = null;

            for (int i = 0; i < entries.Count; i++)
            {
                if (delta.FindEntryValue(entries[i].Name) != null)
                {
                    lastEntryWithValueIndex = i + 1;
                }
            }

            if (lastEntryWithValueIndex == null)
            {
                return null;
            }

            uint nBitmaskBytes = (uint)(lastEntryWithValueIndex / 8);

            if (lastEntryWithValueIndex % 8 > 0)
            {
                nBitmaskBytes++;
            }

            byte[] bitmaskBytes = new byte[nBitmaskBytes];

            for (int i = 0; i < bitmaskBytes.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int index = j + i * 8;

                    if (index >= entries.Count)
                    {
                        break;
                    }

                    if (delta.FindEntryValue(entries[index].Name) != null)
                    {
                        bitmaskBytes[i] |= (byte)(1 << j);
                    }
                }
            }

            return bitmaskBytes;
        }

        public void WriteDelta(BitWriter buffer, Delta delta)
        {
            WriteDelta(buffer, delta, CreateDeltaBitmask(delta));
        }

        public void WriteDelta(BitWriter buffer, Delta delta, byte[] bitmaskBytes)
        {
            if (bitmaskBytes == null) // no bitmask bytes
            {
                buffer.WriteUBits(0, 3);
                return;
            }

            buffer.WriteUBits((uint)bitmaskBytes.Length, 3);

            for (int i = 0; i < bitmaskBytes.Length; i++)
            {
                buffer.WriteByte(bitmaskBytes[i]);
            }

            for (int i = 0; i < bitmaskBytes.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int index = j + i * 8;

                    if (index == entries.Count)
                    {
                        return;
                    }

                    if ((bitmaskBytes[i] & (1 << j)) != 0)
                    {
                        WriteEntry(delta, buffer, entries[index]);
                    }
                }
            }
        }

        private void SkipEntry(BitReader buffer, Entry e)
        {
            if ((e.Flags & EntryFlags.String) == EntryFlags.String)
            {
                buffer.ReadString();
            }
            else
            {
                buffer.SeekBits((int)e.nBits);
            }
        }

        private object ParseEntry(BitReader buffer, Entry e)
        {
            bool signed = ((e.Flags & EntryFlags.Signed) != 0);

            if ((e.Flags & EntryFlags.Byte) != 0)
            {
                if (signed)
                {
                    return (sbyte)ParseInt(buffer, e);
                }
                else
                {
                    return (byte)ParseUnsignedInt(buffer, e);
                }
            }

            if ((e.Flags & EntryFlags.Short) != 0)
            {
                if (signed)
                {
                    return (short)ParseInt(buffer, e);
                }
                else
                {
                    return (ushort)ParseUnsignedInt(buffer, e);
                }
            }

            if ((e.Flags & EntryFlags.Integer) != 0)
            {
                if (signed)
                {
                    return (int)ParseInt(buffer, e);
                }
                else
                {
                    return (uint)ParseUnsignedInt(buffer, e);
                }
            }

            if ((e.Flags & EntryFlags.Float) != 0 || (e.Flags & EntryFlags.TimeWindow8) != 0 || (e.Flags & EntryFlags.TimeWindowBig) != 0)
            {
                bool negative = false;
                int bitsToRead = (int)e.nBits;

                if (signed)
                {
                    negative = buffer.ReadBoolean();
                    bitsToRead--;
                }

                return buffer.ReadUBits(bitsToRead) / e.Divisor * (negative ? -1.0f : 1.0f);
            }

            if ((e.Flags & EntryFlags.Angle) != 0)
            {
                return buffer.ReadUBits((int)e.nBits) * (360.0f / (float)(1 << (int)e.nBits));
            }

            if ((e.Flags & EntryFlags.String) != 0)
            {
                return buffer.ReadString();
            }

            throw new ApplicationException("Unknown delta entry type {0}.".Args(e.Flags));
        }

        private int ParseInt(BitReader buffer, Entry e)
        {
            bool negative = buffer.ReadBoolean();
            return (int)buffer.ReadUBits((int)e.nBits - 1) / (int)e.Divisor * (negative ? -1 : 1);
        }

        private uint ParseUnsignedInt(BitReader buffer, Entry e)
        {
            return buffer.ReadUBits((int)e.nBits) / (uint)e.Divisor;
        }

        private void WriteEntry(Delta delta, BitWriter buffer, Entry e)
        {
            bool signed = ((e.Flags & EntryFlags.Signed) != 0);
            object value = delta.FindEntryValue(e.Name);

            if ((e.Flags & EntryFlags.Byte) != 0)
            {
                if (signed)
                {
                    sbyte writeValue = (sbyte)value;
                    WriteInt(buffer, e, (int)writeValue);
                }
                else
                {
                    byte writeValue = (byte)value;
                    WriteUnsignedInt(buffer, e, (uint)writeValue);
                }
            }
            else if ((e.Flags & EntryFlags.Short) != 0)
            {
                if (signed)
                {
                    short writeValue = (short)value;
                    WriteInt(buffer, e, (int)writeValue);
                }
                else
                {
                    ushort writeValue = (ushort)value;
                    WriteUnsignedInt(buffer, e, (uint)writeValue);
                }
            }
            else if ((e.Flags & EntryFlags.Integer) != 0)
            {
                if (signed)
                {
                    WriteInt(buffer, e, (int)value);
                }
                else
                {
                    WriteUnsignedInt(buffer, e, (uint)value);
                }
            }
            else if ((e.Flags & EntryFlags.Angle) != 0)
            {
                buffer.WriteUBits((uint)((float)value / (360.0f / (float)(1 << (int)e.nBits))), (int)e.nBits);
            }
            else if ((e.Flags & EntryFlags.String) != 0)
            {
                buffer.WriteString((string)value);
            }
            else if ((e.Flags & EntryFlags.Float) != 0 || (e.Flags & EntryFlags.TimeWindow8) != 0 || (e.Flags & EntryFlags.TimeWindowBig) != 0)
            {
                float writeValue = (float)value;
                int bitsToWrite = (int)e.nBits;

                if (signed)
                {
                    buffer.WriteBoolean(writeValue < 0);
                    bitsToWrite--;
                }

                buffer.WriteUBits((uint)Math.Abs(new decimal(writeValue * e.Divisor)), bitsToWrite);
            }
            else
            {
                throw new ApplicationException("Unknown delta entry type {0}.".Args(e.Flags));
            }
        }

        private void WriteInt(BitWriter buffer, Entry e, int value)
        {
            int writeValue = value * (int)e.Divisor;

            buffer.WriteBoolean(writeValue < 0);
            buffer.WriteUBits((uint)Math.Abs(writeValue), (int)e.nBits - 1);
        }

        private void WriteUnsignedInt(BitWriter buffer, Entry e, uint value)
        {
            uint writeValue = value * (uint)e.Divisor;
            buffer.WriteUBits((uint)Math.Abs(writeValue), (int)e.nBits);
        }
    }
}
