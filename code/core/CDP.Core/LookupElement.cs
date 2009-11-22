using System;
using System.Linq;

namespace CDP.Core
{
    public class LookupElement<T>
    {
        public string Key { get; private set; }
        public T Value { get; private set; }
        public bool IsStart { get; private set; }

        public LookupElement(string key, T value, bool isStart)
        {
            Key = key;
            Value = value;
            IsStart = isStart;
        }

        public LookupElement(string key, T value)
            : this(key, value, false)
        {
        }
    }
}