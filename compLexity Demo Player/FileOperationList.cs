using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections;

namespace compLexity_Demo_Player
{
    /// <summary>
    /// Handles logging of file operations that should be performed on program startup after a crash has occured.
    /// </summary>
    static public class FileOperationList
    {
        static private List<FileOperation> operations;
        static private String programPath;
        static private String fileName;

        static public void Add(FileOperation op)
        {
            operations.Add(op);

            // serialize
            Common.XmlFileSerialize(programPath + "\\" + fileName, operations, typeof(ArrayList), new Type[] { typeof(FileMoveOperation), typeof(FileDeleteOperation), typeof(ExtractZipFolderOperation) });
        }

        static public void Execute()
        {
            // execute operations in reverse order
            for (int i = operations.Count - 1; i >= 0; i--)
            {
                try
                {
                    operations[i].Execute();
                }
                catch (Exception)
                {
                    // ignore errors
                }
            }

            operations.Clear();
            File.Delete(programPath + "\\" + fileName);
        }

        static public void Initialise(String _programPath, String _fileName)
        {
            programPath = _programPath;
            fileName = _fileName;

            operations = new List<FileOperation>();

            String configFullPath = programPath + "\\" + fileName;

            if (!File.Exists(configFullPath))
            {
                return;
            }

            // deserialize
            operations = (List<FileOperation>)Common.XmlFileDeserialize(configFullPath, typeof(ArrayList), new Type[] { typeof(FileMoveOperation), typeof(ExtractZipFolderOperation) });
        }
    }
}
