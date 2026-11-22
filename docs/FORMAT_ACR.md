# ACR (Assassin's Creed Revelations, PC) .save format
Reference reader: `tools/acb_reference.py` reads the plain options save. The C# library does not read Revelations yet. Findings below come from 3 saves and from `ACRSP.exe` in IDA (imagebase `0x400000`; `sub_...` are IDA's automatic names, nothing is renamed in the IDB).

## Header
- Same header as the AC2 Steam saves: u32 `0x224` (header length) | u32 `0` | 32 character ASCII id at `0x08` | UTF-16 title from `0x28` | zero padding up to `0x228` (see `FORMAT_AC2.md`).
- The title reads `AC2[N]-tm_<yyyy>_<mm>_<dd> <hh>:<mm>:<ss>`: the game writes the prefix `AC2`. N is `-1` in the options-only save, `0` and `1` in the two game saves.

## Plain save (options only, 4351 bytes)
- The stream starts at `0x228` with u32 `0x16` | `ac db fe 00` | u32 length | u32 uncompressed size | container signature | `01 00 02 00 80 00 00` | blocks, the same container as Brotherhood (`FORMAT_ACB.md`).
- Five containers (uncompressed 283, 1689, 162, 9268 and 68 bytes). Every block decodes with the LZO2A decompressor.
- The first block starts with the same bytes as Brotherhood's options save (`06 00 e1 00 52 3b be bd ...`) but is shorter (161 bytes compressed instead of 165).

## Encrypted saves
- The two larger saves (game saves, slots 0 and 1) are enciphered from `0x228`.
- The AC4 scheme (repeating 16 byte key aligned to the end of the file, recovered from the known plaintext at `+0` and `+16`) does not fit: the key bytes recovered from `+0` (`61 6d 73 69 72 72 61 68` in one save) differ from those recovered from `+16` (`72 72 61 68 72 61 74 73`).
- GUESS: the key is not a plain repeating 16 byte key. Not solved.

## Game code (`ACRSP.exe`)
| VA | What |
| --- | --- |
| `0x463B50` | Save loader (`sub_463B50`). Checks the first u32 is `22` and the second is `0xFEDBAC`, reads a length at `+8`, builds a buffer from `+12`, then reads further length-prefixed buffers, one per object |
| `0x4649E0` | Save writer (`sub_4649E0`). Writes `22` and `0xFEDBAC` into the header |
| `0x1C2AFF0` | Container signature check: u32 at `+4` equals `0x57FBAA33` and u32 at `+8` equals `0x1004FA99` |
| `0x1C2BF60` | Decompresses a container: u32 uncompressed size, then the container |
| `0x1C2BB20` | Creates the chunk stream (chunk size `0x8000`, codec id at `+72`) |
| `0x1C2B400` | Decodes one chunk through the codec table |
| `0x2945484` | Codec table, 5 dwords per codec, first dword is the decompress function. Ids and names: 0 `LZO1X_1`, 1 `LZO1X_999`, 2 `LZO2A`, 3 `LZX` |
| `0x1C3E960` | Decompress function of codec 2 (LZO2A) |
