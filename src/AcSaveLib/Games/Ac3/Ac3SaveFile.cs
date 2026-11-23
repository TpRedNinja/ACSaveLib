using System;
using System.IO;
using System.Text;
using AcSaveLib.Crypto;
using AcSaveLib.Games.Acl;

namespace AcSaveLib.Games.Ac3
{
    /// <summary>
    /// Assassin's Creed III (original, PC) saves. Layout is documented in docs/FORMAT_AC3.md.
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
        /// <summary>The 32 character id of header 0x224, empty otherwise.</summary>
        public string HeaderId { get; private set; } = "";
        /// <summary>The 16 byte XOR key, index = (i - fileLength) mod 16.</summary>
        public byte[] Key { get; private set; } = Array.Empty<byte>();
        public byte[] Decrypted { get; private set; } = Array.Empty<byte>();
        /// <summary>The decrypted stream parsed with the Liberation reader (containers and property items).</summary>
        public AclSaveFile Stream { get; private set; } = null!;

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
            if (BitConverter.ToUInt32(dec, start + 4) != AclSaveFile.FileMagic)
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
            r.Stream = AclSaveFile.Parse(stream);
            return r;
        }
    }
}
