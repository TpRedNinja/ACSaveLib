using System;

namespace AcSaveLib.Compression
{
    /// <summary>Managed LZO1X decompressor (port of the reference algorithm). Decompress only.</summary>
    public static class Lzo1x
    {
        public static byte[] Decompress(byte[] src, int srcOffset, int srcLength, int expectedLength)
        {
            var dst = new byte[expectedLength];
            int ip = srcOffset, ipEnd = srcOffset + srcLength, op = 0, t, m;
            if (ipEnd - ip < 3) throw new InvalidOperationException("LZO input too short.");

            if (src[ip] > 17)
            {
                t = src[ip++] - 17;
                if (t < 4) goto match_next;
                CopyLiterals(src, ref ip, dst, ref op, t);
                goto first_literal_run;
            }

        top:
            t = src[ip++];
            if (t >= 16) goto match;
            if (t == 0)
            {
                while (src[ip] == 0) { t += 255; ip++; }
                t += 15 + src[ip++];
            }
            CopyLiterals(src, ref ip, dst, ref op, t + 3);

        first_literal_run:
            t = src[ip++];
            if (t >= 16) goto match;
            m = op - (1 + 0x0800) - (t >> 2) - (src[ip++] << 2);
            CopyMatch(dst, ref op, m, 3);
            goto match_done;

        match:
            if (t >= 64)
            {
                m = op - 1 - ((t >> 2) & 7) - (src[ip++] << 3);
                t = (t >> 5) - 1;
                CopyMatch(dst, ref op, m, t + 2);
            }
            else if (t >= 32)
            {
                t &= 31;
                if (t == 0)
                {
                    while (src[ip] == 0) { t += 255; ip++; }
                    t += 31 + src[ip++];
                }
                m = op - 1 - ((src[ip] | (src[ip + 1] << 8)) >> 2);
                ip += 2;
                CopyMatch(dst, ref op, m, t + 2);
            }
            else if (t >= 16)
            {
                m = op - ((t & 8) << 11);
                t &= 7;
                if (t == 0)
                {
                    while (src[ip] == 0) { t += 255; ip++; }
                    t += 7 + src[ip++];
                }
                m -= (src[ip] | (src[ip + 1] << 8)) >> 2;
                ip += 2;
                if (m == op) goto done;          // end-of-stream marker
                m -= 0x4000;
                CopyMatch(dst, ref op, m, t + 2);
            }
            else
            {
                m = op - 1 - (t >> 2) - (src[ip++] << 2);
                CopyMatch(dst, ref op, m, 2);
            }

        match_done:
            t = src[ip - 2] & 3;
            if (t == 0) goto top;

        match_next:
            CopyLiterals(src, ref ip, dst, ref op, t);
            t = src[ip++];
            goto match;

        done:
            if (op != expectedLength)
                throw new InvalidOperationException($"LZO produced {op} bytes, expected {expectedLength}.");
            return dst;
        }

        private static void CopyLiterals(byte[] src, ref int ip, byte[] dst, ref int op, int n)
        {
            if (ip + n > src.Length || op + n > dst.Length) throw new InvalidOperationException("LZO literal overrun.");
            Buffer.BlockCopy(src, ip, dst, op, n); ip += n; op += n;
        }

        private static void CopyMatch(byte[] dst, ref int op, int m, int n)
        {
            if (m < 0 || op + n > dst.Length) throw new InvalidOperationException("LZO lookbehind/overrun.");
            for (int i = 0; i < n; i++) dst[op++] = dst[m++];   // byte-wise: matches may overlap
        }
    }
}
