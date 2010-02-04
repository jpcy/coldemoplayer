using System.IO;

namespace CDP.Core
{
    public abstract class FileOperation
    {
        public abstract void Execute();
    }

    public class FileMoveOperation : FileOperation
    {
        public string SourceFileName { get; set; }
        public string DestinationFileName { get; set; }

        public FileMoveOperation()
        {
        }

        public FileMoveOperation(string sourceFileName, string destinationFileName)
        {
            if (string.IsNullOrEmpty(sourceFileName))
            {
                throw new StringArgumentNullOrEmpty("sourceFileName");
            }
            if (string.IsNullOrEmpty(destinationFileName))
            {
                throw new StringArgumentNullOrEmpty("destinationFileName");
            }

            this.SourceFileName = sourceFileName;
            this.DestinationFileName = destinationFileName;
        }

        public override void Execute()
        {
            File.Move(SourceFileName, DestinationFileName);
        }
    }

    public class FileDeleteOperation : FileOperation
    {
        public string FileName { get; set; }

        public FileDeleteOperation()
        {
        }

        public FileDeleteOperation(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new StringArgumentNullOrEmpty("fileName");
            }

            this.FileName = fileName;
        }

        public override void Execute()
        {
            // File.Delete doesn't throw an exception if the file doesn't exist, but it does throw an exception if the path to the file doesn't exist.
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }
        }
    }
}
