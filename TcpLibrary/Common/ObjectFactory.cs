using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TcpLibrary.Common
{
    public class ObjectFactory
    {
        public const string DefaultEncoding = "UTF-8";
        public delegate byte[] ExporterFunc(object obj, out int length, string coding = DefaultEncoding);
        public delegate object ImporterFunc(byte[] input, string coding = DefaultEncoding);

        private static Dictionary<Type, ExporterFunc> base_exporters_table;

        private static Dictionary<Type, ImporterFunc> base_importers_table;

        public ObjectFactory()
        {
            base_exporters_table = new Dictionary<Type, ExporterFunc>();
            base_importers_table = new Dictionary<Type, ImporterFunc>();
            registerExporters();
            registerImporters();
        }
        private void registerExporters()
        {
            RegisterExporter(typeof(byte[]), delegate (object obj, out int length, string coding)
            {
                length = ((byte[])obj).Length;
                return (byte[])obj;
            });
            RegisterExporter(typeof(string), delegate (object obj, out int length, string coding)
            {
                var bytes = Encoding.GetEncoding(coding).GetBytes(obj.ToString());
                length = bytes.Length;
                return bytes;
            });
            RegisterExporter(typeof(int), delegate (object obj, out int length, string coding)
            {
                length = sizeof(int);
                return BitConverter.GetBytes((int)obj);
            });
            RegisterExporter(typeof(long), delegate (object obj, out int length, string coding)
            {
                length = sizeof(long);
                return BitConverter.GetBytes((long)obj);
            });
            RegisterExporter(typeof(bool), delegate (object obj, out int length, string coding)
            {
                length = sizeof(bool);
                return BitConverter.GetBytes((bool)obj);
            });
            RegisterExporter(typeof(double), delegate (object obj, out int length, string coding)
            {
                length = sizeof(double);
                return BitConverter.GetBytes((double)obj);
            });
            RegisterExporter(typeof(float), delegate (object obj, out int length, string coding)
            {
                length = sizeof(float);
                return BitConverter.GetBytes((float)obj);
            });
            RegisterExporter(typeof(short), delegate (object obj, out int length, string coding)
            {
                length = sizeof(short);
                return BitConverter.GetBytes((short)obj);
            });
            RegisterExporter(typeof(ulong), delegate (object obj, out int length, string coding)
            {
                length = sizeof(ulong);
                return BitConverter.GetBytes((ulong)obj);
            });
        }
        private void registerImporters()
        {
            RegisterImporter(typeof(byte[]), delegate (byte[] bytes, string coding)
            {
                return (bytes);
            });
            RegisterImporter(typeof(string), delegate (byte[] bytes, string coding)
            {
                return Encoding.GetEncoding(coding).GetString(bytes);
            });
            RegisterImporter(typeof(int), delegate (byte[] bytes, string coding)
            {
                return BitConverter.ToInt32(bytes, 0);
            });
            RegisterImporter(typeof(long), delegate (byte[] bytes, string coding)
            {
                return BitConverter.ToInt64(bytes, 0);
            });
            RegisterImporter(typeof(bool), delegate (byte[] bytes, string coding)
            {
                return BitConverter.ToBoolean(bytes, 0);
            });
            RegisterImporter(typeof(short), delegate (byte[] bytes, string coding)
            {
                return BitConverter.ToInt16(bytes, 0);
            });
            RegisterImporter(typeof(float), delegate (byte[] bytes, string coding)
            {
                return BitConverter.ToSingle(bytes, 0);
            });
            RegisterImporter(typeof(double), delegate (byte[] bytes, string coding)
            {
                return BitConverter.ToDouble(bytes, 0);
            });
            RegisterImporter(typeof(ulong), delegate (byte[] bytes, string coding)
            {
                return BitConverter.ToUInt64(bytes, 0);
            });
        }
        public void RegisterImporter(Type type, ImporterFunc func)
        {
            if (base_importers_table.ContainsKey(type))
                base_importers_table.Remove(type);
            base_importers_table.Add(type, func);
        }
        public void RegisterExporter(Type type, ExporterFunc func)
        {
            if (base_exporters_table.ContainsKey(type))
                base_exporters_table.Remove(type);
            base_exporters_table.Add(type, func);
        }
        public object ToObjact(Type type, byte[] bytes, string coding = DefaultEncoding)
        {
            if (base_importers_table.ContainsKey(type))
                return base_importers_table[type](bytes, coding);
            else if (type.IsArray)
            {
                var count = BitConverter.ToInt32(bytes, 0);
                var seek = (count + 1) * 4;
                Array list = Array.CreateInstance(type.GetElementType(), count);
                for (int i = 1; i <= count; i++)
                {
                    var packlen = BitConverter.ToInt32(bytes, 4 * i);
                    list.SetValue(ToObjact(type.GetElementType(), Tools.SubBytes(bytes, packlen, seek)), i - 1);
                    seek += packlen;
                }
                return list;
            }
            else
            {
                return null;
            }
        }
        public byte[] ToBytes(Type type, object obj, out int length, string coding = DefaultEncoding)
        {
            if (base_exporters_table.ContainsKey(type))
                return base_exporters_table[type](obj, out length, coding);
            else if (type.IsArray)
            {
                var count = ((Array)obj).Length;
                length = 0;
                List<byte> bytes = new List<byte>();
                List<byte> content = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes(count));
                length += (count + 1) * 4;
                foreach (var item in (Array)obj)
                {
                    var tlength = 0;
                    byte[] sbytes;
                    sbytes = ToBytes(item.GetType(), item, out tlength);
                    bytes.AddRange(BitConverter.GetBytes(tlength));
                    content.AddRange(sbytes);
                    length += tlength;
                }
                bytes.AddRange(content);
                return bytes.ToArray();
            }
            else
            {
                length = -1;
                return null;
            }
        }
    }
}
