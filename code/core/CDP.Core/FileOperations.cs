using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace CDP.Core
{
    /// <summary>
    /// Maintains a persistent list of file operations to be executed in LIFO order.
    /// </summary>
    public interface IFileOperations
    {
        /// <summary>
        /// Load file operations from disk.
        /// </summary>
        void Load();

        /// <summary>
        /// Execute any pending file operations in LIFO order.
        /// </summary>
        void Execute();

        /// <summary>
        /// Add a file operation.
        /// </summary>
        /// <param name="fileOperation">The file operation to add.</param>
        void Add(FileOperation fileOperation);
    }

    [Singleton]
    public class FileOperations : IFileOperations
    {
        private readonly ISettings settings = ObjectCreator.Get<ISettings>();
        private readonly IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();
        private readonly string xmlFileName = "fileoperations.xml";

        // Stack doesn't support serialization. Converting a stack to/from an array to serialize writes the stack in the wrong order. It's less work to just use a dynamic array.
        private List<FileOperation> operations;

        public FileOperations()
        {
            operations = new List<FileOperation>();
        }
        
        public void Load()
        {
            string fileName = fileSystem.PathCombine(settings.ProgramUserDataPath, xmlFileName);

            if (!fileSystem.FileExists(fileName))
            {
                return;
            }

            // Deserialize.
            using (StreamReader stream = new StreamReader(fileName))
            {
                // Stack doesn't support serialization.
                XmlSerializer serializer = new XmlSerializer(typeof(List<FileOperation>), new Type[] { typeof(FileMoveOperation), typeof(FileDeleteOperation) });
                operations = (List<FileOperation>)serializer.Deserialize(stream);
            }
        }

        public void Execute()
        {
            while (operations.Count > 0)
            {
                FileOperation op = operations.Last();
                operations.Remove(op);

                try
                {
                    op.Execute();
                }
                catch (Exception ex)
                {
                    ObjectCreator.Get<IErrorReporter>().LogWarning(null, ex);
                }
            }

            // Delete the XML file.
            string fileName = fileSystem.PathCombine(settings.ProgramUserDataPath, xmlFileName);

            if (fileSystem.FileExists(fileName))
            {
                fileSystem.DeleteFile(fileName);
            }
        }

        public void Add(FileOperation fileOperation)
        {
            operations.Add(fileOperation);
            string fileName = fileSystem.PathCombine(settings.ProgramUserDataPath, xmlFileName);

            // Serialize.
            using (StreamWriter stream = new StreamWriter(fileName))
            {
                // Stack doesn't support serialization.
                XmlSerializer serializer = new XmlSerializer(typeof(List<FileOperation>), new Type[] { typeof(FileMoveOperation), typeof(FileDeleteOperation) });
                serializer.Serialize(stream, operations);
            }
        }
  
    }
}
