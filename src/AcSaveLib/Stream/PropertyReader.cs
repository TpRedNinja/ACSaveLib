using System;
using System.Collections.Generic;

namespace AcSaveLib.Stream
{
    /// <summary>
    /// Reads the CRC32-keyed property stream (see docs/FORMAT_PROPERTY_STREAM.md).
    /// Item = u32 L | body[L]; body = key u32 | ext u32 | 00 00 | type u16 | 0x0b | value.
    /// </summary>
    public static class PropertyReader
    {
        public static List<Node> Parse(byte[] d, int start, int end)
        {
            var list = new List<Node>();
            int p = start;
            while (p + 4 <= end)
            {
                uint L = BitConverter.ToUInt32(d, p);
                if (L == 0) { p++; continue; }                       // zero padding between items
                if (L < 13 || p + 4 + L > end) break;
                int b = p + 4, len = (int)L;
                uint key = BitConverter.ToUInt32(d, b);
                ushort type = BitConverter.ToUInt16(d, b + 10);
                bool zeros = d[b + 8] == 0 && d[b + 9] == 0;

                if ((type == 0x0a1d || type == 0x0a9d) && zeros)
                {
                    uint count = BitConverter.ToUInt32(d, b + 14);
                    var node = new Node { Key = key, Kind = NodeKind.Array, TypeId = type, Offset = p, Value = count };
                    int q = b + 18, aend = b + len;
                    for (uint i = 0; i < count; i++)
                    {
                        while (q < aend && d[q] == 0) q++;
                        if (q + 12 > aend) break;
                        uint ek = BitConverter.ToUInt32(d, q), size = BitConverter.ToUInt32(d, q + 4), l2 = BitConverter.ToUInt32(d, q + 8);
                        var el = new ArrayElement { Key = ek };
                        el.Children.AddRange(Parse(d, q + 12, (int)Math.Min((long)q + 12 + l2, aend)));
                        node.Elements.Add(el);
                        q += 8 + (int)size;
                    }
                    list.Add(node); p = b + len; continue;
                }

                if (len >= 39 && zeros && d[b + 12] == 0x0b && (type == 0x13 || type == 0x16) && SameFour(d, b + 4, b + 27))
                {
                    var node = new Node { Key = key, Kind = NodeKind.Object, TypeId = type, Offset = p, Value = BitConverter.ToUInt32(d, b + 4) };
                    node.Children.AddRange(Parse(d, b + 39, b + len));
                    list.Add(node); p = b + len; continue;
                }

                if (zeros && d[b + 12] == 0x0b && len - 13 <= 12)
                {
                    var val = new byte[len - 13];
                    Buffer.BlockCopy(d, b + 13, val, 0, val.Length);
                    int n = (type == 0x12 || type == 0x09) ? Math.Min(8, val.Length) : Math.Min(8, val.Length);
                    ulong v = 0;
                    for (int i = n - 1; i >= 0; i--) v = (v << 8) | val[i];
                    list.Add(new Node { Key = key, Kind = NodeKind.Value, TypeId = type, Value = v, Bytes = val, Offset = p });
                    p = b + len; continue;
                }

                var raw = new byte[len];
                Buffer.BlockCopy(d, b, raw, 0, len);
                list.Add(new Node { Key = key, Kind = NodeKind.Raw, TypeId = type, Bytes = raw, Offset = p });
                p = b + len;
            }
            return list;
        }

        private static bool SameFour(byte[] d, int a, int b) => d[a] == d[b] && d[a + 1] == d[b + 1] && d[a + 2] == d[b + 2] && d[a + 3] == d[b + 3];
    }
}
