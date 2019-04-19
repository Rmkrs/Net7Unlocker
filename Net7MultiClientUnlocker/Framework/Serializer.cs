namespace Net7MultiClientUnlocker.Framework
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;

    public static class Serializer
    {
        private static readonly Dictionary<Type, XmlSerializer> XmlSerializerCache = new Dictionary<Type, XmlSerializer>();

        public static XmlSerializer CreateDefaultXmlSerializer(Type type)
        {
            XmlSerializer serializer;
            if (XmlSerializerCache.TryGetValue(type, out serializer))
            {
                return serializer;
            }

            var importer = new XmlReflectionImporter();
            var mapping = importer.ImportTypeMapping(type, null, null);
            serializer = new XmlSerializer(mapping);
            return XmlSerializerCache[type] = serializer;
        }

        public static void Serialize<T>(T value, string filename)
            where T : class
        {
            if (value == null)
            {
                return;
            }

            try
            {
                var serializer = CreateDefaultXmlSerializer(typeof(T));
                EnsurePath(filename);
                var stream = new FileStream(filename, FileMode.Create);
                serializer.Serialize(stream, value);
                stream.Close();
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        public static T Deserialize<T>(string filename) where T : new()
        {
            try
            {
                var serializer = CreateDefaultXmlSerializer(typeof(T));
                var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                var result = (T)serializer.Deserialize(stream);
                stream.Close();
                return result;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        private static void EnsurePath(string filename)
        {
            var directoryInfo = new FileInfo(filename).Directory;
            directoryInfo?.Create();
        }
    }
}
