using System;

namespace AcSaveLib.Crypto
{
    /// <summary>
    /// Repeating-key XOR whose key phase is anchored to the END of the file
    /// (key index = (i - fileLength) mod keyLength). Used by AC4 Black Flag saves.
    /// The key can be recovered from any known plaintext, so no per-user ID is needed for AC4.
    /// </summary>
    public static class XorTailCipher
    {
        /// <summary>Recovers the key from a known plaintext located at <paramref name="offset"/>.</summary>
        public static byte[] RecoverKey(byte[] file, int offset, byte[] knownPlaintext, int keyLength)
        {
            if (knownPlaintext.Length < keyLength) throw new ArgumentException("Known plaintext must cover a full key period.");
            if (offset + keyLength > file.Length) throw new ArgumentException("File too short.");
            var key = new byte[keyLength];
            for (int j = 0; j < keyLength; j++)
            {
                int idx = Mod(offset + j - file.Length, keyLength);
                key[idx] = (byte)(file[offset + j] ^ knownPlaintext[j]);
            }
            return key;
        }

        /// <summary>Returns a decrypted copy; bytes before <paramref name="start"/> are copied unchanged.</summary>
        public static byte[] Apply(byte[] file, int start, byte[] key)
        {
            var o = (byte[])file.Clone();
            for (int i = start; i < o.Length; i++)
                o[i] ^= key[Mod(i - o.Length, key.Length)];
            return o;
        }

        private static int Mod(int a, int m) { int r = a % m; return r < 0 ? r + m : r; }
    }
}
