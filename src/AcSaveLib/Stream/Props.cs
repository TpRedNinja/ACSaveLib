using System;
using System.Collections.Generic;

namespace AcSaveLib.Stream
{
    /// <summary>
    /// Convenience reader over the children of an object or array element. Missing properties yield defaults,
    /// so models never throw on a save that lacks a field.
    /// </summary>
    public readonly struct Props
    {
        private readonly List<Node>? _items;
        public Props(List<Node>? items) { _items = items; }

        public bool IsEmpty => _items == null || _items.Count == 0;

        public Node? Get(string name)
        {
            if (_items == null) return null;
            uint key = Names.PropertyNames.Crc32(name);
            foreach (var n in _items) if (n.Key == key) return n;
            return null;
        }

        /// <summary>Child by raw key, for properties whose name is not known yet.</summary>
        public Node? GetByKey(uint key)
        {
            if (_items == null) return null;
            foreach (var n in _items) if (n.Key == key) return n;
            return null;
        }

        public ulong U64(string name) => Get(name)?.Value ?? 0;
        public uint U32(string name) => (uint)(Get(name)?.Value ?? 0);
        public int S32(string name) => unchecked((int)(uint)(Get(name)?.Value ?? 0));
        public byte U8(string name) => (byte)(Get(name)?.Value ?? 0);
        public bool Bool(string name) => (Get(name)?.Value ?? 0) != 0;
        public float Float(string name) => BitConverter.UInt32BitsToSingle((uint)(Get(name)?.Value ?? 0));
        /// <summary>Enum value (low 32 bits; the high 32 bits hold the hash of the value's name).</summary>
        public uint Enum(string name) => (uint)((Get(name)?.Value ?? 0) & 0xFFFFFFFF);
        public string Text(string name) => Get(name)?.Text ?? "";

        /// <summary>
        /// String property of type 0x1A (u32 byte length, ASCII characters, 0 byte), e.g. the header SaveGameName.
        /// Items of 8 bytes or fewer are read as plain values (body = length + characters); longer ones come back as Raw nodes
        /// whose body still holds the item header (key, 6 zero bytes, type 0x001A, 0x0B), so the length starts at offset 13.
        /// </summary>
        public string AnsiString(string name)
        {
            var n = Get(name);
            if (n == null) return "";
            var b = n.Bytes;
            int off = n.Kind == NodeKind.Raw ? 13 : 0;
            if (b.Length < off + 4) return "";
            int len = (int)BitConverter.ToUInt32(b, off);
            if (len < 0 || off + 4 + len > b.Length) return "";
            return System.Text.Encoding.ASCII.GetString(b, off + 4, len);
        }

        /// <summary>Child object, or an empty Props if absent.</summary>
        public Props Obj(string name) => new Props(Get(name)?.Children);

        /// <summary>Elements of an object array (each element's children).</summary>
        public IEnumerable<Props> Elements(string name)
        {
            var n = Get(name);
            if (n == null) yield break;
            foreach (var e in n.Elements) yield return new Props(e.Children);
        }

        public int ElementCount(string name) => Get(name)?.Elements.Count ?? 0;

        /// <summary>Values of a packed array of numbers (IDs, bytes, bools, u32).</summary>
        public List<ulong> Packed(string name) => Get(name)?.Packed ?? new List<ulong>();
        public List<uint> PackedU32(string name) { var r = new List<uint>(); foreach (var v in Packed(name)) r.Add((uint)v); return r; }
        public List<bool> PackedBool(string name) { var r = new List<bool>(); foreach (var v in Packed(name)) r.Add(v != 0); return r; }
        public List<byte> PackedU8(string name) { var r = new List<byte>(); foreach (var v in Packed(name)) r.Add((byte)v); return r; }
    }
}
