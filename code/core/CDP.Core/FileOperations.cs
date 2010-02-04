using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JsonExSerializer;

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
        private readonly string jsonFileName = "fileoperations.json";
        private Stack<FileOperation> operations;

        public FileOperations()
        {
            operations = new Stack<FileOperation>();
        }
        
        public void Load()
        {
            string fileName = fileSystem.PathCombine(settings.ProgramUserDataPath, jsonFileName);

            if (!fileSystem.FileExists(fileName))
            {
                return;
            }

            // Deserialize.
            using (StreamReader stream = new StreamReader(fileName))
            {
                Serializer serializer = new Serializer(typeof(Stack<FileOperation>));
                operations = (Stack<FileOperation>)serializer.Deserialize(stream);
            }
        }

        public void Execute()
        {
            while (operations.Count > 0)
            {
                FileOperation op = operations.Pop();

                try
                {
                    op.Execute();
                }
                catch (Exception ex)
                {
                    ObjectCreator.Get<IErrorReporter>().LogWarning(null, ex);
                }
            }

            // Delete the file.
            string fileName = fileSystem.PathCombine(settings.ProgramUserDataPath, jsonFileName);

            if (fileSystem.FileExists(fileName))
            {
                fileSystem.DeleteFile(fileName);
            }
        }

        public void Add(FileOperation fileOperation)
        {
             operations.Push(fileOperation);
            string fileName = fileSystem.PathCombine(settings.ProgramUserDataPath, jsonFileName);

            // Serialize.
            using (StreamWriter stream = new StreamWriter(fileName))
            {
                Serializer serializer = new Serializer(typeof(Stack<FileOperation>));
                serializer.Serialize(operations, stream);
            }
        }
  
    }
}
