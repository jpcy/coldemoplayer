using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace compLexity_Demo_Player
{
    public abstract class FileOperation
    {
        public abstract void Execute();
    }

    public class FileMoveOperation : FileOperation
    {
        [XmlAttribute("sourceFileName")]
        public String SourceFileName;

        [XmlAttribute("destinationFilename")]
        public String DestinationFileName;

        public FileMoveOperation()
        {
        }

        public FileMoveOperation(String sourceFileName, String destinationFileName)
        {
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
        [XmlAttribute("fileName")]
        public String FileName;

        public FileDeleteOperation()
        {
        }

        public FileDeleteOperation(String fileName)
        {
            this.FileName = fileName;
        }

        public override void Execute()
        {
            File.Delete(FileName);
        }
    }

    public class ExtractZipFolderOperation : FileOperation
    {
        [XmlAttribute("zipFileName")]
        public String ZipFileName;

        [XmlAttribute("zipFolderPath")]
        public String ZipFolderPath;

        [XmlAttribute("destinationPath")]
        public String DestinationPath;

        public ExtractZipFolderOperation()
        {
        }

        public ExtractZipFolderOperation(String zipFileName, String zipFolderPath, String destinationPath)
        {
            this.ZipFileName = zipFileName;
            this.ZipFolderPath = zipFolderPath;
            this.DestinationPath = destinationPath;
        }

        public override void Execute()
        {
            Common.ZipExtractFolder(ZipFileName, ZipFolderPath, DestinationPath);
        }
    }
}
