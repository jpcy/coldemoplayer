using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Specialized;

namespace CDP.IdTech3.Commands
{
    public class SvcConfigString : Command
    {
        public override CommandIds Id
        {
            get { return CommandIds.svc_configstring; }
        }

        public override string Name
        {
            get { return "svc_configstring"; }
        }

        public override bool IsSubCommand
        {
            get { return true; }
        }

        public override bool ContainsSubCommands
        {
            get { return false; }
        }

        public short Index { get; set; }
        public string Value { get; set; }
        public StringDictionary KeyValuePairs { get; set; }
        public bool IsPlayer { get; private set; }

        private readonly string keyValueSeparator = @"\";

        public override void Read(BitReader buffer)
        {
            Index = buffer.ReadShort();
            Value = buffer.ReadString();

            IsPlayer = Value.StartsWith("n" + keyValueSeparator);

            if (Value.StartsWith(keyValueSeparator) || IsPlayer)
            {
                string valueToSplit = (IsPlayer ? Value : Value.Substring(1));
                string[] splitValues = valueToSplit.Split(keyValueSeparator.ToCharArray());
                int length = splitValues.Length;

                // Odd number of split strings probably means there's an extra trailing separator, ignore it.
                if (splitValues.Length % 2 != 0)
                {
                    length--;
                }

                KeyValuePairs = new StringDictionary();

                for (int i = 0; i < length; i += 2)
                {
                    KeyValuePairs.Add(splitValues[i], splitValues[i + 1]);
                }
            }
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Index: {0}", Index);
            log.WriteLine("Value: {0}", Value);

            if (KeyValuePairs != null)
            {
                log.WriteLine("Key/Value pairs:");

                foreach (string key in KeyValuePairs.Keys)
                {
                    log.WriteLine("{0}: {1}", key, KeyValuePairs[key]);
                }
            }
        }
    }
}
