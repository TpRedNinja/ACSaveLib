using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace AcSaveLib.Names
{
    /// <summary>
    /// Reads a game localization file (LocalizationPackage_English.xml, exported with AnvilToolkit): &lt;String ID="n"&gt;text&lt;/String&gt;.
    /// The IDs are the OasisLineIDs stored in saves (trainee names and so on). The file is game text and is loaded at runtime, not embedded.
    /// The file declares UTF-16 but holds UTF-8 in the samples, so the text is decoded here and the declaration is ignored.
    /// </summary>
    public sealed class LocalizationPackage
    {
        private readonly Dictionary<uint, string> _strings = new();

        public int Count => _strings.Count;

        public static LocalizationPackage Load(string path)
        {
            var bytes = File.ReadAllBytes(path);
            Encoding enc = bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE ? Encoding.Unicode
                : bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF ? Encoding.BigEndianUnicode : new UTF8Encoding(false);
            var pkg = new LocalizationPackage();
            using var reader = XmlReader.Create(new StringReader(enc.GetString(bytes)));
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
