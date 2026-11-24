using System;
using System.IO;
using System.Text;
using AcSaveLib.Crypto;
using System.Collections.Generic;
using AcSaveLib.Container;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Ac3
{
    /// <summary>One decoded container after the header container: its root key, items and typed models.</summary>
    public sealed class Ac3Stream
    {
        public uint RootKey { get; init; }
        public int Length { get; init; }
        public List<Node> Items { get; init; } = new();
        /// <summary>Typed models of the named fields. <see cref="Ac3GameData.HasGameData"/> is false for streams without game data.</summary>
        public Ac3GameData Game { get; init; } = new();
        /// <summary>PlayerOptionsSaveData (profile saves), else null.</summary>
        public Ac3Options? Options { get; init; }
    }

    /// <summary>
    /// Assassin's Creed III (original, PC) and Assassin's Creed III Remastered saves (same layout; the Remastered title starts with "AC3RM["). Layout is documented in docs/FORMAT_AC3.md.
    /// The file holds a Liberation-style stream (u32 33, u32 0x00FEDBAC, ... LZO1x containers) XORed with a 16 byte key anchored to the end of the file.
    /// The key differs between sources (accounts or storefronts), so it is recovered from the known plaintext at the start of the stream.
    /// Header 0x204: u32 0x204, u32 0, UTF-16 title from 0x08, stream from 0x208.
    /// Header 0x224: u32 0x224, u32 0, 32 character ASCII id at 0x08, UTF-16 title from 0x28, stream from 0x228.
    /// No header (.sav): the stream starts at offset 0. Read-only.
    /// </summary>
    public sealed class Ac3SaveFile
    {
        /// <summary>Plaintext at the start of the stream: Version 33, the magic 0x00FEDBAC and 8 zero bytes (one full key period).</summary>
        private static readonly byte[] KnownPlaintext = Convert.FromHexString("21000000acdbfe000000000000000000");

        /// <summary>Header length of the file (0x204 or 0x224), 0 for the headerless variant.</summary>
        public int HeaderLength { get; private set; }
        public string Title { get; private set; } = "";
        /// <summary>True when the title marks an AC3 Remastered save ("AC3RM[slot]-tm_...").</summary>
        public bool IsRemastered => Title.StartsWith("AC3RM[", StringComparison.Ordinal);
        /// <summary>The 32 character id of header 0x224, empty otherwise.</summary>
        public string HeaderId { get; private set; } = "";
        /// <summary>The 16 byte XOR key, index = (i - fileLength) mod 16.</summary>
        public byte[] Key { get; private set; } = Array.Empty<byte>();
        public byte[] Decrypted { get; private set; } = Array.Empty<byte>();
        /// <summary>The decrypted stream (u32 33, magic, LZO1x containers), decoded.</summary>
        public byte[] StreamBytes { get; private set; } = Array.Empty<byte>();
        /// <summary>Decoded containers in file order.</summary>
        public List<byte[]> Containers { get; private set; } = new();
        /// <summary>Items of each container, same order as <see cref="Containers"/>.</summary>
        public List<List<Node>> Items { get; private set; } = new();
        public Ac3Header Header { get; private set; } = new();
        /// <summary>Containers after the header container, in file order.</summary>
        public List<Ac3Stream> Streams { get; private set; } = new();
        public Ac3Stream? MainGame => Streams.Find(x => x.Game.HasGameData);
        public Ac3Options? Options => Streams.Find(x => x.Options != null)?.Options;
        /// <summary>Property items start at offset 26 of a decoded container.</summary>
        public const int ItemsStart = 26;
        public const uint FileMagic = 0x00FEDBAC;

        public static Ac3SaveFile Load(string path, byte[]? key = null) => Parse(File.ReadAllBytes(path), key);

        /// <param name="key">Optional 16 byte key; recovered from the known plaintext when null.</param>
        public static Ac3SaveFile Parse(byte[] file, byte[]? key = null)
        {
            uint first = file.Length >= 4 ? BitConverter.ToUInt32(file, 0) : 0;
            int hdr = first == 0x204 || first == 0x224 ? (int)first : 0;
            int start = hdr == 0 ? 0 : hdr + 4;
            if (file.Length < start + 64) throw new InvalidDataException("Not an AC3 save: file too short.");
            key ??= XorTailCipher.RecoverKey(file, start, KnownPlaintext, 16);
            var dec = XorTailCipher.Apply(file, start, key);
            if (BitConverter.ToUInt32(dec, start + 4) != FileMagic)
                throw new InvalidDataException("Not an AC3 save: the stream does not start with version 33 and the magic 0x00FEDBAC (a different layout, or the key is wrong).");
            var r = new Ac3SaveFile { Decrypted = dec, HeaderLength = hdr, Key = key };
            if (hdr != 0)
            {
                int titleAt = hdr == 0x224 ? 0x28 : 8;
                if (hdr == 0x224) r.HeaderId = Encoding.ASCII.GetString(file, 8, 32);
                int end = titleAt; while (end + 1 < hdr && (dec[end] | dec[end + 1]) != 0) end += 2;
                r.Title = Encoding.Unicode.GetString(dec, titleAt, end - titleAt);
            }
            var stream = new byte[dec.Length - start];
            Array.Copy(dec, start, stream, 0, stream.Length);
            r.StreamBytes = stream;
            r.Containers = ChunkContainer.ReadAll(stream, 0);
            if (r.Containers.Count < 1) throw new InvalidDataException("No container found.");
            foreach (var c in r.Containers) r.Items.Add(PropertyReader.Parse(c, ItemsStart, c.Length, true));
            r.Header = Ac3Header.From(r.Items[0]);
            for (int i = 1; i < r.Containers.Count; i++)
            {
                var opt = new Props(r.Items[i]).Get("PlayerOptionsSaveData");
                r.Streams.Add(new Ac3Stream
                {
                    RootKey = r.Containers[i].Length >= 18 ? BitConverter.ToUInt32(r.Containers[i], 14) : 0, Length = r.Containers[i].Length, Items = r.Items[i],
                    Game = Ac3GameData.From(r.Items[i]), Options = opt != null ? Ac3Options.From(opt) : null,
                });
            }
            return r;
        }
    }
}
