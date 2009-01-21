using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

// TODO: remove this? analyser uses different code

namespace compLexity_Demo_Player
{
    public class InfoKeyValue
    {
        public String Value;
        public Single Timestamp;

        public InfoKeyValue()
        {
        }

        public InfoKeyValue(String value, Single timestamp)
        {
            Value = value;
            Timestamp = timestamp;
        }
    }

    public class InfoKey
    {
        public String Key;
        public LinkedList<InfoKeyValue> Values;

        public InfoKey()
        {
        }

        public InfoKey(String key)
        {
            Key = key;
            Values = new LinkedList<InfoKeyValue>();
        }

        public String FindNewestValue()
        {
            if (Values.Last == null)
            {
                // shouldn't happen, there should always be at least one value
                throw new ApplicationException("InfoKey without a value.");
            }

            return Values.Last.Value.Value; // lol
        }

        public void Add(InfoKeyValue value)
        {
            Values.AddLast(value);
        }
    }

    public class InfoKeyList
    {
        public LinkedList<InfoKey> InfoKeys;

        public InfoKeyList()
        {
            InfoKeys = new LinkedList<InfoKey>();
        }

        public void Add(String key, String value, Single timestamp)
        {
            InfoKey infoKey = Common.FirstOrDefault<InfoKey>(InfoKeys, ik => ik.Key == key);

            if (infoKey == null)
            {
                infoKey = new InfoKey(key);

                // add new infokey
                InfoKeys.AddLast(infoKey);
            }
            else
            {
                // don't create a new value entry if the previous value is exactly the same
                if (infoKey.FindNewestValue() == value)
                {
                    return;
                }
            }

            // create infokey value
            InfoKeyValue infoKeyValue = new InfoKeyValue(value, timestamp);

            // add new value to infokey values
            infoKey.Add(infoKeyValue);
        }

        public String FindNewestValue(String key)
        {
            InfoKey result = Common.FirstOrDefault<InfoKey>(InfoKeys, ik => ik.Key == key);

            if (result == null)
            {
                return null;
            }

            return result.FindNewestValue();
        }
    }
}
