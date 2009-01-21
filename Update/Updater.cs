using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.Security.Cryptography;

namespace Update
{
    public class Updater
    {
        public class Entry
        {
            [XmlAttribute("FileName")]
            public String FileName { get; set; }

            [XmlAttribute("Hash")]
            public String Hash { get; set; }
        }

        private const Single findProcessTimeout = 2500.0f;
        private readonly String updateUrl = "http://coldemoplayer.gittodachoppa.com/update/";
        private readonly String manifestFileName = "manifest.xml";
        private readonly String workingDirectory;
        private readonly Procedure<String> appendToLog;
        private readonly Procedure signalPointOfNoReturn;
        private readonly Procedure<Boolean> signalCompletion;

        public Updater(Procedure<String> appendToLog, Procedure signalPointOfNoReturn, Procedure<Boolean> signalCompletion)
        {
            this.appendToLog = appendToLog;
            this.signalPointOfNoReturn = signalPointOfNoReturn;
            this.signalCompletion = signalCompletion;
            workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        public void ThreadStart()
        {
            try
            {
                ThreadWorker();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                appendToLog(ex.Message);
                signalCompletion(false);
            }
        }

        private void ThreadWorker()
        {
            // wait for/close the demo player process
            Process demoPlayerProcess = FindProcess("compLexity Demo Player", workingDirectory + "\\compLexity Demo Player.exe");

            if (demoPlayerProcess != null)
            {
                appendToLog("Waiting for compLexity Demo Player to close");
                Stopwatch watch = new Stopwatch();
                watch.Start();

                while (!demoPlayerProcess.HasExited)
                {
                    Thread.Sleep(250);

                    if (watch.ElapsedMilliseconds > findProcessTimeout)
                    {
                        demoPlayerProcess.Kill(); // fuck it, kill the process
                        watch.Start();
                    }

                    appendToLog(".");
                }

                appendToLog("\n");
            }

            // download update manifest
            appendToLog("Downloading update manifest...\n");

            WebRequest request = WebRequest.Create(updateUrl + manifestFileName);
            List<Entry> manifest;

            using (WebResponse response = request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Entry>), new XmlRootAttribute("Entries"));
                    manifest = (List<Entry>)serializer.Deserialize(stream);
                }
            }

            // generate a list of files that need to be downloaded
            appendToLog("Generating download list...\n");
            List<String> downloadList = new List<String>();
            MD5 md5 = MD5.Create();

            Procedure<String> addToDownloadList = (fileName) =>
            {
                downloadList.Add(fileName);
                appendToLog("   " + fileName + "\n");
            };

            foreach (Entry entry in manifest)
            {
                String fileName = workingDirectory + "\\" + entry.FileName;

                if (File.Exists(fileName))
                {
                    using (FileStream stream = File.OpenRead(fileName))
                    {
                        StringBuilder hashHexString = new StringBuilder();
                        Byte[] hash = md5.ComputeHash(stream);

                        for (Int32 i = 0; i < hash.Length; i++)
                        {
                            hashHexString.Append(hash[i].ToString("X2"));
                        }

                        if (!String.Equals(entry.Hash, hashHexString.ToString(), StringComparison.Ordinal))
                        {
                            addToDownloadList(entry.FileName);
                        }
                    }
                }
                else
                {
                    addToDownloadList(entry.FileName);
                }
            }

            // TODO: download individual files, overwrite existing files and create folders as needed
            signalPointOfNoReturn();
            appendToLog("Downloading files...\n");

            foreach (String fileName in downloadList)
            {
                appendToLog("   " + fileName);

                // create any needed folders
                String fullPath = workingDirectory + "\\" + Path.GetDirectoryName(fileName);

                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                // download the file
                WebDownloadBinaryFile(updateUrl + fileName.Replace('\\', '/'), workingDirectory + "\\" + fileName, new Action<Int32>((percent) =>
                {
                    if (percent != 0 && percent % 10 == 0)
                    {
                        appendToLog(".");
                    }
                }));

                appendToLog("\n");
            }

            // update complete
            signalCompletion(true);
        }

        private static Process FindProcess(String fileNameWithoutExtension, String exeFullPath)
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

                if (String.Equals(exeFullPath, compare, StringComparison.CurrentCultureIgnoreCase))
                {
                    return p;
                }
            }

            return null;
        }

        public static void WebDownloadBinaryFile(String address, String outputFileName, Action<Int32> downloadProgress)
        {
            const Int32 bufferSize = 1024;

            WebRequest request = WebRequest.Create(address);
            using (WebResponse response = request.GetResponse())
            using (Stream input = response.GetResponseStream())
            using (FileStream output = File.Create(outputFileName))
            using (BinaryReader reader = new BinaryReader(input))
            using (BinaryWriter writer = new BinaryWriter(output))
            {
                Int64 fileLength = response.ContentLength;
                Int32 bytesRead = 0;

                while (true)
                {
                    Byte[] buffer = reader.ReadBytes(bufferSize);

                    bytesRead += buffer.Length;

                    if (downloadProgress != null)
                    {
                        downloadProgress((Int32)(bytesRead / (Single)fileLength * 100.0f));
                    }

                    if (buffer.Length > 0)
                    {
                        writer.Write(buffer);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
