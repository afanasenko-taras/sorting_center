using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace AbstractModel
{
    public class Helper
    {
        public static byte[] SerializeXML<T>(T stamp)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(T));
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, stamp);
            return stream.ToArray();
        }



        public static void SerializeXMLToFile<T>(T stamp, string filePath)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(T));
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                formatter.Serialize(fileStream, stamp);
            }
        }

        public static T DeserializeXMLFromFile<T>(string filePath)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(T));
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                return (T)formatter.Deserialize(fileStream);
            }
        }

        public static T DeserializeXML<T>(byte[] binaryData)
        {
            var formatter = new XmlSerializer(typeof(T));
            var ms = new MemoryStream(binaryData);
            return (T)formatter.Deserialize(ms);
        }

    }
}
