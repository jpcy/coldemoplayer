using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;

namespace Update_Manifest_Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Entry> manifest = new List<Entry>();
            MD5 md5 = MD5.Create();

            FindFiles(new DirectoryInfo(Environment.CurrentDirectory), (relativeFileName) =>
            {
                Byte[] hash;

                using (FileStream stream = File.OpenRead(Environment.CurrentDirectory + "\\" + relativeFileName))
                {
                    hash = md5.ComputeHash(stream);
                }

                StringBuilder hashHexString = new StringBuilder();

                for (Int32 i = 0; i < hash.Length; i++)
                {
                    hashHexString.Append(hash[i].ToString("X2"));
                }

                manifest.Add(new Entry() { FileName = relativeFileName, Hash = hashHexString.ToString() });
            });

            XmlSerializer serializer = new XmlSerializer(typeof(List<Entry>), new XmlRootAttribute("Entries"));

            using (FileStream stream = File.Create("manifest.xml"))
            {
                serializer.Serialize(stream, manifest);
            }
        }

        private static void FindFiles(DirectoryInfo dirInfo, Action<String> callback)
        {
            if ((dirInfo.Attributes & FileAttributes.Hidden) != 0)
            {
                return;
            }

            foreach (FileInfo fileInfo in dirInfo.GetFiles())
            {
                if (fileInfo.FullName != Environment.GetCommandLineArgs()[0])
                {
                    callback(fileInfo.FullName.Remove(0, Environment.CurrentDirectory.Length + 1));
                }
            }

            foreach (DirectoryInfo childDirInfo in dirInfo.GetDirectories())
            {
                FindFiles(childDirInfo, callback);
            }
        }
    }

   public class Entry
   {
       [XmlAttribute("FileName")]
       public String FileName;

       [XmlAttribute("Hash")]
       public String Hash;
   }
}
