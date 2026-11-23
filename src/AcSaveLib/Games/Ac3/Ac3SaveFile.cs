using System;
using System.IO;
using System.Text;
using AcSaveLib.Crypto;
using AcSaveLib.Games.Acl;

namespace AcSaveLib.Games.Ac3
{
    /// <summary>
    /// Assassin's Creed III (original, PC) saves. Layout is documented in docs/FORMAT_AC3.md.
    /// Both variants hold a Liberation-style stream (u32 33, u32 0x00FEDBAC, ... LZO1x containers) XORed with a 16 byte key anchored to the end of the file.
    /// The old variant (.save) has a plain 0x204 byte header (u32 0x204, u32 0, UTF-16 title) and the stream from offset 0x208.
    /// The newer variant (.sav) has no header and the stream from offset 0, with a different key. Read-only.
    /// </summary>
    public sealed class Ac3SaveFile
    {
        public const int HeaderLength = 0x204;
        /// <summary>The stream starts here; the first u32 of the header (0x204) is followed by a zero u32.</summary>
        public const int StreamStart = 0x208;

        /// <summary>Repeating key, index = (i - fileLength) mod 16. Recovered from the three Theta saves; identical in all of them.</summary>
        public static readonly byte[] DefaultKey = Convert.FromHexString("0154a57f32bb4407857877ed29423b2e");

        /// <summary>Key of the headerless .sav variant, same indexing.</summary>
        public static readonly byte[] SavKey = Convert.FromHexString("48ee7a4e94f30deeb36f2781ec1108d8");

        public string Title { get; private set; } = "";
        public byte[] Decrypted { get; private set; } = Array.Empty<byte>();
        /// <summary>The decrypted stream parsed with the Liberation reader (containers and property items).</summary>
        public AclSaveFile Stream { get; private set; } = null!;

        public static Ac3SaveFile Load(string path, byte[]? key = null) => Parse(File.ReadAllBytes(path), key);

        public static Ac3SaveFile Parse(byte[] file, byte[]? key = null)
        {
            if (file.Length >= StreamStart + 64 && BitConverter.ToUInt32(file, 0) == HeaderLength)
            {
                var dec = XorTailCipher.Apply(file, StreamStart, key ?? DefaultKey);
                var r = new Ac3SaveFile { Decrypted = dec };
                int end = 8; while (end + 1 < HeaderLength && (dec[end] | dec[end + 1]) != 0) end += 2;
                r.Title = Encoding.Unicode.GetString(dec, 8, end - 8);
                var stream = new byte[dec.Length - StreamStart];
                Array.Copy(dec, StreamStart, stream, 0, stream.Length);
                r.Stream = AclSaveFile.Parse(stream);
                return r;
            }
            var all = XorTailCipher.Apply(file, 0, key ?? SavKey);
            if (all.Length < 64 || BitConverter.ToUInt32(all, 4) != AclSaveFile.FileMagic)
                throw new InvalidDataException("Not an AC3 save: neither the 0x204 header nor the headerless stream was found.");
            return new Ac3SaveFile { Decrypted = all, Stream = AclSaveFile.Parse(all) };
        }
    }
}
