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

                // Array layouts seen so far:
                //   0x0a1d / 0x0a9d / 0x0b1d : 01 | u32 count  (count at body+14, elements from body+18)
                //   0x0b17 (fixed-size array) : u32 count      (count at body+13, elements from body+17); body+8..9 may be non-zero
                bool arrA = (type == 0x0a1d || type == 0x0a9d || type == 0x0b1d) && zeros;
                bool arrB = type == 0x0b17 && len >= 17 && d[b + 12] == 0x0b;
                if (arrA || arrB)
                {
                    uint count = BitConverter.ToUInt32(d, arrA ? b + 14 : b + 13);
                    var node = new Node { Key = key, Kind = NodeKind.Array, TypeId = type, Offset = p, Value = count };
                    int q = arrA ? b + 18 : b + 17, aend = b + len;
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

                // Packed array of plain numbers: body = key | ext | 00 00 | type | 0b | 01 | u32 count | count * elemSize bytes.
                // Seen: 0x089d / 0x091d (u64 IDs), 0x019d (u8), 0x001d (bool). Object arrays (0x0a.., 0x0b..) are handled elsewhere.
                int hi = type >> 8;
                if (zeros && len >= 18 && d[b + 12] == 0x0b && d[b + 13] == 1 && ((type & 0xff) == 0x9d || (type & 0xff) == 0x1d) && hi != 0x0a && hi != 0x0b)
                {
                    uint cnt = BitConverter.ToUInt32(d, b + 14);
                    long payload = len - 18;
                    int es = cnt == 0 ? (hi == 0x08 || hi == 0x09 ? 8 : 1) : (payload % cnt == 0 ? (int)(payload / cnt) : 0);
                    if ((cnt == 0 && payload == 0) || (cnt > 0 && (es == 1 || es == 2 || es == 4 || es == 8)))
                    {
                        var pn = new Node { Key = key, Kind = NodeKind.Array, TypeId = type, Offset = p, Value = cnt, ElementSize = es };
                        for (int i = 0; i < cnt; i++)
                        {
                            ulong v = 0;
                            for (int j = es - 1; j >= 0; j--) v = (v << 8) | d[b + 18 + i * es + j];
                            pn.Packed.Add(v);
                        }
                        list.Add(pn); p = b + len; continue;
                    }
                }

                // LString: body = key | ext | 00 00 | 1b 00 | 0b | u32 charCount | UTF-16 chars + terminator
                if (type == 0x001b && len >= 19 && d[b + 12] == 0x0b)
                {
                    uint chars = BitConverter.ToUInt32(d, b + 13);
                    if (len == 17 + 2L * (chars + 1))
                    {
                        string text = System.Text.Encoding.Unicode.GetString(d, b + 17, (int)chars * 2);
                        list.Add(new Node { Key = key, Kind = NodeKind.Value, TypeId = type, Offset = p, Text = text, Value = chars });
                        p = b + len; continue;
                    }
                }

                // Fixed-size packed array: body = key | 00000000 | u16 count | type | 0b | u32 count | count * 4 bytes (u32).
                // Seen: 0x0397 (OnlineMissionIDs, 100 x u32).
                if (type == 0x0397 && len >= 17 && d[b + 12] == 0x0b)
                {
                    uint cnt = BitConverter.ToUInt32(d, b + 13);
                    if (cnt > 0 && len == 17 + 4L * cnt)
                    {
                        var pn = new Node { Key = key, Kind = NodeKind.Array, TypeId = type, Offset = p, Value = cnt, ElementSize = 4 };
                        for (int i = 0; i < cnt; i++) pn.Packed.Add(BitConverter.ToUInt32(d, b + 17 + i * 4));
                        list.Add(pn); p = b + len; continue;
                    }
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
