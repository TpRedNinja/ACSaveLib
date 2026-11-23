using System;
using System.IO;

namespace AcSaveLib.Compression
{
    /// <summary>
    /// Managed LZO2A decompressor (port of the game's codec 2 routine). Flag bits are read from a byte stream, low bit first.
    /// Decompress only. The stream ends with a long match of distance 0 (end marker).
    /// </summary>
    public static class Lzo2a
    {
        /// <summary>Decodes one block. <paramref name="inputEnd"/> is the position after the end marker.</summary>
        public static byte[] Decompress(byte[] src, int srcOffset, int srcLength, int expectedLength, out int inputEnd)
        {
            var dst = new byte[expectedLength];
            int ip = srcOffset, limit = srcOffset + srcLength, op = 0;
            uint bits = 0;   // current flag bits, consumed from the low bit
            int count = 0;   // number of unread flag bits
            try
            {
                while (true)
                {
                    // Literals: a 0 flag bit means copy one byte, a 1 flag bit starts a match.
                    while (true)
                    {
                        if (count == 0) { bits |= src[ip++]; count = 8; }
                        count--;
                        if ((bits & 1) != 0) break;
                        dst[op++] = src[ip++];
                        bits >>= 1;
                    }
                    uint next = bits >> 1;
                    if (count == 0) { next |= src[ip++]; count = 8; }
                    count--;
                    int length, distance;
                    if ((next & 1) != 0)
                    {
                        // Long match: 13 bit distance, 3 bit length code (0 = length follows as extra bytes).
                        int b0 = src[ip], b1 = src[ip + 1];
                        ip += 2;
                        bits = next >> 1;
                        distance = (b0 & 0x1F) | (b1 << 5);
                        int code = b0 >> 5;
                        if (code != 0)
                        {
                            if (distance == 0) { inputEnd = ip; return Finish(dst, op); }
                            length = code + 2;
                        }
                        else
                        {
                            if (distance == 0) throw new InvalidDataException("LZO2A match with distance 0.");
                            length = 9;
                            while (src[ip] == 0) { ip++; length += 255; }
                            length += src[ip++];
                        }
                    }
                    else
                    {
                        // Short match: 2 to 5 bytes, distance up to 256.
                        next >>= 1;
                        if (count < 2) { next |= (uint)src[ip++] << count; count += 8; }
                        length = (int)(next & 3) + 2;
                        bits = next >> 2;
                        count -= 2;
                        distance = src[ip++] + 1;
                    }
                    int from = op - distance;
                    if (from < 0 || op + length > dst.Length) throw new InvalidDataException("LZO2A match outside the output buffer.");
                    for (int i = 0; i < length; i++) dst[op++] = dst[from++];
                    if (ip > limit) throw new InvalidDataException("LZO2A block read past its stated size.");
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidDataException("LZO2A block is truncated or corrupt.");
            }
        }

        private static byte[] Finish(byte[] dst, int op)
        {
            if (op != dst.Length) throw new InvalidDataException($"LZO2A block decoded to {op} bytes, expected {dst.Length}.");
            return dst;
        }
    }
}
