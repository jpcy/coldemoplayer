using System;
using System.Collections.Generic;
using System.Linq;

namespace CDP.HalfLife.Analysis
{
    public class Player
    {
        public class InfoKey
        {
            public class Value
            {
                public string InnerValue { get; private set; }
                public float Timestamp { get; private set; }

                public Value(string value, float timestamp)
                {
                    InnerValue = value;
                    Timestamp = timestamp;
                }
            }

            public string Key { get; private set; }
            public List<Value> Values { get; private set; }
            public string NewestValueInnerValue
            {
                get
                {
                    Value last = Values.LastOrDefault();

                    if (last == null)
                    {
                        return null;
                    }

                    return last.InnerValue;
                }
            }

            public InfoKey(string key)
            {
                Key = key;
                Values = new List<Value>();
            }
        }

        public byte Slot { get; set; }
        public int EntityId { get; set; }
        public string Name
        {
            get
            {
                InfoKey nameInfoKey = InfoKeys.SingleOrDefault(ik => ik.Key == "name");

                if (nameInfoKey == null)
                {
                    return null;
                }

                return nameInfoKey.NewestValueInnerValue;
            }
        }
        public string TeamName { get; set; }
        public List<InfoKey> InfoKeys { get; private set; }

        public Player()
        {
            InfoKeys = new List<InfoKey>();
        }

        public void AddInfoKeyValue(string key, string value, float timestamp)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("String cannot be empty or null.", "key");
            }
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("String cannot be empty or null.", "value");
            }

            InfoKey infoKey = InfoKeys.SingleOrDefault(ik => ik.Key == key);

            if (infoKey == null)
            {
                infoKey = new InfoKey(key);
                InfoKeys.Add(infoKey);
            }

            if (infoKey.NewestValueInnerValue == value)
            {
                return;
            }

            InfoKey.Value infoKeyValue = new InfoKey.Value(value, timestamp);
            infoKey.Values.Add(infoKeyValue);
        }
    }
}
