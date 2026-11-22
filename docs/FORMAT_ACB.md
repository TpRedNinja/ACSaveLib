# ACB (Assassin's Creed Brotherhood, PC) .SAV format
Reference reader: `tools/acb_reference.py` (Python). The C# library does not read Brotherhood yet. CONFIRMED on 6 saves and 1 options file (100%, Free Roam, True 100%, personal saves): every block decodes to its stated size.
No encryption. Little-endian. Compressed with LZO2A, not LZO1x.

```
u32 22 | u32 0x00FEDBAC | u32 length of the rest of the first buffer | u32 uncompressed size of container 1
container 1 (header properties) | 16 byte gap | container 2 (main property stream)
```

| Offset | Size | Meaning |
| --- | --- | --- |
| `0x00` | u32 | `0x16` (22) |
| `0x04` | 4 | `ac db fe 00` (u32 `0x00FEDBAC`, the magic AC2 and AC4 also use) |
| `0x08` | u32 | byte length of everything after this field up to the end of container 1 (`4 + 8 + 7 + 13 + compressed size` for a one-block container) |
| `0x0C` | u32 | uncompressed size of container 1 (283 or 284) |
| `0x10` | 8 | container signature `33 aa fb 57 99 fa 04 10` |
| `0x18` | 7 | `01 00 02 00 80 00 00`: u16 version 1, u8 codec id 2 (LZO2A), u32 chunk size `0x8000` (LIKELY; AC4 has codec id 0, LZO1X_1) |
| `0x1F` | | first block |

## Container and blocks
- Block: `01` | u32 compressed size | u32 uncompressed size | u32 checksum (algorithm not identified, ignored) | LZO2A data. A container is the 15 byte signature header followed by its blocks.
- Codec: LZO2A, a byte-flag LZO variant. CONFIRMED: every block of all test files decodes to exactly its stated uncompressed size and consumes exactly its stated compressed size. `tools/acb_reference.py` holds the decompressor (port of the game's routine, see `FORMAT_ACR.md`).
- Gap between container 1 and 2 (16 bytes): u32 length of everything after this field | u32 `1` | u32 `0x00CAFE00` | u32 uncompressed size of container 2.
- Options files (`*_OPTIONS`, about 1 KB) have three containers (283, 1310 and 162 bytes). Their second gap holds the length, `3` and `0x11FACE11`, then the size 1310; the file ends with 5 more bytes after the last block.
- Container 1 is 283 or 284 bytes. Container 2 has 9 to 18 blocks (263871 to 568393 bytes in the samples).

## Property stream
- A decoded container starts with 10 zero bytes, then the root key (u32), two u32 sizes, and the items from offset 22.
- Item framing is the AC2 one (`FORMAT_AC2.md`, `FORMAT_PROPERTY_STREAM.md`): `u32 L | key | ext | 00 00 | type u16 | 0b | value`, CRC32 property names.
- Container 1 root key `0xBDBE3B52` (SaveGame). Header properties in order: Version (`0x16`), MagicNumber, DLCPackagesBits (6), CodeChangelist (`0x055E0F`), SaveGameID, IsOptions, SaveGameName ("Options" in the options file), CurrentWorld, CurrentMission, TimePlayed, PlayerGlobalSynch, SaveGameDate.
- Container 2 root key `0x94D6F8F1` (the AC4 main stream root key). Top-level items, same order in the 100% and the Free Roam save: ContextID, NarrativeMissionHistory, SideMissionHistory, World, PSPState, SnapshotPSPState, PlayerPortal, AnimusDatabaseData, UITutorialData, PresentEmailsData, AccomplishmentManagerData, GameStatsSaveData, TraineesGuildManagerSaveData, FacebookSaveGameData, PlayerProgSaveData, SnapshotPlayerProgSaveData, SnapshotPlayerProgressionDelta, VillaSaveData, SnapshotVillaSaveData, WorldUpgradeSaveData, SnapshotWorldUpgradeSaveData, `00000012`.
- Names come from `Names/names.txt`. One top-level key is not in it: `00000012`.
- Nested items from ContextID to TraineesGuildManagerSaveData decode with the AC2 object layout and match ACViewer screenshots of a no-progress save (names, types, values). Top-level keys `1fc94c94`, `e3fa794f`, `31eb1e17`, `09b1c516` are TraineesGuildManagerSaveData, FacebookSaveGameData, WorldUpgradeSaveData and SnapshotWorldUpgradeSaveData. Names inside Facebook, PlayerProg, Villa and WorldUpgrade data are not checked against screenshots yet.
