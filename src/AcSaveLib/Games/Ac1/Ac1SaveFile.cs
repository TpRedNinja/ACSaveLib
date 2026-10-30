using System;
using System.Collections.Generic;
using System.IO;
using AcSaveLib.Names;

namespace AcSaveLib.Games.Ac1
{
    /// <summary>One step of a field path: the CRC32 of a property name and an array index (65535 = not indexed).</summary>
    public readonly record struct PathStep(uint NameHash, ushort Index)
    {
        public const ushort NoIndex = 0xFFFF;
        public string Name => Ac1Names.Property(NameHash);
        public override string ToString() => Index == NoIndex ? Name : $"{Name}[{Index}]";
    }

    /// <summary>One leaf value of an object. The path leads from the saved object down to this value.</summary>
    public sealed class Ac1Field
    {
        public bool IsArray { get; set; }
        public List<PathStep> Path { get; set; } = new();
        public uint TypeWord { get; set; }
        public uint EnumType { get; set; }
        /// <summary>Raw bytes of the value as stored (big-endian, including the u32 length for arrays).</summary>
        public byte[] Raw { get; set; } = Array.Empty<byte>();

        /// <summary>Field type = (typeWord >> 16) &amp; 0x3F. 0 bool, 3 byte, 6/7 int32, 11 two floats, 25 enum, 29 array.</summary>
        public int Type => (int)((TypeWord >> 16) & 0x3F);
        /// <summary>For array types (29, 24, 23): element type = (typeWord >> 23) &amp; 0x3F.</summary>
        public int ElementType => (int)((TypeWord >> 23) & 0x3F);
        public string PathText => string.Join(".", Path);
        public string Name => Path.Count == 0 ? "" : Path[Path.Count - 1].Name;

        public bool IsScalar => Type is >= 0 and <= 11 or 17 or 18 or 20 or 21 or 25 or 26 or 27 or 28;
        public ulong AsUInt64() { ulong v = 0; foreach (var b in Raw) v = (v << 8) | b; return v; }
        public uint AsUInt32() => (uint)AsUInt64();
        public int AsInt32() => unchecked((int)AsUInt32());
        public bool AsBool() => Raw.Length > 0 && Raw[0] != 0;
        public float AsFloat() => BitConverter.Int32BitsToSingle(AsInt32());

        /// <summary>Elements of an array field (count = element count, not byte count).</summary>
        public List<ulong> Elements()
        {
            var r = new List<ulong>();
            if (!IsArray || Raw.Length < 4) return r;
            int sz = Ac1SaveFile.TypeSize(ElementType);
            int n = (Raw[0] << 24) | (Raw[1] << 16) | (Raw[2] << 8) | Raw[3];
            for (int i = 0; i < n; i++)
            {
                ulong v = 0;
                for (int k = 0; k < sz; k++) v = (v << 8) | Raw[4 + i * sz + k];
                r.Add(v);
            }
            return r;
        }
    }

    /// <summary>One saved object: a manager section (2092, 2093, 3003, 3004, 3008, 3009, 3010) or a mission item (hash ID).</summary>
    public sealed class Ac1Object
    {
        public uint ClassId { get; set; }
        public uint ValueAreaSize { get; set; }
        public List<Ac1Field> Fields { get; set; } = new();
        public string Name => Ac1Names.Class(ClassId);

        public Ac1Field? Find(string pathText) { foreach (var f in Fields) if (f.PathText == pathText) return f; return null; }
        public IEnumerable<Ac1Field> FindByName(string name) { foreach (var f in Fields) if (f.Name == name) yield return f; }
    }

    /// <summary>
    /// Assassin's Creed (2007, PC) .sav. Big-endian, no encryption, no compression. Layout: docs/FORMAT_AC1.md.
    /// Parse and Write round-trip byte-exact (verified on the owner's saves).
    /// </summary>
    public sealed class Ac1SaveFile
    {
        public const uint Magic = 0x1234FEDC;
        public ushort Version { get; set; }
        public List<Ac1Object> Objects { get; set; } = new();

        // byte size of each field type (engine table 0x192F378; AC1_notes.md 10.1)
        private static readonly Dictionary<int, int> Sizes = new()
        {
            {0,1},{1,1},{2,1},{3,1},{4,2},{5,2},{6,4},{7,4},{8,8},{9,8},{10,4},{11,8},{12,12},{13,16},{14,16},{15,36},{16,64},
            {17,4},{18,4},{20,4},{21,4},{25,4},{26,4},{27,4},{28,4},{30,257},{31,257},{32,514},{33,1028},{34,2056},{35,1028},{36,4100},{37,1040},{38,1040},
        };
        public static int TypeSize(int type) => Sizes.TryGetValue(type, out var s) ? s : throw new InvalidDataException($"Unknown AC1 field type {type}.");

        public static Ac1SaveFile Load(string path) => Parse(File.ReadAllBytes(path));

        public static Ac1SaveFile Parse(byte[] b)
        {
            int p = 0;
            ulong U(int n)
            {
                if (p + n > b.Length) throw new InvalidDataException("Unexpected end of AC1 save.");
                ulong v = 0; for (int i = 0; i < n; i++) v = (v << 8) | b[p + i]; p += n; return v;
            }
            uint size = (uint)U(4);
            if (size != b.Length) throw new InvalidDataException($"File size field {size} does not match length {b.Length}.");
            if ((uint)U(4) != Magic) throw new InvalidDataException("Not an AC1 save (magic mismatch).");
            var save = new Ac1SaveFile { Version = (ushort)U(2) };
            uint count = (uint)U(4);
            for (uint o = 0; o < count; o++)
            {
                var obj = new Ac1Object { ClassId = (uint)U(4) };
                uint fc = (uint)U(4);
                for (uint i = 0; i < fc; i++)
                {
                    var f = new Ac1Field { IsArray = U(1) != 0 };
                    uint pc = (uint)U(4);
                    for (uint k = 0; k < pc; k++) f.Path.Add(new PathStep((uint)U(4), (ushort)U(2)));
                    obj.Fields.Add(f);
                }
                uint mc = (uint)U(4);
                if (mc != fc) throw new InvalidDataException($"Metadata count {mc} differs from field count {fc} in object {obj.ClassId}.");
                for (int i = 0; i < mc; i++) { obj.Fields[i].TypeWord = (uint)U(4); obj.Fields[i].EnumType = (uint)U(4); }
                obj.ValueAreaSize = (uint)U(4);
                foreach (var f in obj.Fields)
                {
                    int start = p; int t = f.Type;
                    if (t == 29 || t == 24)
                    {
                        int n = (int)U(4);
                        p += n * TypeSize(f.ElementType);
                    }
                    else if (t == 23) p += (int)(f.TypeWord & 0x7FFF) * TypeSize(f.ElementType);
                    else p += TypeSize(t);
                    if (p > b.Length) throw new InvalidDataException("Value runs past the end of the file.");
                    f.Raw = new byte[p - start];
                    Array.Copy(b, start, f.Raw, 0, p - start);
                }
                save.Objects.Add(obj);
            }
            if (p != b.Length) throw new InvalidDataException($"Parsed {p} bytes but file is {b.Length}.");
            return save;
        }

        public byte[] Write()
        {
            using var m = new MemoryStream();
            void U(ulong v, int n) { for (int i = n - 1; i >= 0; i--) m.WriteByte((byte)(v >> (8 * i))); }
            U(0, 4); U(Magic, 4); U(Version, 2); U((uint)Objects.Count, 4);
            foreach (var o in Objects)
            {
                U(o.ClassId, 4); U((uint)o.Fields.Count, 4);
                foreach (var f in o.Fields)
                {
                    U(f.IsArray ? 1u : 0u, 1); U((uint)f.Path.Count, 4);
                    foreach (var s in f.Path) { U(s.NameHash, 4); U(s.Index, 2); }
                }
                U((uint)o.Fields.Count, 4);
                foreach (var f in o.Fields) { U(f.TypeWord, 4); U(f.EnumType, 4); }
                U(o.ValueAreaSize, 4);
                foreach (var f in o.Fields) m.Write(f.Raw, 0, f.Raw.Length);
            }
            var r = m.ToArray();
            r[0] = (byte)(r.Length >> 24); r[1] = (byte)(r.Length >> 16); r[2] = (byte)(r.Length >> 8); r[3] = (byte)r.Length;
            return r;
        }

        public Ac1Object? Find(uint classId) { foreach (var o in Objects) if (o.ClassId == classId) return o; return null; }
    }

    public static class Ac1Names
    {
        private static readonly Dictionary<uint, string> Props = LoadTsv("AcSaveLib.ac1_props.tsv");
        private static readonly Dictionary<uint, string> Classes = LoadTsv("AcSaveLib.ac1_classes.tsv");

        private static Dictionary<uint, string> LoadTsv(string resource)
        {
            var map = new Dictionary<uint, string>();
            using var s = typeof(Ac1Names).Assembly.GetManifestResourceStream(resource);
            if (s == null) return map;
            using var r = new StreamReader(s);
            string? line;
            while ((line = r.ReadLine()) != null)
            {
                if (line.Length == 0 || line[0] == '#') continue;
                var parts = line.Split('\t');
                if (parts.Length >= 2 && uint.TryParse(parts[0], out var h)) map[h] = parts[1];
            }
            return map;
        }

        public static string Property(uint hash) => Props.TryGetValue(hash, out var n) ? n : hash.ToString("x8");
        public static bool IsKnownProperty(uint hash) => Props.ContainsKey(hash);
        public static string Class(uint id) => Classes.TryGetValue(id, out var n) ? n : id.ToString("X8");
        public static bool IsKnownClass(uint id) => Classes.ContainsKey(id);
        public static void AddClass(uint id, string name) => Classes[id] = name;
        /// <summary>Property hash for a name known in the table (the table, not CRC32, is authoritative for a few names such as MissionStatus).</summary>
        public static uint? PropertyHash(string name) { foreach (var kv in Props) if (kv.Value == name) return kv.Key; return null; }
    }
}
