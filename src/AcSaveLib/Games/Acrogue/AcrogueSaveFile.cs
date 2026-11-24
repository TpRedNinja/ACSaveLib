using System;
using System.Collections.Generic;
using System.IO;
using AcSaveLib.Container;
using AcSaveLib.Crypto;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acrogue
{
    /// <summary>
    /// Assassin's Creed Rogue (.save, PC). Layout is documented in docs/FORMAT_ACROGUE.md.
    /// File -> XOR tail cipher (from byte 552, 16 byte key anchored to the end of the file) -> chunked LZO containers -> CRC32-keyed property stream.
    /// The key is recovered from the known plaintext (version 36, magic 0x00FEDBAC, 8 zero bytes); it is never fixed in code. Read-only.
    /// </summary>
    public sealed class AcrogueSaveFile
    {
        public const int EncryptedStart = 552;
        public const int ContainersStart = 576;
        /// <summary>Property items start at offset 26 of a decoded container: 14 zero bytes, root key, two u32 sizes and a flag byte.</summary>
        public const int ItemsStart = 26;
        public const byte FormatVersion = 36;
        private static readonly byte[] KnownPlaintext = { FormatVersion, 0, 0, 0, 0xAC, 0xDB, 0xFE, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        /// <summary>UTF-16 save title from the plaintext header (e.g. "ACC[0]-tm_...").</summary>
        public string Title { get; private set; } = "";
        /// <summary>The 16 byte XOR key recovered from this file (the MD5 of the Uplay CD key string, see docs/FORMAT_ACROGUE.md).</summary>
        public byte[] Key { get; private set; } = Array.Empty<byte>();
        public byte[] Decrypted { get; private set; } = Array.Empty<byte>();
        /// <summary>Decompressed containers in file order: header, (profile saves: global profile container), main.</summary>
        public List<byte[]> Containers { get; private set; } = new();
        public byte[] HeaderStream => Containers[0];
        /// <summary>The last container: game data of a game save, SinglePlayerOptionsSaveData of a profile save.</summary>
        public byte[] Stream => Containers[Containers.Count - 1];
        public List<Node> HeaderProperties { get; private set; } = new();
        public List<Node> Root { get; private set; } = new();
        /// <summary>Items of the middle container of a profile save (PlayerOptionsSaveData, AccomplishmentManagerData), empty for game saves.</summary>
        public List<Node> ProfileRoot { get; private set; } = new();

        public RogueHeader Header { get; private set; } = new();
        public RogueGameData Game { get; private set; } = new();
        /// <summary>True when the main container holds game data (a game save), false for the profile save.</summary>
        public bool IsGameSave => Game.HasGameData;

        public static AcrogueSaveFile Load(string path) => Parse(File.ReadAllBytes(path));

        public static AcrogueSaveFile Parse(byte[] file)
        {
            if (file.Length < ContainersStart + 24) throw new InvalidDataException("File too small to be a Rogue save.");
            var r = new AcrogueSaveFile();
            r.Title = System.Text.Encoding.Unicode.GetString(file, 4, 0x200 - 4).Trim().TrimEnd((char)0).Trim();
            r.Key = XorTailCipher.RecoverKey(file, EncryptedStart, KnownPlaintext, 16);
            r.Decrypted = XorTailCipher.Apply(file, EncryptedStart, r.Key);
            if (BitConverter.ToUInt32(r.Decrypted, EncryptedStart + 4) != 0x00FEDBAC)
                throw new InvalidDataException("Not a Rogue save: the stream does not start with version 36 and the magic 0x00FEDBAC.");
            r.Containers = ChunkContainer.ReadAll(r.Decrypted, ContainersStart);
            if (r.Containers.Count < 2) throw new InvalidDataException("Expected a header container and a main container.");
            r.HeaderProperties = PropertyReader.Parse(r.HeaderStream, ItemsStart, r.HeaderStream.Length);
            r.Root = PropertyReader.Parse(r.Stream, ItemsStart, r.Stream.Length);
            if (r.Containers.Count > 2) r.ProfileRoot = PropertyReader.Parse(r.Containers[1], ItemsStart, r.Containers[1].Length);
            r.Header = RogueHeader.From(r.HeaderProperties);
            r.Game = RogueGameData.From(r.Root);
            return r;
        }
    }
}
