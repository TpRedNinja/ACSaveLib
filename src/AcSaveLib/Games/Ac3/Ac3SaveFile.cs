using System;
using System.IO;
using System.Text;
using AcSaveLib.Crypto;
using AcSaveLib.Games.Acl;

namespace AcSaveLib.Games.Ac3
{
    /// <summary>
    /// Assassin's Creed III (original, PC) saves. Layout is documented in docs/FORMAT_AC3.md.
    /// All variants hold a Liberation-style stream (u32 33, u32 0x00FEDBAC, ... LZO1x containers) XORed with a 16 byte key anchored to the end of the file.
    /// Variant A (.save, header 0x204): u32 0x204, u32 0, UTF-16 title from 0x08, stream from 0x208.
    /// Variant B (.save, header 0x224): u32 0x224, u32 0, 32 character ASCII id at 0x08, UTF-16 title from 0x28, stream from 0x228, another key.
    /// Variant C (.sav): no header, the stream from offset 0, a third key. Read-only.
    /// </summary>
    public sealed class Ac3SaveFile
    {
        /// <summary>Repeating keys, index = (i - fileLength) mod 16. Each key decrypts every file of its variant that was available.</summary>
        public static readonly byte[] KeyA = Convert.FromHexString("0154a57f32bb4407857877ed29423b2e");
        public static readonly byte[] KeyB = Convert.FromHexString("0a2abb77fab80caad1af78519d683e59");
        public static readonly byte[] KeyC = Convert.FromHexString("48ee7a4e94f30deeb36f2781ec1108d8");

        /// <summary>Header length of the file (0x204 or 0x224), 0 for the headerless variant.</summary>
        public int HeaderLength { get; private set; }
        public string Title { get; private set; } = "";
        /// <summary>The 32 character id of header 0x224, empty otherwise.</summary>
        public string HeaderId { get; private set; } = "";
        public byte[] Decrypted { get; private set; } = Array.Empty<byte>();
        /// <summary>The decrypted stream parsed with the Liberation reader (containers and property items).</summary>
        public AclSaveFile Stream { get; private set; } = null!;

        public static Ac3SaveFile Load(string path, byte[]? key = null) => Parse(File.ReadAllBytes(path), key);

        public static Ac3SaveFile Parse(byte[] file, byte[]? key = null)
        {
            uint first = file.Length >= 4 ? BitConverter.ToUInt32(file, 0) : 0;
            int hdr = first == 0x204 || first == 0x224 ? (int)first : 0;
            int start = hdr == 0 ? 0 : hdr + 4;
            if (file.Length < start + 64) throw new InvalidDataException("Not an AC3 save: file too short.");
            key ??= hdr == 0x204 ? KeyA : hdr == 0x224 ? KeyB : KeyC;
            var dec = XorTailCipher.Apply(file, start, key);
            if (BitConverter.ToUInt32(dec, start + 4) != AclSaveFile.FileMagic)
                throw new InvalidDataException("Not an AC3 save with a known key: magic 0x00FEDBAC not found after decrypting.");
            var r = new Ac3SaveFile { Decrypted = dec, HeaderLength = hdr };
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
