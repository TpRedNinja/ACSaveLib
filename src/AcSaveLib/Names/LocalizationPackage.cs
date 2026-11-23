using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace AcSaveLib.Names
{
    /// <summary>
    /// Text for OasisLineIDs (the line ids stored in saves for trainee names and so on).
    /// <see cref="Builtin"/> holds only the 64 Brotherhood trainee names (embedded resource trainee_names.tsv), so no game file is needed.
    /// <see cref="Load"/> reads a full LocalizationPackage_English.xml (exported with AnvilToolkit, &lt;String ID="n"&gt;text&lt;/String&gt;) when the caller has one.
    /// The file declares UTF-16; the text is decoded here from its byte order mark and the declaration is ignored.
    /// </summary>
    public sealed class LocalizationPackage
    {
        private readonly Dictionary<uint, string> _strings = new();

        public int Count => _strings.Count;

        private static LocalizationPackage? _builtin;

        /// <summary>The embedded trainee names (id, tab, name per line).</summary>
        public static LocalizationPackage Builtin => _builtin ??= LoadBuiltin();

        private static LocalizationPackage LoadBuiltin()
        {
            var pkg = new LocalizationPackage();
            using var stream = typeof(LocalizationPackage).Assembly.GetManifestResourceStream("AcSaveLib.trainee_names.tsv");
            if (stream == null) return pkg;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                int tab = line.IndexOf('\t');
                if (tab > 0 && uint.TryParse(line.Substring(0, tab), out uint id)) pkg._strings[id] = line.Substring(tab + 1);
            }
            return pkg;
        }

        public static LocalizationPackage Load(string path)
        {
            var bytes = File.ReadAllBytes(path);
            Encoding enc = bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE ? Encoding.Unicode
                : bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF ? Encoding.BigEndianUnicode : new UTF8Encoding(false);
            var pkg = new LocalizationPackage();
            using var reader = XmlReader.Create(new StringReader(enc.GetString(bytes).TrimStart('\uFEFF')));
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element || reader.Name != "String") continue;
                if (!uint.TryParse(reader.GetAttribute("ID"), out uint id)) continue;
                pkg._strings[id] = reader.ReadElementContentAsString();
            }
            return pkg;
        }

        /// <summary>Text for an OasisLineID, or null (0xFFFFFFFF in a save means no line).</summary>
        public string? Lookup(uint id) => _strings.TryGetValue(id, out var s) ? s : null;

        public string Describe(uint id) => Lookup(id) ?? $"#{id}";
    }
}
