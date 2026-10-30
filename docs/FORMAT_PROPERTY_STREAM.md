# Property stream format (likely shared by many Ubisoft games - verify per game)

Item: `u32 L | body[L]`. Body (b = body start): key u32 (CRC32 zlib of the property name, little-endian) | ext u32 | `00 00` | type u16 | `0b` | value.
All layouts below were verified on 5 AC4 saves (0 RAW nodes). Parser: src/AcSaveLib/Stream/PropertyReader.cs.

- Leaf: value length = L - 13. Types: 0x0000/0x00c0 bool(1), 0x03 u8, 0x07 u32, 0x09/0x12/0x19 u64, 0x159 enum.
  Float values (ShipLife, CooldownTime) are stored as u32 bits. For u64 IDs and arrays ext repeats the key; for plain leaves ext is 0.
- Enum 0x159: u32 value + 4 bytes = CRC32 of the value's name (e.g. InvItem_Money = 12f6b6f1). The hash is a way to confirm enum value names.
- Object arrays 0x0a1d / 0x0a9d / 0x0b1d: `01 | u32 count` at b+13..b+17 (count at b+14), elements from b+18:
  `key | u32 size | u32 L2 | children`; next element at elementStart + 8 + size; skip zero padding.
- Fixed object arrays 0x0b17 (QuickSlotItems): count at b+13, elements from b+17.
- Packed number arrays (type low byte 0x9d or 0x1d, but not 0x0a/0x0b high byte): `0b | 01 | u32 count | raw values`.
  Element size = payload / count in {1,2,4,8} (u64 for 0x089d/0x091d, u8 for 0x019d, bool for 0x001d). Empty arrays: size by type.
- u32 packed array 0x0397: like above but without the `01` byte, length = 17 + 4*count.
- LString 0x001b: length = 17 + 2*(chars+1), UTF-16 text with terminator (e.g. "<lang=01>multiplayer rewards").
- Object: type 0x13/0x16, L >= 39, ext == body[27:31]; children start at body offset 39.

ACViewer quirk (NOT a library bug): ACViewer shows simple primitive arrays one slot off compared to the file's byte layout. Trust the bytes.
