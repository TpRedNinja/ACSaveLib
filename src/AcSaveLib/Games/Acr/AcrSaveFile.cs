using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AcSaveLib.Container;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acr
{
    /// <summary>One decoded container after the header container: its root key, its items and, if it holds game data, the typed models.</summary>
    public sealed class AcrStream
    {
        public uint RootKey { get; init; }
        public int Length { get; init; }
        public List<Node> Items { get; init; } = new();
        /// <summary>Typed models of the named fields. <see cref="AcrGameData.HasGameData"/> is false for streams without game data.</summary>
        public AcrGameData Game { get; init; } = new();
        /// <summary>PlayerOptionsSaveData (options saves), else null.</summary>
        public AcrOptions? Options { get; init; }
    }

    /// <summary>
    /// Assassin's Creed Revelations (.save, PC). Layout is documented in docs/FORMAT_ACR.md.
    /// AC2-style header (0x228 bytes, plain), then the chunked container stream (same layout as Brotherhood). Game saves are enciphered from 0x228 with a
    /// repeating 12 byte XOR key aligned to the end of the file; the options save is plain. Read-only.
    /// </summary>
    public sealed class AcrSaveFile
    {
        /// <summary>Offset where the container stream (and the cipher) starts.</summary>
        public const int StreamStart = 0x228;
        public const uint FileMagic = 0x00FEDBAC;
        /// <summary>Longest cipher period the key recovery tries (the key is the player's legacy username, 3 to 15 characters, so its length varies; no input is needed to find it).</summary>
        public const int MaxRecoveredPeriod = 16;
        /// <summary>Known plaintext at the start of the stream: u32 22, magic 0x00FEDBAC, and (at +16) the container signature, the codec bytes `01 00 02 00 80 00 00` and the first block marker `01`: 24 bytes, which fix every key column for periods up to 16.</summary>
        private static readonly (int Offset, byte Value)[] KnownPlain = BuildKnownPlain();

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
        public AcrHeader Header { get; private set; } = new();
        /// <summary>Containers after the header container, in file order.</summary>
        public List<AcrStream> Streams { get; private set; } = new();

        /// <summary>The first stream with game data (the main game), or null.</summary>
        public AcrStream? MainGame => Streams.FirstOrDefault(s => s.Game.HasGameData);
        public AcrOptions? Options => Streams.Select(s => s.Options).FirstOrDefault(o => o != null);

        /// <summary>The key that decrypted this save (stream-phase form, see <see cref="Decrypt"/>), or null for a plain save.</summary>
        public byte[]? KeyUsed { get; private set; }

        public static AcrSaveFile Load(string path, string? legacyUsername = null) => Parse(File.ReadAllBytes(path), legacyUsername);

        /// <summary>
        /// Parses a save. For an encrypted save the key is the legacy username when given (as ACViewer and ACST take it),
        /// otherwise it is recovered from the known plaintext at the start of the stream.
        /// </summary>
        public static AcrSaveFile Parse(byte[] file, string? legacyUsername = null)
        {
            if (file.Length < StreamStart + 16) throw new InvalidDataException("File too small to be a Revelations save.");
            var r = new AcrSaveFile { HeaderLength = BitConverter.ToUInt32(file, 0) };
            r.HeaderId = Encoding.ASCII.GetString(file, 8, 32);
            r.Title = ReadTitle(file);
            r.WasEncrypted = BitConverter.ToUInt32(file, StreamStart + 4) != FileMagic;
            if (r.WasEncrypted)
            {
                if (legacyUsername != null)
                {
                    r.KeyUsed = KeyFromLegacyUsername(legacyUsername, file.Length);
                    r.Data = Decrypt(file, r.KeyUsed);
                }
                else
                {
                    (r.Data, r.KeyUsed) = RecoverAndDecrypt(file);
                }
            }
            else r.Data = file;
            if (BitConverter.ToUInt32(r.Data, StreamStart + 4) != FileMagic)
                throw new InvalidDataException("Stream magic 0x00FEDBAC not found after decrypting.");
            r.Containers = ChunkContainer.ReadAll(r.Data, StreamStart);
            if (r.Containers.Count < 2) throw new InvalidDataException("Expected at least two containers.");
            r.HeaderProperties = ParseContainer(r.Containers[0]);
            r.Header = AcrHeader.From(r.HeaderProperties);
            for (int i = 1; i < r.Containers.Count; i++)
            {
                var c = r.Containers[i];
                var items = ParseContainer(c);
                var opt = new Props(items).Get("PlayerOptionsSaveData");
                r.Streams.Add(new AcrStream
                {
                    RootKey = BitConverter.ToUInt32(c, 10), Length = c.Length, Items = items,
                    Game = AcrGameData.From(items), Options = opt != null ? AcrOptions.From(opt) : null,
                });
            }
            return r;
        }

        /// <summary>
        /// Returns a copy with the stream (from 0x228 to the end of the file) XORed with a repeating key in stream-phase form:
        /// byte i uses key[(i - 0x228) mod key.Length]. CONFIRMED on both game saves.
        /// </summary>
        public static byte[] Decrypt(byte[] file, byte[] key)
        {
            var o = (byte[])file.Clone();
            for (int i = StreamStart; i < o.Length; i++) o[i] ^= key[(i - StreamStart) % key.Length];
            return o;
        }

        /// <summary>
        /// Stream-phase key for a legacy username. The game applies the reversed username aligned to the end of the file
        /// (last byte of the file uses the first byte of the reversed name); the owner's name "starharrisma" is the 12 byte key "amsirrahrats".
        /// CONFIRMED for a 12 character name only; how names of other lengths are handled is not verified.
        /// </summary>
        public static byte[] KeyFromLegacyUsername(string username, int fileLength)
        {
            var rev = Encoding.ASCII.GetBytes(username.ToLowerInvariant().ToCharArray().Reverse().ToArray());
            int p = rev.Length;
            if (p == 0) throw new ArgumentException("Empty username.");
            var key = new byte[p];
            for (int j = 0; j < p; j++)
                key[j] = rev[(((j + StreamStart - fileLength + 1) % p) + p) % p];
            return key;
        }

        private static (int, byte)[] BuildKnownPlain()
        {
            var l = new List<(int, byte)>();
            byte[] head = { 0x16, 0, 0, 0, 0xAC, 0xDB, 0xFE, 0x00 };
            for (int i = 0; i < head.Length; i++) l.Add((i, head[i]));
            byte[] sig = { 0x33, 0xAA, 0xFB, 0x57, 0x99, 0xFA, 0x04, 0x10 };
            for (int i = 0; i < sig.Length; i++) l.Add((16 + i, sig[i]));
            byte[] codec = { 0x01, 0x00, 0x02, 0x00, 0x80, 0x00, 0x00, 0x01 };
            for (int i = 0; i < codec.Length; i++) l.Add((24 + i, codec[i]));
            return l.ToArray();
        }

        /// <summary>Recovers a repeating key of period 1..<see cref="MaxRecoveredPeriod"/> from the known plaintext and keeps the first one that decrypts a valid container stream.</summary>
        private static (byte[] Data, byte[] Key) RecoverAndDecrypt(byte[] file)
        {
            for (int p = 1; p <= MaxRecoveredPeriod; p++)
            {
                var key = new byte[p]; var set = new bool[p]; bool ok = true;
                foreach (var (off, val) in KnownPlain)
                {
                    byte k = (byte)(file[StreamStart + off] ^ val); int j = off % p;
                    if (set[j] && key[j] != k) { ok = false; break; }
                    key[j] = k; set[j] = true;
                }
                if (!ok || set.Any(x => !x)) continue;
                var data = Decrypt(file, key);
                try
                {
                    if (ChunkContainer.ReadAll(data, StreamStart).Count >= 2) return (data, key);
                }
                catch (Exception) { /* wrong period */ }
            }
            throw new InvalidDataException("Could not recover the key from the known plaintext: not a Revelations save, or a layout this reader does not know.");
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
