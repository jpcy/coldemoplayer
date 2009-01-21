using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Threading;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Diagnostics;

namespace compLexity_Demo_Player
{
    public partial class DownloadMapWindow : Window
    {
        private Boolean hasActivated = false;
        private Thread thread;
        private readonly String displayFileName;
        private readonly String sourceUrl;
        private readonly String destinationFileName;
        private readonly Stopwatch stopwatch = new Stopwatch();

        public DownloadMapWindow(String displayFileName, String sourceUrl, String destinationFileName)
        {
            InitializeComponent();

            this.displayFileName = displayFileName;
            this.sourceUrl = sourceUrl;
            this.destinationFileName = destinationFileName;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (!hasActivated)
            {
                hasActivated = true;
                stopwatch.Start();
                thread = new Thread(new ThreadStart(ThreadStart));
                thread.Start();
            }
        }

        private void uiCancelButton_Click(object sender, RoutedEventArgs e)
        {
            Common.AbortThread(thread);
            Close();
        }

        private void SetError(String message)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                uiFileDetailsTextBlock.Text = message;
                uiCancelButton.Content = "Close";
            }));
        }

        private void SetProgress(Int64 position, Int64 length, Int32 percent)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                uiFileDetailsTextBlock.Text = String.Format("{0} [{1}/{2}KB] @ {3}KB/s", displayFileName, position / 1024, length / 1024, (Int32)(position / 1024.0f / (Single)(stopwatch.ElapsedMilliseconds / 1000)));
                uiProgressBar.Value = percent;
            }));
        }

        private void CloseWindow(Boolean result)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                DialogResult = result;
            }));
        }

        private void ThreadStart()
        {
            try
            {
                ThreadWorker();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (WebException)
            {
                CloseWindow(false);
                return;
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
                return;
            }
            finally
            {
                // delete the zip file
                File.Delete(destinationFileName);
            }

            CloseWindow(true);
        }

        private void ThreadWorker()
        {
            // create destination folder
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFileName));

            // download the file
            const Int32 bufferSize = 32768;
            Int32 lastPercent = 0;

            WebRequest request = WebRequest.Create(sourceUrl);
            using (WebResponse response = request.GetResponse())
            {
                using (BinaryReader reader = new BinaryReader(response.GetResponseStream()))
                {
                    using (BinaryWriter writer = new BinaryWriter(File.Create(destinationFileName)))
                    {
                        Int64 fileLength = response.ContentLength;
                        Int32 bytesRead = 0;

                        while (true)
                        {
                            Byte[] buffer = reader.ReadBytes(bufferSize);
                            bytesRead += buffer.Length;
                            Int32 percent = (Int32)(bytesRead / (Single)fileLength * 100.0f);

                            if (percent != lastPercent)
                            {
                                SetProgress(bytesRead, fileLength, percent);
                            }

                            lastPercent = percent;

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

            // extract the zip file
            ZipExtract(destinationFileName);
        }

        public static void ZipExtract(String zipFileName)
        {
            String destinationPath = Path.GetDirectoryName(zipFileName);

            using (ZipInputStream stream = new ZipInputStream(File.OpenRead(zipFileName)))
            {
                ZipEntry entry;

                while ((entry = stream.GetNextEntry()) != null)
                {
                    String directoryName = "\\" + Path.GetDirectoryName(entry.Name);
                    String fileName = Path.GetFileName(entry.Name);

                    // create directory if it doesn't exist
                    String newDirectoryFullPath = destinationPath + directoryName;

                    if (!Directory.Exists(newDirectoryFullPath))
                    {
                        Directory.CreateDirectory(newDirectoryFullPath);
                    }

                    // extract file
                    if (fileName != String.Empty)
                    {
                        ZipStreamWriteFile(stream, destinationPath + directoryName + "\\" + fileName);
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
    }
}
