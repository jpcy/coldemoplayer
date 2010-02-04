using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using CDP.Core;
using CDP.Core.Extensions;

namespace CDP.Gui
{
    internal interface IFileAssociation
    {
        /// <summary>
        /// Associates the given file name extensions with the program.
        /// </summary>
        /// <param name="extensions">The list of file extensions to associate.</param>
        /// <param name="defaultShellCommand">The default double-click shell command.</param>
        /// <returns>True if the icon has changed and a refresh is required.</returns>
        bool Associate(string[] extensions, string defaultShellCommand);

        /// <summary>
        /// Refresh shell icons.
        /// </summary>
        void RefreshIcons();
    }

    internal class FileAssociation : IFileAssociation
    {
        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(int eventId, uint flags, IntPtr item1, IntPtr item2);
        private readonly string programEntryName = "compLexity Demo Player";

        public bool Associate(string[] extensions, string defaultShellCommand)
        {
            if (Debugger.IsAttached)
            {
                return false;
            }

            if (extensions == null)
            {
                throw new ArgumentNullException("extensions");
            }

            if (defaultShellCommand == null)
            {
                throw new ArgumentNullException("defaultShellCommand");
            }

            if (defaultShellCommand != "open" && defaultShellCommand != "play")
            {
                throw new ArgumentException("Invalid command \'{0}\'. Valid commands are \'open\' and \'play\'.".Args(defaultShellCommand), "defaultShellCommand");
            }

            // Create ".*" entries.
            foreach (string e in extensions)
            {
                // Make sure the extension starts with ".".
                string extension = e.StartsWith(".") ? e : "." + e;

                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(extension))
                {
                    key.SetValue(string.Empty, programEntryName);
                }
            }

            // Create program entry.
            bool refresh = false;
            ISettings settings = ObjectCreator.Get<ISettings>();
            IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();

            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(programEntryName))
            {
                // Icon.
                using (RegistryKey subkey = key.CreateSubKey("DefaultIcon"))
                {
                    if ((string)subkey.GetValue(string.Empty) != settings.ProgramExeFullPath)
                    {
                        refresh = true;
                    }

                    subkey.SetValue(string.Empty, settings.ProgramExeFullPath);
                }

                // Default shell command.
                using (RegistryKey subkey = key.CreateSubKey("shell"))
                {
                    subkey.SetValue(string.Empty, defaultShellCommand);
                }

                // Open.
                CreateShellEntries(key, "open", Strings.ShellCommandOpen, "\"" + settings.ProgramExeFullPath + "\" \"%1\"");

                // Play.
                CreateShellEntries(key, "play", Strings.ShellCommandPlay, "\"" + fileSystem.PathCombine(fileSystem.GetDirectoryName(settings.ProgramExeFullPath), "CDP.Cli.exe") + "\" -pause \"%1\"");
            }

            return refresh;
        }

        private void CreateShellEntries(RegistryKey key, string commandName, string commandDescription, string command)
        {
            using (RegistryKey subkey = key.CreateSubKey("shell\\" + commandName))
            {
                subkey.SetValue(string.Empty, commandDescription);
            }

            using (RegistryKey subkey = key.CreateSubKey("shell\\" + commandName + "\\command"))
            {
                subkey.SetValue(string.Empty, command);
            }
        }

        public void RefreshIcons()
        {
            const int SHCNE_ASSOCCHANGED = 0x08000000;
            const int SHCNF_IDLIST = 0;
            IntPtr p = new IntPtr();
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, p, p);
        }
    }
}
