using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.Core
{
    public interface IErrorReporter
    {
        /// <summary>
        /// Log a warning to disk. Warnings are stored in %APPDATA%\compLexity Demo Player\warnings.
        /// </summary>
        /// <param name="message">A message to log (optional if ex is not null).</param>
        /// <param name="ex">An exception to log (optional if message is not null).</param>
        void LogWarning(string message, Exception ex);

        /// <summary>
        /// Log an unhandled exception to disk.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        void LogUnhandledException(Exception ex);

        /// <summary>
        /// Determine whether an unhandled exception has been logged to disk.
        /// </summary>
        /// <returns>True if the unhandled exception log file exists, otherwise false.</returns>
        bool UnhandledExceptionLogExists();

        /// <summary>
        /// Read the contents of the unhandled exception log.
        /// </summary>
        /// <returns>The contents of the unhandled exception log as a single string.</returns>
        string ReadUnhandledExceptionLog();
    }

    /// <remarks>
    /// This should not have dependencies on any local code (e.g. Settings, FileSystem classes).
    /// </remarks>
    public class ErrorReporter : IErrorReporter
    {
        private readonly string programDataPath;
        private readonly string appDataFolderName = "compLexity Demo Player";
        private readonly string warningsFolderName = "warnings";
        private readonly string unhandledExceptionFileName = "unhandledexception.log";

        public ErrorReporter()
        {
            // %APPDATA%\compLexity Demo Player
            programDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appDataFolderName);

            if (!Directory.Exists(programDataPath))
            {
                Directory.CreateDirectory(programDataPath);
            }
        }

        public void LogWarning(string message, Exception ex)
        {
            if (string.IsNullOrEmpty(message) && ex == null)
            {
                throw new ArgumentException("message and ex cannot both be empty/null.");
            }

            string warningsFolderPath = Path.Combine(programDataPath, warningsFolderName);

            if (!Directory.Exists(warningsFolderPath))
            {
                Directory.CreateDirectory(warningsFolderPath);
            }

            string fileName;

            do
            {
                fileName = Path.Combine(warningsFolderPath, Path.GetRandomFileName());
            }
            while (File.Exists(fileName));

            using (TextWriter writer = new StreamWriter(fileName))
            {
                LogEnvironmentInformation(writer, ex == null);

                if (!string.IsNullOrEmpty(message))
                {
                    writer.WriteLine("Message: \'{0}\'.", message);
                }

                if (ex != null)
                {
                    writer.WriteLine(ex);
                }
            }
        }

        public void LogUnhandledException(Exception ex)
        {
            string fileName = Path.Combine(programDataPath, unhandledExceptionFileName);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            using (TextWriter writer = new StreamWriter(fileName))
            {
                LogEnvironmentInformation(writer, false);
                writer.WriteLine(ex);
            }
        }

        public bool UnhandledExceptionLogExists()
        {
            return File.Exists(Path.Combine(programDataPath, unhandledExceptionFileName));
        }

        public string ReadUnhandledExceptionLog()
        {
            using (TextReader reader = new StreamReader(Path.Combine(programDataPath, unhandledExceptionFileName)))
            {
                return reader.ReadToEnd();
            }
        }

        private void LogEnvironmentInformation(TextWriter writer, bool includeStackTrace)
        {
            writer.WriteLine("Environment.OSVersion: \'{0}\'", Environment.OSVersion);
            writer.WriteLine("Environment.Is64BitOperatingSystem: \'{0}\'", Environment.Is64BitOperatingSystem);
            writer.WriteLine("Environment.Version: \'{0}\'", Environment.Version);

            if (includeStackTrace)
            {
                writer.WriteLine(Environment.StackTrace);
            }

            writer.WriteLine();
        }
    }
}
