using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AcSaveLib.Container;
using AcSaveLib.Games.Acb;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acr
{
    /// <summary>One decoded container after the header container: its root key, its items and, if it holds game data, the typed models.</summary>
    public sealed class AcrStream
    {
        public uint RootKey { get; init; }
        public int Length { get; init; }
        public List<Node> Items { get; init; } = new();
        /// <summary>Typed models of the named fields. <see cref="AcbGameData.HasGameData"/> is false for streams without game data.</summary>
        public AcbGameData Game { get; init; } = new();
        /// <summary>PlayerOptionsSaveData (options saves), else null.</summary>
        public AcbOptions? Options { get; init; }
    }

    /// <summary>
    /// Assassin's Creed Revelations (.save, PC). Layout is documented in docs/FORMAT_ACR.md.
    /// AC2-style header (0x228 bytes, plain), then the Brotherhood container stream. Game saves are enciphered from 0x228 with a
    /// repeating 12 byte XOR key aligned to the end of the file; the options save is plain. Read-only.
    /// </summary>
    public sealed class AcrSaveFile
    {
        /// <summary>Offset where the container stream (and the cipher) starts.</summary>
        public const int StreamStart = 0x228;
        public const uint FileMagic = 0x00FEDBAC;
        /// <summary>XOR key of the game saves (12 ASCII bytes).</summary>
        public static readonly byte[] Key = Encoding.ASCII.GetBytes("amsirrahrats");

        public uint HeaderLength { get; private set; }
        /// <summary>32 character ASCII id at 0x08 (meaning not known).</summary>
        public string HeaderId { get; private set; } = "";
        /// <summary>UTF-16 title at 0x28, for example "AC2[0]-tm_2025_03_26 ...".</summary>
        public string Title { get; private set; } = "";
        public bool WasEncrypted { get; private set; }
        /// <summary>The file with the stream decrypted (equal to the input for plain saves).</summary>
        public byte[] Data { get; private set; } = Array.Empty<byte>();
        public List<byte[]> Containers { get; private set; } = new();
        public List<Node> HeaderProperties { get; private set; } = new();
        public AcbHeader Header { get; private set; } = new();
        /// <summary>Containers after the header container, in file order.</summary>
        public List<AcrStream> Streams { get; private set; } = new();

        /// <summary>The first stream with game data (the main game), or null.</summary>
        public AcrStream? MainGame => Streams.FirstOrDefault(s => s.Game.HasGameData);
        public AcbOptions? Options => Streams.Select(s => s.Options).FirstOrDefault(o => o != null);

        public static AcrSaveFile Load(string path) => Parse(File.ReadAllBytes(path));

        public static AcrSaveFile Parse(byte[] file)
        {
            if (file.Length < StreamStart + 16) throw new InvalidDataException("File too small to be a Revelations save.");
            var r = new AcrSaveFile { HeaderLength = BitConverter.ToUInt32(file, 0) };
            r.HeaderId = Encoding.ASCII.GetString(file, 8, 32);
            r.Title = ReadTitle(file);
            r.WasEncrypted = BitConverter.ToUInt32(file, StreamStart + 4) != FileMagic;
            r.Data = r.WasEncrypted ? Decrypt(file) : file;
            if (BitConverter.ToUInt32(r.Data, StreamStart + 4) != FileMagic)
                throw new InvalidDataException("Stream magic 0x00FEDBAC not found after decrypting.");
            r.Containers = ChunkContainer.ReadAll(r.Data, StreamStart);
            if (r.Containers.Count < 2) throw new InvalidDataException("Expected at least two containers.");
            r.HeaderProperties = ParseContainer(r.Containers[0]);
            r.Header = AcbHeader.From(r.HeaderProperties);
            for (int i = 1; i < r.Containers.Count; i++)
            {
                var c = r.Containers[i];
                var items = ParseContainer(c);
                var opt = new Props(items).Get("PlayerOptionsSaveData");
                r.Streams.Add(new AcrStream
                {
                    RootKey = BitConverter.ToUInt32(c, 10), Length = c.Length, Items = items,
                    Game = AcbGameData.From(items), Options = opt != null ? AcbOptions.From(opt) : null,
                });
            }
            return r;
        }

        /// <summary>
        /// Returns a copy with the stream (from 0x228 to the end of the file) decrypted: byte i is XORed with Key[(i - fileLength + 1) mod 12],
        /// so the last byte of the file uses Key[0]. CONFIRMED: every block of both game saves decodes after this.
        /// </summary>
        public static byte[] Decrypt(byte[] file)
        {
            var o = (byte[])file.Clone();
            for (int i = StreamStart; i < o.Length; i++)
            {
                int k = ((i - o.Length + 1) % Key.Length + Key.Length) % Key.Length;
                o[i] ^= Key[k];
            }
            return o;
        }

        private static List<Node> ParseContainer(byte[] c)
        {
            if (c.Length < 22) throw new InvalidDataException("Container too short.");
            return PropertyReader.Parse(c, 22, c.Length, true);
        }

        private static string ReadTitle(byte[] f)
        {
            var sb = new StringBuilder();
            for (int i = 0x28; i + 1 < StreamStart; i += 2)
            {
                char c = (char)(f[i] | f[i + 1] << 8);
                if (c == 0) break;
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
