using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CDP.Core;

namespace CDP.UnrealTournament2004
{
    public class Demo : Core.Demo
    {
        public override string GameName
        {
            get { return "Unreal Tournament 2004"; }
        }

        public override string MapName { get; protected set; }
        public override string Perspective { get; protected set; }
        public override TimeSpan Duration { get; protected set; }
        public override ArrayList Players { get; protected set; }

        public override string[] IconFileNames
        {
            get
            {
                return new string[]
                {
                    fileSystem.PathCombine(settings.ProgramPath, "icons", "ut2004.ico")
                };
            }
        }

        public override string MapImagesRelativePath
        {
            get { return null; }
        }

        public override bool CanAnalyse
        {
            get { return false; }
        }

        public override bool CanPlay
        {
            get { return true; }
        }

        private readonly ISettings settings = ObjectCreator.Get<ISettings>();
        private readonly IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();
        private bool isLoaded = false;

        public override void Load()
        {
            try
            {
                if (isLoaded)
                {
                    throw new ApplicationException("Demo has already been loaded.");
                }
                else
                {
                    isLoaded = true;
                }

                using (Core.FastFileStream stream = new Core.FastFileStream(FileName, Core.FastFileAccess.Read))
                {
                    // Skip magic.
                    stream.Seek(16, SeekOrigin.Begin);

                    // Skip unknown.
                    stream.Seek(5, SeekOrigin.Current);

                    // Map name.
                    MapName = ReadVerifiableString(stream);

                    // Game mode.
                    AddDetail(Strings.DemoDetailGameMode, ReadVerifiableString(stream));

                    // Skip unknown.
                    stream.Seek(12, SeekOrigin.Current);

                    // Recorded by.
                    AddDetail(Strings.DemoDetailRecordedBy, ReadVerifiableString(stream));

                    // Recorded on.
                    AddDetail(Strings.DemoDetailRecordedOn, ReadVerifiableString(stream));
                }
            }
            catch (Exception ex)
            {
                OnOperationError(FileName, ex);
                return;
            }

            OnOperationComplete();
        }

        public override void Read()
        {
            throw new NotImplementedException();
        }

        public override void Write(string destinationFileName)
        {
            if (!isLoaded)
            {
                throw new ApplicationException("Can't write a demo that hasn't been loaded.");
            }

            const int bufferSize = 4096;

            try
            {
                ResetOperationCancelledState();
                ResetProgress();

                using (Core.FastFileStream inputStream = new Core.FastFileStream(FileName, Core.FastFileAccess.Read))
                using (Core.FastFileStream outputStream = new Core.FastFileStream(destinationFileName, Core.FastFileAccess.Write))
                {
                    while (true)
                    {
                        int bytesToRead = bufferSize;

                        if (inputStream.BytesLeft < bytesToRead)
                        {
                            bytesToRead = (int)inputStream.BytesLeft;

                            if (bytesToRead == 0)
                            {
                                break;
                            }
                        }

                        outputStream.WriteBytes(inputStream.ReadBytes(bytesToRead));
                        UpdateProgress(inputStream.Position, inputStream.Length);

                        if (IsOperationCancelled())
                        {
                            OnOperationCancelled();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnOperationError(FileName, ex);
                return;
            }

            OnOperationComplete();
        }

        /// <summary>
        /// Read a string and its length from a stream and verify that the expected length matches the actual string length.
        /// </summary>
        private string ReadVerifiableString(FastFileStream stream)
        {
            byte length = stream.ReadByte();

            if (stream.BytesLeft < length)
            {
                throw new ApplicationException(string.Format("Unexpected end of stream. \'{0}\' bytes left, need \'{1}\'", stream.BytesLeft, length));
            }

            string s = stream.ReadString();
            int stringLength = string.IsNullOrEmpty(s) ? 0 : s.Length + 1;

            if (stringLength != length)
            {
                throw new ApplicationException(string.Format("Unexpected string length. Expected \'{0}\', got \'{1}\'.", length, stringLength));
            }

            return s;
        }
    }
}
