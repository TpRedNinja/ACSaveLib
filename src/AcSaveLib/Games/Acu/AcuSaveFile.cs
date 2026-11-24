using System;
using System.Collections.Generic;
using System.IO;
using AcSaveLib.Container;
using AcSaveLib.Crypto;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acu
{
    /// <summary>
    /// Assassin's Creed Unity (.save, PC). Layout is documented in docs/FORMAT_ACU.md.
    /// File -> XOR tail cipher (from byte 552, 16 byte key anchored to the end of the file) -> chunked LZO containers -> CRC32-keyed property stream.
    /// The key is recovered from known plaintext (version 42, magic 0x00FEDBAC and the first container signature); it is never fixed in code. Read-only.
    /// </summary>
    public sealed class AcuSaveFile
    {
        public const int EncryptedStart = 552;
        /// <summary>The UTF-16 title starts at 0x28; the 32 bytes before it hold an ASCII id (zeros in some saves).</summary>
        public const int TitleOffset = 0x28;
        /// <summary>The first container signature starts 20 bytes after the encrypted start: version, magic and three u32 values come before it.</summary>
        public const int ContainersStart = EncryptedStart + 20;
        /// <summary>Property items start at offset 26 of a decoded container: 14 zero bytes, root key, two u32 sizes and a flag byte.</summary>
        public const int ItemsStart = 26;
        public const byte FormatVersion = 42;

        // Known plaintext: version and magic at the start, then the container signature and the chunk count (1) and chunk flag layout after it.
        private static readonly (int Offset, byte[] Plaintext)[] KnownPieces =
        {
            (EncryptedStart, new byte[] { FormatVersion, 0, 0, 0, 0xAC, 0xDB, 0xFE, 0 }),
            (ContainersStart, new byte[] { 0x33, 0xAA, 0xFB, 0x57, 0x99, 0xFA, 0x04, 0x10, 0x01, 0, 0, 0 }),
        };

        /// <summary>UTF-16 save title from the plaintext header (e.g. "AC4BF[0]-tm_2025_10_25_10_45_44").</summary>
        public string Title { get; private set; } = "";
        /// <summary>The 16 byte XOR key recovered from this file.</summary>
        public byte[] Key { get; private set; } = Array.Empty<byte>();
        /// <summary>Decrypted but still compressed file bytes.</summary>
        public byte[] Decrypted { get; private set; } = Array.Empty<byte>();
        /// <summary>Decompressed containers in file order: header, (slot 3 saves: global profile container), last container.</summary>
        public List<byte[]> Containers { get; private set; } = new();
        public byte[] HeaderStream => Containers[0];
        /// <summary>The last container: game data of a game save, SinglePlayerOptionsSaveData of a slot 3 (options) save.</summary>
        public byte[] Stream => Containers[Containers.Count - 1];
        public List<Node> HeaderProperties { get; private set; } = new();
        public List<Node> Root { get; private set; } = new();
        /// <summary>Items of the middle container of a slot 3 (options) save, empty for game saves.</summary>
        public List<Node> ProfileRoot { get; private set; } = new();

        public AcuHeader Header { get; private set; } = new();
        /// <summary>Typed game data of a game save (empty for an options save).</summary>
        public AcuGameData Game { get; private set; } = new();
        /// <summary>Typed options and profile data of a slot 3 (options) save (empty for a game save).</summary>
        public AcuOptionsData Options { get; private set; } = new();
        /// <summary>True when the header marks the file as an options save (slot 3).</summary>
        public bool IsOptions => Header.IsOptions;

        public static AcuSaveFile Load(string path) => Parse(File.ReadAllBytes(path));

        public static AcuSaveFile Parse(byte[] file)
        {
            if (file.Length < ContainersStart + 24) throw new InvalidDataException("File too small to be a Unity save.");
            var r = new AcuSaveFile();
            r.Title = System.Text.Encoding.Unicode.GetString(file, TitleOffset, 0x200 - TitleOffset).Trim().TrimEnd((char)0).Trim();
            try { r.Key = XorTailCipher.RecoverKey(file, KnownPieces, 16); }
            catch (ArgumentException e) { throw new InvalidDataException("Cannot recover the key: " + e.Message); }
            r.Decrypted = XorTailCipher.Apply(file, EncryptedStart, r.Key);
            if (r.Decrypted[EncryptedStart] != FormatVersion || BitConverter.ToUInt32(r.Decrypted, EncryptedStart + 4) != 0x00FEDBAC)
                throw new InvalidDataException("Not a Unity save: the stream does not start with version 42 and the magic 0x00FEDBAC.");
            r.Containers = ChunkContainer.ReadAll(r.Decrypted, ContainersStart);
            if (r.Containers.Count < 2) throw new InvalidDataException("Expected a header container and a main container.");
            r.HeaderProperties = PropertyReader.Parse(r.HeaderStream, ItemsStart, r.HeaderStream.Length);
            r.Root = PropertyReader.Parse(r.Stream, ItemsStart, r.Stream.Length);
            if (r.Containers.Count > 2) r.ProfileRoot = PropertyReader.Parse(r.Containers[1], ItemsStart, r.Containers[1].Length);
            r.Header = AcuHeader.From(r.HeaderProperties);
            if (r.IsOptions) r.Options = AcuOptionsData.From(r.ProfileRoot, r.Root);
            else r.Game = AcuGameData.From(r.Root);
            return r;
        }
    }
}
