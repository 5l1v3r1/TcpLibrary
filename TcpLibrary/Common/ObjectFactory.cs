using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;

namespace TcpLibrary.Common
{
    public static class ObjectFactory
    {
        public const string DefaultEncoding = "UTF-8";
        public delegate byte[] ExporterFunc(object obj, string coding = DefaultEncoding);
        public delegate object ImporterFunc(byte[] input, string coding = DefaultEncoding);
        public delegate bool ExcludeFunc(FieldInfo fi);

        private static bool IsInited = false;

        private static Dictionary<Type, ExporterFunc> base_exporters_table;

        private static Dictionary<Type, ImporterFunc> base_importers_table;

        private static Dictionary<Type, FieldInfo[]> base_fieldinfo_table;

        private static List<ExcludeFunc> base_excludes_list;


        public static void Init(bool must = false)
        {
            if (IsInited && !must) return;
            base_exporters_table = new Dictionary<Type, ExporterFunc>();
            base_importers_table = new Dictionary<Type, ImporterFunc>();
            base_fieldinfo_table = new Dictionary<Type, FieldInfo[]>();
            base_excludes_list = new List<ExcludeFunc>();
            registerExporters();
            registerImporters();
            registerExcludes();
        }

        private static void registerExcludes()
        {
            RegisterExclude(delegate (FieldInfo fi)
            {
                if (fi.Name.StartsWith("_"))
                    return true;
                else return false;
            });
        }

        public static void RegisterExclude(ExcludeFunc func)
        {
            if (base_excludes_list.Contains(func))
                return;
            base_excludes_list.Add(func);
        }

        private static bool isExclude(FieldInfo fi)
        {
            foreach (var item in base_excludes_list)
            {
                if (item(fi))
                    return true;
            }
            return false;
        }
        private static void registerExporters()
        {
            RegisterExporter(typeof(byte[]), delegate (object obj, string coding)
            {
                return (byte[])obj;
            });
            RegisterExporter(typeof(string), delegate (object obj, string coding)
            {
                return Encoding.GetEncoding(coding).GetBytes(obj.ToString());
            });
            RegisterExporter(typeof(int), delegate (object obj, string coding)
            {
                return BitConverter.GetBytes((int)obj);
            });
            RegisterExporter(typeof(long), delegate (object obj, string coding)
            {
                return BitConverter.GetBytes((long)obj);
            });
            RegisterExporter(typeof(bool), delegate (object obj, string coding)
            {
                return BitConverter.GetBytes((bool)obj);
            });
            RegisterExporter(typeof(double), delegate (object obj, string coding)
            {
                return BitConverter.GetBytes((double)obj);
            });
            RegisterExporter(typeof(float), delegate (object obj, string coding)
            {
                return BitConverter.GetBytes((float)obj);
            });
            RegisterExporter(typeof(short), delegate (object obj, string coding)
            {
                return BitConverter.GetBytes((short)obj);
            });
            RegisterExporter(typeof(ulong), delegate (object obj, string coding)
            {
                return BitConverter.GetBytes((ulong)obj);
            });
            RegisterExporter(typeof(Version), delegate (object obj, string coding)
            {
                byte[] bytes = new byte[16];
                Version ver = (Version)obj;
                Buffer.BlockCopy(new int[] { ver.Major, ver.Minor, ver.Build, ver.Revision }, 0, bytes, 0, 16);
                return bytes;
            });
            RegisterExporter(new Type[] { typeof(Size), typeof(Point) }, delegate (object obj, string coding)
            {
                byte[] bytes = new byte[8];
                Point point = (Point)obj;
                Buffer.BlockCopy(new int[] { point.X, point.Y }, 0, bytes, 0, 8);
                return bytes;
            });
            RegisterExporter(typeof(Color), delegate (object obj, string coding)
            {
                var rgba = ((Color)obj).ToArgb();
                return BitConverter.GetBytes(rgba);
            });
            RegisterExporter(typeof(TimeSpan), delegate (object obj, string coding)
            {
                byte[] bytes = new byte[8];
                TimeSpan timespan = (TimeSpan)obj;
                Buffer.BlockCopy(new long[] { timespan.Ticks }, 0, bytes, 0, 8);
                return bytes;
            });
        }
        private static void registerImporters()
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
            RegisterImporter(typeof(Version), delegate (byte[] bytes, string coding)
            {
                int[] parts = new int[4];
                Buffer.BlockCopy(bytes, 0, parts, 0, 16);
                return new Version(parts[0], parts[1], parts[2], parts[3]);
            });
            RegisterImporter(new Type[] { typeof(Size), typeof(Point) }, delegate (byte[] bytes, string coding)
            {
                int[] parts = new int[2];
                Buffer.BlockCopy(bytes, 0, parts, 0, 8);
                return new Point(parts[0], parts[1]);
            });
            RegisterImporter(typeof(Color), delegate (byte[] bytes, string coding)
            {
                var rgba = BitConverter.ToInt32(bytes, 0);
                return Color.FromArgb(rgba);
            });
            RegisterImporter(typeof(TimeSpan), delegate (byte[] bytes, string coding)
            {
                long[] parts = new long[1];
                Buffer.BlockCopy(bytes, 0, parts, 0, 8);
                return new TimeSpan(parts[0]);
            });
        }
        public static void RegisterImporter(Type[] types, ImporterFunc func)
        {
            foreach (var item in types)
                RegisterImporter(item, func);
        }
        public static void RegisterImporter(Type type, ImporterFunc func)
        {
            if (base_importers_table.ContainsKey(type))
                base_importers_table.Remove(type);
            base_importers_table.Add(type, func);
        }
        public static void RegisterExporter(Type[] types, ExporterFunc func)
        {
            foreach (var item in types)
                RegisterExporter(item, func);
        }
        public static void RegisterExporter(Type type, ExporterFunc func)
        {
            if (base_exporters_table.ContainsKey(type))
                base_exporters_table.Remove(type);
            base_exporters_table.Add(type, func);
        }
        public static object ToObjact(Type type, byte[] bytes, string coding = DefaultEncoding)
        {
            if (base_importers_table.ContainsKey(type))
                return base_importers_table[type](bytes, coding);
            else if (type.IsEnum)
                return base_importers_table[typeof(int)](bytes, coding);
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
            else if (type.IsClass)
            {
                Dictionary<FieldInfo, int> sizeList = new Dictionary<FieldInfo, int>();
                var cons = Activator.CreateInstance(type);
                FieldInfo[] tfis = getFields(type);
                var count = 0;
                List<FieldInfo> fis = new List<FieldInfo>();
                foreach (var item in tfis)
                {
                    if (isExclude(item)) continue;
                    fis.Add(item);
                    count++;
                }
                int seek = count * 4;
                for (int i = 0; i < count; i++)
                {
                    var fi = fis[i];
                    var packlen = BitConverter.ToInt32(bytes, 4 * i);
                    if (fi.FieldType == typeof(int) || fi.FieldType.IsEnum)
                        fi.SetValue(cons, packlen);
                    else
                    {
                        fi.SetValue(cons, ToObjact(fi.FieldType, Tools.SubBytes(bytes, packlen, seek)));
                        seek += packlen;
                    }
                }
                return cons;
            }
            else
            {
                return null;
            }
        }
        public static byte[] ToBytes(Type type, object obj, string coding = DefaultEncoding)
        {
            if (base_exporters_table.ContainsKey(type))
                return base_exporters_table[type](obj, coding);
            else if (type.IsEnum)
                return base_exporters_table[typeof(int)](obj, coding);
            else if (type.IsArray)
            {
                var count = ((Array)obj).Length;
                List<byte> bytes = new List<byte>();
                List<byte> content = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes(count));
                foreach (var item in (Array)obj)
                {
                    byte[] sbytes;
                    sbytes = ToBytes(item.GetType(), item);
                    bytes.AddRange(BitConverter.GetBytes(sbytes.Length));
                    content.AddRange(sbytes);
                }
                bytes.AddRange(content);
                return bytes.ToArray();
            }
            else if (type.IsClass)
            {
                List<byte> header = new List<byte>();
                List<byte> data = new List<byte>();
                foreach (FieldInfo fi in getFields(type))
                {
                    if (isExclude(fi)) continue;
                    var bytes = ToBytes(fi.FieldType, fi.GetValue(obj), coding);
                    if (fi.FieldType == typeof(int) || fi.FieldType.IsEnum)
                        header.AddRange(bytes);
                    else
                    {
                        header.AddRange(BitConverter.GetBytes(bytes.Length));
                        data.AddRange(bytes);
                    }
                }
                header.AddRange(data);
                return header.ToArray();
            }
            else
            {
                return null;
            }
        }
        private static FieldInfo[] getFields(Type type)
        {
            if (!base_fieldinfo_table.ContainsKey(type))
            {
                var tfis = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                base_fieldinfo_table.Add(type, tfis);
            }
            return base_fieldinfo_table[type];
        }
    }
}
