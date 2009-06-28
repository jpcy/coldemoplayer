using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Diagnostics; // Process
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Windows.Controls;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace compLexity_Demo_Player
{
    public static class Common
    {
        public static void XmlFileSerialize(String fileName, Object o, Type type)
        {
            XmlFileSerialize(fileName, o, type, null);
        }

        public static void XmlFileSerialize(String fileName, Object o, Type type, Type[] extraTypes)
        {
            XmlSerializer serializer;

            if (extraTypes == null)
            {
                serializer = new XmlSerializer(type);
            }
            else
            {
                serializer = new XmlSerializer(type, extraTypes);
            }

            using (TextWriter writer = new StreamWriter(fileName))
            {
                serializer.Serialize(writer, o);
            }
        }

        public static Object XmlFileDeserialize(String fileName, Type type)
        {
            return XmlFileDeserialize(fileName, type, null);
        }

        public static Object XmlFileDeserialize(String fileName, Type type, Type[] extraTypes)
        {
            Object result = null;

            using (TextReader stream = new StreamReader(fileName))
            {
                result = XmlFileDeserialize((StreamReader)stream, type, extraTypes);
            }

            return result;
        }

        public static Object XmlFileDeserialize(StreamReader stream, Type type, Type[] extraTypes)
        {
            XmlSerializer serializer;

            if (extraTypes == null)
            {
                serializer = new XmlSerializer(type);
            }
            else
            {
                serializer = new XmlSerializer(type, extraTypes);
            }

            return serializer.Deserialize(stream);
        }

        public static Process FindProcess(String fileNameWithoutExtension)
        {
            return FindProcess(fileNameWithoutExtension, null);
        }

        public static Process FindProcess(String fileNameWithoutExtension, String exeFullPath)
        {
            return FindProcess(fileNameWithoutExtension, exeFullPath, -1);
        }

        public static Process FindProcess(String fileNameWithoutExtension, String exeFullPath, Int32 ignoreId)
        {
            Process[] processes = Process.GetProcessesByName(fileNameWithoutExtension);

            if (exeFullPath == null)
            {
                if (processes.Length > 0)
                {
                    return processes[0];
                }

                return null;
            }

            foreach (Process p in processes)
            {
                String compare = exeFullPath; // just incase the following fucks up...

                // ugly fix for weird error message.
                // possible cause: accessing MainModule too soon after process has executed.
                // FileName seems to be read just find though so...
                try
                {
                    compare = p.MainModule.FileName;
                }
                catch (System.ComponentModel.Win32Exception)
                {
                }

                if (p.Id != ignoreId && String.Equals(exeFullPath, compare, StringComparison.CurrentCultureIgnoreCase))
                {
                    return p;
                }
            }

            return null;
        }

        public static String SanitisePath(String s)
        {
            return s.Replace('/', '\\');
        }

        public static MessageWindow.Result Message(String message)
        {
            return Message(null, message);
        }

        public static MessageWindow.Result Message(System.Windows.Window owner, String message)
        {
            return Message(owner, message, null);
        }

        public static MessageWindow.Result Message(System.Windows.Window owner, String message, Exception ex)
        {
            return Message(owner, message, ex, 0);
        }

        public static MessageWindow.Result Message(System.Windows.Window owner, String message, Exception ex, MessageWindow.Flags flags)
        {
            return Message(owner, message, ex, flags, null);
        }

        public static MessageWindow.Result Message(System.Windows.Window owner, String message, Exception ex, MessageWindow.Flags flags, Procedure<MessageWindow.Result> setResult)
        {
            MessageWindow window = new MessageWindow(message, ex, flags, setResult);
            window.ShowInTaskbar = (owner == null);
            window.Owner = owner;
            window.WindowStartupLocation = (owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner);
            window.ShowDialog();

            return window.CloseResult;
        }

        /// <summary>
        /// Returns a string representation of the specified number in the format H:MM:SS.
        /// </summary>
        /// <param name="d">The duration in seconds.</param>
        /// <returns></returns>
        public static String DurationString(Single d)
        {
            TimeSpan ts = new TimeSpan(0, 0, (Int32)d);
            return String.Format("{0}{1}:{2}", (ts.Hours > 0 ? (ts.Hours.ToString() + ":") : ""), ts.Minutes.ToString("00"), ts.Seconds.ToString("00"));
        }

        public static String ReadNullTerminatedString(BinaryReader br, Int32 charCount)
        {
            Int64 startingByteIndex = br.BaseStream.Position;
            String s = ReadNullTerminatedString(br);

            Int32 bytesToSkip = charCount - (Int32)(br.BaseStream.Position - startingByteIndex);
            br.BaseStream.Seek(bytesToSkip, SeekOrigin.Current);

            return s;
        }

        // TODO: replace with bitbuffer algorithm? benchmark and see which is faster
        public static String ReadNullTerminatedString(BinaryReader br)
        {
            List<Char> chars = new List<Char>();

            while (true)
            {
                Char c = br.ReadChar();

                if (c == '\0')
                    break;

                chars.Add(c);
            }

            return new String(chars.ToArray());
        }

        public static void ZipExtractFolder(String zipFileName, String folderPath, String destinationPath)
        {
            using (FileStream fileStream = File.OpenRead(zipFileName))
            {
                using (ZipInputStream stream = new ZipInputStream(fileStream))
                {
                    ZipEntry entry;

                    while ((entry = stream.GetNextEntry()) != null)
                    {
                        String directoryName = Path.GetDirectoryName(entry.Name);
                        String fileName = Path.GetFileName(entry.Name);

                        if (directoryName.StartsWith(folderPath))
                        {
                            String newDirectoryName = directoryName.Replace(folderPath, "");

                            if (directoryName != folderPath)
                            {
                                if (!Directory.Exists(destinationPath + newDirectoryName))
                                {
                                    Directory.CreateDirectory(destinationPath + newDirectoryName);
                                }
                            }

                            if (fileName != String.Empty)
                            {
                                ZipStreamWriteFile(stream, destinationPath + newDirectoryName + "\\" + fileName);
                            }
                        }
                    }
                }
            }
        }

        private static void ZipStreamWriteFile(ZipInputStream stream, String fileFullPath)
        {
            const Int32 blockSize = 4096;

            using (FileStream streamWriter = File.Create(fileFullPath))
            {
                Byte[] data = new Byte[blockSize];

                while (true)
                {
                    Int32 bytesRead = stream.Read(data, 0, data.Length);

                    if (bytesRead > 0)
                    {
                        streamWriter.Write(data, 0, bytesRead);
                    }
                    else
                    {
                        break;
                    }
                }

            }
        }

        public static Int32 LogBase2(Int32 number)
        {
            Debug.Assert(number == 1 || number % 2 == 0);

            Int32 result = 0;

            while ((number >>= 1) != 0)
            {
                result++;
            }

            return result;
        }

        public static void AbortThread(System.Threading.Thread thread)
        {
            Debug.Assert(thread != null);

            if (!thread.IsAlive)
            {
                return;
            }

            thread.Abort();
            thread.Join();

           /* do
            {
                thread.Abort();
            }
            while (!thread.Join(100));*/
        }

        public static TSource FirstOrDefault<TSource>(IEnumerable<TSource> source, Function<TSource, bool> predicate)
        {
            foreach (TSource item in source)
            {
                if (predicate(item))
                {
                    return item;
                }
            }

            return default(TSource);
        }

        public static void LogException(Exception e)
        {
            LogException(e, false);
        }

        public static void LogException(Exception e, bool warning)
        {
            String logsFullFolderPath = Config.ProgramDataPath + "\\logs";

            if (!Directory.Exists(logsFullFolderPath))
            {
                Directory.CreateDirectory(logsFullFolderPath);
            }

            using (TextWriter writer = new StreamWriter(logsFullFolderPath + "\\" + DateTime.Now.ToShortDateString().Replace('/', '-') + "_" + (warning == true ? "warning_" : String.Empty) + Path.ChangeExtension(Path.GetRandomFileName(), ".log")))
            {
                Procedure<Exception> logException = null;

                logException = delegate(Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        logException(ex.InnerException);
                    }

                    writer.WriteLine(ex.Message);
                    writer.WriteLine("Type: " + e.GetType().ToString());
                    writer.WriteLine("Source: " + ex.Source);
                    writer.WriteLine("Inner Exception: " + (e != ex).ToString());
                    writer.WriteLine(ex.StackTrace);
                    writer.WriteLine();
                };

                logException(e);
            }
        }

        /// <summary>
        /// Calculates a Steam ID from a "*sid" Half-Life infokey value.
        /// </summary>
        /// <param name="sidInfoKeyValue">The "*sid" infokey value.</param>
        /// <returns>A Steam ID in the format "STEAM_0:x:y".</returns>
        public static String CalculateSteamId(String sidInfoKeyValue)
        {
            UInt64 sid = UInt64.Parse(sidInfoKeyValue);

            if (sid == 0)
            {
                return "HLTV";
            }

            UInt64 authId = sid - 76561197960265728;
            Int32 serverId = ((authId % 2) == 0 ? 0 : 1);
            authId = (authId - (UInt64)serverId) / 2;
            return String.Format("STEAM_0:{0}:{1}", serverId, authId);
        }

        /// <summary>
        /// Copies the column values of the selected rows in the given ListView to the clipboard. Column values are comma separated, each row is on a new line.
        /// </summary>
        /// <remarks>A column's value is read from the first TextBlock element in the column.</remarks>
        public static void ListViewCopySelectedRowsToClipboard(ListView listView)
        {
            if (listView.SelectedItems.Count == 0)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            Action<DependencyObject> enumerateVisual = null;
            bool prependComma = false;

            enumerateVisual = dobj =>
            {
                for (int j = 0; j < VisualTreeHelper.GetChildrenCount(dobj); j++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dobj, j);
                    TextBlock textBlock = child as TextBlock;

                    if (textBlock != null)
                    {
                        if (prependComma)
                        {
                            sb.Append(", ");
                        }
                        else
                        {
                            prependComma = true;
                        }

                        sb.Append(textBlock.Text);
                        return;
                    }

                    enumerateVisual(child);
                }
            };

            foreach (object o in listView.SelectedItems)
            {
                prependComma = false;
                enumerateVisual(listView.ItemContainerGenerator.ContainerFromItem(o));
                sb.Append(Environment.NewLine);
            }

            try
            {
                Clipboard.SetText(sb.ToString());
            }
            catch (System.Runtime.InteropServices.COMException)
            {
            }
        }
    }

    [ValueConversion(typeof(Object), typeof(Object))]
    public class NullConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return "-";
            }

            return value;
        }

        public object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Single), typeof(String))]
    public class TimestampConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            return Common.DurationString((Single)value);
        }

        public object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Func
    public delegate TResult Function<TResult>();
    public delegate TResult Function<T, TResult>(T arg);
    public delegate TResult Function<T1, T2, TResult>(T1 arg1, T2 arg2);
    public delegate TResult Function<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult Function<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    // Action
    public delegate void Procedure();
    public delegate void Procedure<T1>(T1 arg);
    public delegate void Procedure<T1, T2>(T1 arg1, T2 arg2);
    public delegate void Procedure<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Procedure<T1, T2, T3, T4>(T1 arg1, T2 arg2, T4 arg4);
}
