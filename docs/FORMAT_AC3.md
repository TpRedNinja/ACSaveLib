# AC3 (Assassin's Creed III, original PC) save format
Reader: `Games/Ac3/Ac3SaveFile.cs` (C#, read-only), CLI verb `ac3`. CONFIRMED on six files from the internet (not the owner's own saves): three `.save` files of a "100%" base-game pack (2012) and three `.sav` files of a "100% base game and DLC" pack (`ACIII and TOKW 100 Save File`, 2026). Every container decodes and every property item parses with no leftover bytes. ACViewer was not used, so nothing is checked against a viewer.

The decrypted stream is the Liberation HD one (`FORMAT_ACL.md`): u32 33, u32 `0x00FEDBAC`, 8 zero bytes, u32 length, u32 uncompressed size, then LZO1x chunked containers with a CRC32-keyed property stream. `Ac3SaveFile` decrypts and hands the stream to `AclSaveFile`.

## Two file variants
| Variant | Files | Layout | Key (index `(i - fileLength) mod 16`) |
| --- | --- | --- | --- |
| `.save` | `1.save`, `2.save`, `3.save` | plain 0x204 byte header, stream from `0x208` | `0154a57f32bb4407857877ed29423b2e` |
| `.sav` | `Save001.sav` to `Save003.sav` | no header, stream from offset 0 | `48ee7a4e94f30deeb36f2781ec1108d8` |

- Encryption is a repeating 16 byte XOR anchored to the END of the file (the AC4 scheme, `XorTailCipher`). The key was recovered per variant from the column majority of the zero-heavy plaintext and then checked against the known plaintext (`u32 33`, `ac db fe 00`).
- The same key decrypts all three files of a variant. Whether the key depends on the user or the game build is not known.
- Header of the `.save` variant: u32 `0x204` | u32 0 | UTF-16 title from `0x08` (`AC3[N]-tm_<yyyy>_<mm>_<dd> <hh>:<mm>:<ss>`, N = save slot, -1 for the options file) | zero padding to `0x204` | 4 zero bytes (not encrypted) | stream at `0x208`. The `.sav` variant has no title; the titles are in `saves.ini` next to the files (`Save001=AC3[-1]-tm_...`).

## Containers
| File | Slot | Containers (decoded bytes) | Header DLCPackagesBits | CodeChangelist |
| --- | --- | --- | --- | --- |
| `1.save`, `2.save` | -1 (options) | 245, 1665 / 1669, 547 | 2 | 657502 |
| `3.save` | 0 | 245, 931466 | 2 | 657502 |
| `Save001.sav` | -1 (options) | 245, 1701, 547, 3312 | 686 | 708901 |
| `Save002.sav` | 0 | 245, 1289765 | 686 | 708901 |
| `Save003.sav` | 4 | 245, 508336, 454 | 686 | 708901 |

- Container 0 is the header container (root `0xBDBE3B52`) with Version 33, MagicNumber, DLCPackagesBits, CodeChangelist, SaveGameID, IsOptions, SaveGameName, TimePlayed, PlayerGlobalSynch, SaveGameDate (Unix seconds). TimePlayed 152833 / 184773 / 30585 in the three game saves (unit unknown); PlayerGlobalSynch is 100 in all three.
- Gap between containers (16 bytes, as in Liberation): u32 length of everything after this field | u32 value | u32 marker | u32 uncompressed size of the next container. `Save003.sav`: gap 0 to 1 has value 3 and marker `00 fe ca 00`; gap 1 to 2 has value 1 and marker `33 ce da 33`.
- Object and mission ids are 64-bit values (for example Mission `5263052245`, Group `5263052164`), not the 32-bit handles of earlier games. SaveGameName and the language ids in the options file are 64-bit too.

## Game save, container 1
Top-level items, in order: ContextID, NarrativeMissionHistory, SideMissionHistory, CurrentMission, World, PSPState, SnapshotPSPState, PlayerPortal, AnimusDatabaseData, HuntingData, MapFogData, MessagingDatabaseData, UITutorialData, PresentEmailsData, AccomplishmentManagerData, GameStatsSaveData, PlayerProgSaveData, SnapshotPlayerProgSaveData, SnapshotPlayerProgressionDelta, WorldUpgradeSaveData, SnapshotWorldUpgradeSaveData, `0eb2b23c`.

`Save003.sav` has a third container: ContextID and `5251e5ea`, an array of three `b4d5f6fc` entries, each holding `fcf44de9` (3, 1, ...), CurrentMission, CurrentWorld and CurrentPlayerPortal. [likely: one entry per world context, probably the Tyranny of King Washington / Frontier setup]

## Options file, containers 1 to 3
- Container 1: ContextID, PlayerOptionsSaveData, AccomplishmentManagerData, PSPState, UPlayMapUnlocked, UPlaySkinUnlocked, UniqueKeyMap1Unlocked to UniqueKeyMap4Unlocked, CompletedUplayActions.
- Container 2: ContextID, `3e2c66d9`, `5586b7fe`. Container 3 exists only in `Save001.sav`.

## Names
- Most keys resolve through earlier games' names (`Names/names.txt`).
- CRC32 dictionary matches of words and word pairs, each at a matching spot, not seen in a viewer [likely]: AnimalID, BlueprintsDateNumber, ConvoyType, CurrentHealth, CurrentPlayerPortal, IsLocked, LocationIndex, New, NewItemsUnlocked, NewSkillsUnlocked, PlayedCollectiblesGlyph, RankIndex, RegionID, SkillPoints, Status, Type. The ProgressionCharacterData fields (CurrentHealth, SkillPoints, RankIndex, Status, Type, NewItemsUnlocked, NewSkillsUnlocked) sit in the character progression array; IsLocked, LocationIndex and New sit in an array of `8363de15/65d04c04` entries.
- Open: 37 unnamed keys in `Save003.sav`, 51 in the other game saves (`ac3 <file> unknown`).
