using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace AcSaveLib.Names
{
    /// <summary>
    /// Maps 64-bit entity IDs to "Name.Type" using the DataPC*.index.txt files exported by ACViewer
    /// (lines like "0x0000000669E09DC0: SQ02M10_Havana.Mission"). The files are large (~100 MB), so they are
    /// loaded from a folder at runtime instead of being embedded.
    /// </summary>
    public sealed class ForgeIndex
    {
        private readonly Dictionary<ulong, string> _map = new Dictionary<ulong, string>();

        public int Count => _map.Count;

        public static ForgeIndex LoadFolder(string folder)
        {
            var idx = new ForgeIndex();
            foreach (var file in Directory.GetFiles(folder, "*.index.txt"))
                foreach (var line in File.ReadLines(file))
                {
                    if (line.Length < 20 || line[0] != '0' || line[1] != 'x') continue;
                    int colon = line.IndexOf(':');
                    if (colon < 3 || colon + 2 > line.Length) continue;
                    if (ulong.TryParse(line.AsSpan(2, colon - 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var id))
                        idx._map[id] = line.Substring(colon + 2);
                }
            return idx;
        }

        /// <summary>Full "Name.Type" or null.</summary>
        public string? Lookup(ulong id) => _map.TryGetValue(id, out var s) ? s : null;

        public string Describe(ulong id) => Lookup(id) ?? $"0x{id:X}";
    }
}
