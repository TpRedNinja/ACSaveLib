# Property stream format (likely shared by many Ubisoft games - verify per game)

Item: `u32 L | body[L]`. Body: key u32 (CRC32 zlib of the property name, little-endian) | ext u32 | `00 00` | type u16 | `0b` | value.
- Leaf: value length = L - 13. Types: 0x0000/0x00c0 bool(1), 0x03 u8, 0x07 u32, 0x09/0x12/0x19 u64, 0x159 enum (u32 + 4 hash bytes).
  For u64 IDs and arrays ext repeats the key; for plain leaves ext is 0.
- Array: type 0x0a1d/0x0a9d; value = `01 | u32 count` (body offset 14). Elements follow inside the same L:
  `key | u32 size | u32 L2 | children`; next element at elementStart + 8 + size; skip zero padding.
- Object: type 0x13/0x16, L >= 39, ext == body[27:31]; children start at body offset 39.
Parser: src/AcSaveLib/Stream/PropertyReader.cs. Unknown types come back as NodeKind.Raw.
