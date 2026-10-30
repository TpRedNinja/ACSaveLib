using System;
using System.Collections.Generic;
using System.IO;
using AcSaveLib.Compression;

namespace AcSaveLib.Container
{
    /// <summary>
    /// Ubisoft compressed-buffer container: signature 33 AA FB 57 99 FA 04 10, 7 header bytes,
    /// then repeated { 0x01 marker, u32 compressedSize, u32 uncompressedSize, u32 checksum, LZO1x data }.
    /// </summary>
    public static class ChunkContainer
    {
        public static readonly byte[] Signature = { 0x33, 0xAA, 0xFB, 0x57, 0x99, 0xFA, 0x04, 0x10 };
        private const int HeaderLength = 15;   // signature(8) + u32 version(1) + 3 bytes (0x80 0x00 0x00)

        public static bool HasSignature(byte[] d, int pos)
        {
            if (pos < 0 || pos + 8 > d.Length) return false;
            for (int i = 0; i < 8; i++) if (d[pos + i] != Signature[i]) return false;
            return true;
        }

        public static int IndexOfSignature(byte[] d, int from)
        {
            for (int p = from; p + 8 <= d.Length; p++) if (HasSignature(d, p)) return p;
            return -1;
        }

        /// <summary>Reads one container starting at <paramref name="pos"/>; returns the concatenated uncompressed blocks.</summary>
        public static byte[] Read(byte[] d, int pos, out int endPos)
        {
            if (!HasSignature(d, pos)) throw new InvalidDataException($"No container signature at {pos}.");
            int q = pos + HeaderLength;
            int nextSig = IndexOfSignature(d, pos + 8);
            int limit = nextSig >= 0 ? nextSig : d.Length;
            using var ms = new MemoryStream();
            while (q + 13 <= limit && d[q] == 1)
            {
                uint cs = BitConverter.ToUInt32(d, q + 1), us = BitConverter.ToUInt32(d, q + 5);
                // A trailer byte after the last block can equal the 0x01 marker; reject blocks that do not fit before the next container.
                if (cs == 0 || us == 0 || (long)q + 13 + cs > limit) break;
                q++;
                // d[q+8..q+12] = checksum (algorithm not identified; ignored)
                var block = Lzo1x.Decompress(d, q + 12, (int)cs, (int)us);
                ms.Write(block, 0, block.Length);
                q += 12 + (int)cs;
            }
            endPos = q;
            return ms.ToArray();
        }

        /// <summary>Reads every container in the file, in order.</summary>
        public static List<byte[]> ReadAll(byte[] d, int start)
        {
            var list = new List<byte[]>();
            int p = IndexOfSignature(d, start);
            while (p >= 0)
            {
                list.Add(Read(d, p, out int end));
                p = IndexOfSignature(d, end);
            }
            return list;
        }
    }
}
