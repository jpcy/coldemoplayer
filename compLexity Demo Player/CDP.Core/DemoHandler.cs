using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;

namespace CDP.Core
{
    /// <summary>
    /// Represents a desciption for how a certain demo type should be handled by the demo player.
    /// </summary>
    public abstract class DemoHandler
    {
        public class PlayerColumn
        {
            public string Header { get; private set; }
            public string DisplayMemberBinding { get; private set; }

            public PlayerColumn(string header, string displayMemberBinding)
            {
                Header = header;
                DisplayMemberBinding = displayMemberBinding;
            }
        }

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

        public abstract string FullName { get; }
        public abstract string Name { get; }
        public abstract string[] Extensions { get; } // e.g. "dem".
        public abstract PlayerColumn[] PlayerColumns { get; }
        public abstract Setting[] Settings { get; }
        public abstract UserControl SettingsView { get; protected set; }

        public abstract bool IsValidDemo(Stream stream);
    }
}
