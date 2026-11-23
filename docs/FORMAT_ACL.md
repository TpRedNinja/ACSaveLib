# ACL (Assassin's Creed Liberation HD, PC) save format
Reader: `Games/Acl/AclSaveFile.cs` (C#, read-only; containers and raw property items only, no typed models), CLI verb `acl`. CONFIRMED on the two files in the game's `SAVES` folder: every block decodes to its stated size and every item parses (one new-game save, `ASSASSINSCREEDLIBERATIONSAVEGAME0.SAV`, and `OPTIONS`). ACViewer does not detect these files, so nothing here is checked against a viewer.

No encryption. Little-endian. Compressed with LZO1x (codec id 0, the AC4 codec), not LZO2A. The container and file framing look like Brotherhood's (`FORMAT_ACB.md`).

## File layout
```
u32 33 | u32 0x00FEDBAC | 8 zero bytes | u32 length | u32 uncompressed size of container 0
container 0 (header) | 16 byte gap | container 1 | [gap | container 2 | gap | container 3]
```

| Offset | Size | Meaning |
| --- | --- | --- |
| `0x00` | u32 | `0x21` (33), equal to the header property Version |
| `0x04` | u32 | `0x00FEDBAC` (the magic Brotherhood and AC4 also use) |
| `0x08` | 8 | zero |
| `0x10` | u32 | byte length of everything after this field up to the end of container 0 (`0xF4` in the game save) |
| `0x14` | u32 | uncompressed size of container 0 (`0x116` = 278 in both files) |
| `0x18` | 8 | container signature `33 aa fb 57 99 fa 04 10` |
| `0x20` | 7 | `01 00 00 00 80 00 00`: u16 version 1, u8 codec id 0 (LZO1x), u32 chunk size `0x8000` [likely] |

## Container and blocks
- Container and block framing is the one in `ChunkContainer`: a 15 byte signature header, then blocks `01` | u32 compressed size | u32 uncompressed size | u32 checksum (algorithm not identified, ignored) | data.
- Block sizes: container 1 of the game save has 6 blocks (190339 bytes decoded); every other container has 1 block.
- The file ends exactly at the end of the last block (no trailer).
- Gap between containers (16 bytes): u32 length of everything after this field up to the end of the next container | u32 value | u32 marker | u32 uncompressed size of the next container.

| File | Gap | Value | Marker (little-endian bytes) |
| --- | --- | --- | --- |
| game save | 0 to 1 | 3 | `00 fe ca 00` |
| OPTIONS | 0 to 1 | 12 | `11 ce fa 11` |
| OPTIONS | 1 to 2 | 2 | `22 fe ef 21` |
| OPTIONS | 2 to 3 | 9 | `11 0b b0 11` |

- Game save containers decode to 278 and 190339 bytes. OPTIONS containers decode to 278, 1844, 470 and 257 bytes.

## Property stream
- A decoded container starts with 14 zero bytes, then the root key (u32), two u32 sizes, and the items from offset 26. Brotherhood has 10 zero bytes and the items from offset 22.
- Item framing is the AC2 one (`FORMAT_AC2.md`, `FORMAT_PROPERTY_STREAM.md`): `u32 L | key | ext | 00 00 | type u16 | 0b | value`, CRC32 property names. `PropertyReader` parses every container of both files without leftovers; each file has one raw item (see `FIELDS_ACL.md`).
- Root keys: container 0 `0xBDBE3B52` (SaveGame); game save container 1 `0x94D6F8F1` (the main stream root of AC4 and Brotherhood). OPTIONS containers 1 to 3: `0x305AE1A8`, `0xC9876D66`, `0xB4B55039` (the last two are the roots Brotherhood and Revelations options files also have).
- Header properties in order: Version (33), MagicNumber (`0x00FEDBAC`), DLCPackagesBits, CodeChangelist (3638899), SaveGameID, IsOptions, SaveGameName ("Aveline" in the game save, "Options" in OPTIONS), TimePlayed, PlayerGlobalSynch, SaveGameDate (Unix seconds), `d413aa09` (627070 in the game save, 0 in OPTIONS). There is no CurrentWorld or CurrentMission in the header.

## Names
- Names come from `Names/names.txt`. Most keys already resolved through the Brotherhood and Revelations names.
- Names for the ship, shop and fog-map keys are CRC32 matches of word combinations, each at a matching spot, not seen in a viewer [likely]: `ShipListSave`, `ShipSaveData`, `ShipTypeSave`, `ShipName`, `ShipContext`, `UsedSave`, `RouteSave`, `PendingEventsSave`, `Price`, `BuyPrice`, `PriceMultiplier`, `Quantity`, `Locked`, `HolsterLevel`, `HatLevel`, `Width`, `Height`, `HuntingData`, `DiscoveredAnimals`, `UplayActions`, `LastNewsSeen`.
- Open: 38 unnamed keys in the game save and 61 in OPTIONS (`acl <file> unknown`).
