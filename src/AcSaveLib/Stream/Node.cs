using System;
using System.Collections.Generic;
using AcSaveLib.Names;

namespace AcSaveLib.Stream
{
    public enum NodeKind { Value, Object, Array, Raw }

    /// <summary>One item of the property stream.</summary>
    public sealed class Node
    {
        public uint Key { get; init; }
        public string Name => PropertyNames.Lookup(Key);
        public NodeKind Kind { get; init; }
        /// <summary>Type id found in the item header (e.g. 0x03 u8, 0x07 u32, 0x12 u64 id, 0x0a1d array).</summary>
        public ushort TypeId { get; init; }
        /// <summary>Numeric value for Kind==Value (up to 8 bytes, little endian).</summary>
        public ulong Value { get; init; }
        /// <summary>Raw value bytes (Value) or whole body (Raw).</summary>
        public byte[] Bytes { get; init; } = Array.Empty<byte>();
        /// <summary>Children for Kind==Object.</summary>
        public List<Node> Children { get; init; } = new();
        /// <summary>Elements for Kind==Array.</summary>
        public List<ArrayElement> Elements { get; init; } = new();
        /// <summary>Text for LString items (type 0x001b): UTF-16, length-prefixed in characters. Null otherwise.</summary>
        public string? Text { get; init; }
        /// <summary>Elements for packed arrays of plain numbers (Kind==Array, ElementSize &gt; 0): u8/u16/u32/u64 values.</summary>
        public List<ulong> Packed { get; init; } = new();
        /// <summary>Byte size of each packed element, 0 if this is not a packed array.</summary>
        public int ElementSize { get; init; }
        /// <summary>Offset of the item (its length field) inside the property stream.</summary>
        public int Offset { get; init; }

        public bool AsBool => Value != 0;
        public Node? Find(string name) => Find(PropertyNames.Crc32(name));
        public Node? Find(uint key) { foreach (var c in Children) if (c.Key == key) return c; return null; }
    }

    /// <summary>One element of an array item: an element key plus its child items.</summary>
    public sealed class ArrayElement
    {
        public uint Key { get; init; }
        public string Name => PropertyNames.Lookup(Key);
        public List<Node> Children { get; init; } = new();
        public Node? Find(string name) => Find(PropertyNames.Crc32(name));
        public Node? Find(uint key) { foreach (var c in Children) if (c.Key == key) return c; return null; }
    }
}
