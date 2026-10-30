using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AcSaveLib.Names
{
    /// <summary>Property keys in the stream are CRC32 (zlib) of the property name. This maps keys back to names.</summary>
    public static class PropertyNames
    {
        private static readonly Dictionary<uint, string> Map = Load();
        private static uint[]? _table;

        public static uint Crc32(string s)
        {
            _table ??= BuildTable();
            uint c = 0xFFFFFFFF;
            foreach (byte b in System.Text.Encoding.ASCII.GetBytes(s)) c = _table[(c ^ b) & 0xFF] ^ (c >> 8);
            return ~c;
        }

        public static string Lookup(uint key) => Map.TryGetValue(key, out var n) ? n : key.ToString("x8");
        public static bool IsKnown(uint key) => Map.ContainsKey(key);
        public static void Add(string name) => Map[Crc32(name)] = name;
        public static int Count => Map.Count;

        private static uint[] BuildTable()
        {
            var t = new uint[256];
            for (uint i = 0; i < 256; i++) { uint c = i; for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1; t[i] = c; }
            return t;
        }

        private static Dictionary<uint, string> Load()
        {
            var map = new Dictionary<uint, string>();
            using var s = typeof(PropertyNames).Assembly.GetManifestResourceStream("AcSaveLib.names.txt");
            if (s == null) return map;
            using var r = new StreamReader(s);
            string? line;
            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                map[Crc32(line)] = line;
            }
            return map;
        }
    }
}
