using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SevenZip;

namespace CDP.Core
{
    /// <summary>
    /// A compressed archive of game resources. For example, a map and associated textures, models and sounds.
    /// </summary>
    public class GameResourceArchive
    {
        public class ContainsBackupFileException : Exception
        {
            public ContainsBackupFileException(string backupFileExtension)
                : base(string.Format("Archive contains a file with the same extension as backup files (.{0}).", backupFileExtension))
            {
            }
        }

        private readonly string backupFileExtension = "bak";
        private string fileName;

        public GameResourceArchive(string fileName)
        {
            this.fileName = fileName;
        }

        /// <summary>
        /// Safely extracts the archive, backing up any existing files that would otherwise be overwritten and adding entries to FileOperations so the original state of the file system can be later restored.
        /// </summary>
        /// <param name="outputPath"></param>
        public void SafeExtract(string outputPath)
        {
            IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();
            IFileOperations fileOperations = ObjectCreator.Get<IFileOperations>();

            using (Stream stream = File.OpenRead(fileName))
            using (SevenZipExtractor extractor = new SevenZipExtractor(stream))
            {
                // Make sure that no files in the archive have the same extension as backup files.
                foreach (string f in extractor.ArchiveFileNames)
                {
                    if (fileSystem.GetExtension(f).ToLower() == backupFileExtension)
                    {
                        throw new ContainsBackupFileException(backupFileExtension);
                    }
                }

                foreach (ArchiveFileInfo fileInfo in extractor.ArchiveFileData)
                {
                    string fileNameFullPath = fileSystem.PathCombine(outputPath, fileInfo.FileName);

                    // If a matching filename already exists, backup the existing file (rename it and add an entry to FileOperations so it can be restored later).
                    if (fileSystem.FileExists(fileNameFullPath))
                    {
                        string backupFileName = fileSystem.ChangeExtension(fileNameFullPath, backupFileExtension);

                        // If a backup already exists, just delete it.
                        if (fileSystem.FileExists(backupFileName))
                        {
                            fileSystem.DeleteFile(backupFileName);
                        }

                        fileOperations.Add(new FileMoveOperation(backupFileName, fileNameFullPath));
                        fileSystem.MoveFile(fileNameFullPath, backupFileName);
                    }

                    if (!fileInfo.IsDirectory)
                    {
                        fileOperations.Add(new FileDeleteOperation(fileNameFullPath));
                    }
                }

                extractor.ExtractArchive(outputPath);
            }
        }
    }
}
