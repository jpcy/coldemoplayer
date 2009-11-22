using System;

namespace CDP.Core
{
    public class Setting
    {
        public override bool Equals(object obj)
        {
            Setting setting = obj as Setting;

            if (obj != null)
            {
                return Key == setting.Key;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public string Key { get; private set; }
        public Type Type { get; private set; }
        public object DefaultValue { get; private set; }

        public Setting(string key, Type type, object defaultValue)
        {
            Key = key;
            Type = type;
            DefaultValue = defaultValue;
        }
    }
}
