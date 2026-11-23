using System;
using System.Collections.Generic;
using System.IO;
using AcSaveLib.Container;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acl
{
    /// <summary>
    /// Assassin's Creed Liberation HD (.SAV and OPTIONS, PC). Layout is documented in docs/FORMAT_ACL.md.
    /// No encryption. The file holds LZO2A chunked containers (same container layout as Brotherhood). Read-only.
    /// </summary>
    public sealed class AclSaveFile
    {
        public const uint FileMagic = 0x00FEDBAC;

        public byte[] Data { get; private set; } = Array.Empty<byte>();
        /// <summary>Decompressed containers in file order.</summary>
        public List<byte[]> Containers { get; private set; } = new();

        /// <summary>Items of each container, same order as <see cref="Containers"/>.</summary>
        public List<List<Node>> Items { get; private set; } = new();

        /// <summary>Items start at offset 26 of a decoded container: 14 zero bytes, root key, two u32 sizes.</summary>
        public const int ItemsStart = 26;

        public static AclSaveFile Load(string path) => Parse(File.ReadAllBytes(path));

        public static AclSaveFile Parse(byte[] file)
        {
            if (file.Length < 64 || BitConverter.ToUInt32(file, 4) != FileMagic)
                throw new InvalidDataException("Not a Liberation save: magic 0x00FEDBAC not found at offset 4.");
            var r = new AclSaveFile { Data = file };
            r.Containers = ChunkContainer.ReadAll(file, 0);
            if (r.Containers.Count < 1) throw new InvalidDataException("No container found.");
            foreach (var c in r.Containers) r.Items.Add(PropertyReader.Parse(c, ItemsStart, c.Length, true));
            return r;
        }
    }
}
