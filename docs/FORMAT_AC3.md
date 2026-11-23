# AC3 (Assassin's Creed III, original PC) save format
Reader: `Games/Ac3/Ac3SaveFile.cs` (C#, read-only), CLI verb `ac3`. CONFIRMED on eight files from the internet (not the owner's own saves): three `.save` files of a "100%" base-game pack (2012), three `.sav` files of a "100% base game and DLC" pack (`ACIII and TOKW 100 Save File`, 2026) and two `.save` files of a "Sequence 6" pack (2017 and 2025, another player). Every container decodes and every property item parses with no leftover bytes. ACViewer was not used, so nothing is checked against a viewer.

The decrypted stream is the Liberation HD one (`FORMAT_ACL.md`): u32 33, u32 `0x00FEDBAC`, 8 zero bytes, u32 length, u32 uncompressed size, then LZO1x chunked containers with a CRC32-keyed property stream. `Ac3SaveFile` decrypts and hands the stream to `AclSaveFile`.

## Encryption and header variants
- Encryption is a repeating 16 byte XOR anchored to the END of the file (the AC4 scheme, `XorTailCipher`). The stream starts with known plaintext (`u32 33`, `ac db fe 00`, 8 zero bytes = one full key period), so `Ac3SaveFile` recovers the key from it; no key is stored in the library.
- The key differs between the three packs and is the same for every file of a pack (any file type, any date):

| Pack | Files | Header | Key (index `(i - fileLength) mod 16`) |
| --- | --- | --- | --- |
| 2012 "100%" base game | `1.save`, `2.save`, `3.save` | plain 0x204 bytes, stream from `0x208` | `0154a57f32bb4407857877ed29423b2e` |
| Sequence 6 | `1.save`, `2.save` (2025 and 2017) | plain 0x224 bytes, stream from `0x228` | `0a2abb77fab80caad1af78519d683e59` |
| "100% base game and DLC" | `Save001.sav` to `Save003.sav` | none, stream from offset 0 | `48ee7a4e94f30deeb36f2781ec1108d8` |

- AC4 saves show the same pattern (`FORMAT_AC4.md`): a different key per source, the same key within a source over years. So the key looks like a per-account or per-storefront game key, not one constant for the game. [likely]
- Whether the header length (0x204, 0x224, none) depends on the game build or the storefront is not known.
- Headers 0x204 and 0x224 start with u32 header length | u32 0 | data | zero padding | 4 zero bytes (not encrypted) | stream. Header 0x204 has the UTF-16 title from `0x08` (`AC3[N]-tm_<yyyy>_<mm>_<dd> <hh>:<mm>:<ss>`, N = save slot). Header 0x224 has a 32 character ASCII id at `0x08` (different in every file) and the title from `0x28`. The headerless files have no title; the titles are in `saves.ini` next to the files (`Save001=AC3[-1]-tm_...`).
- Save slot N in the title: -1 is the options file (settings, and the list of owned DLC [guess]); 0 is the top save slot. The base game and Tyranny of King Washington each have 3 save slots; how slots 1 to 5 map to them is not worked out (`Save003.sav` is slot 4).
- AC3 Remastered is reported to key its saves with an account id instead. [guess; no Remastered saves yet]

## Containers
| File | Slot | Containers (decoded bytes) | Header DLCPackagesBits | CodeChangelist |
| --- | --- | --- | --- | --- |
| `1.save`, `2.save` | -1 (options) | 245, 1665 / 1669, 547 | 2 | 657502 |
| `3.save` | 0 | 245, 931466 | 2 | 657502 |
| Sequence 6 `1.save` | 0 | 245, 410369 | 0 | 4294967295 |
| Sequence 6 `2.save` | -1 (options) | 245, 1605, 547 | 0 | 708901 |
| `Save001.sav` | -1 (options) | 245, 1701, 547, 3312 | 686 | 708901 |
| `Save002.sav` | 0 | 245, 1289765 | 686 | 708901 |
| `Save003.sav` | 4 | 245, 508336, 454 | 686 | 708901 |

- Container 0 is the header container (root `0xBDBE3B52`) with Version 33, MagicNumber, DLCPackagesBits, CodeChangelist, SaveGameID, IsOptions, SaveGameName, TimePlayed, PlayerGlobalSynch, SaveGameDate (Unix seconds). TimePlayed 152833 / 184773 / 30585 in the three 100% game saves and 12785 in the Sequence 6 save (unit unknown); PlayerGlobalSynch is 100 in the three 100% saves and 16 in the Sequence 6 save.
- Gap between containers (16 bytes, as in Liberation): u32 length of everything after this field | u32 value | u32 marker | u32 uncompressed size of the next container. `Save003.sav`: gap 0 to 1 has value 3 and marker `00 fe ca 00`; gap 1 to 2 has value 1 and marker `33 ce da 33`.
- Object and mission ids are 64-bit values (for example Mission `5263052245`, Group `5263052164`), not the 32-bit handles of earlier games. SaveGameName and the language ids in the options file are 64-bit too.

## Game save, container 1
NarrativeMissionHistory lists its groups in progression order and the history is cumulative: the Sequence 6 save has the first 10 groups of the 100% base save (same ids, same order, later ones missing), and the last two of them (`5263052186`, `5449512666`) hold 1 mission each instead of 8 and 7. [likely]

Top-level items, in order: ContextID, NarrativeMissionHistory, SideMissionHistory, CurrentMission, World, PSPState, SnapshotPSPState, PlayerPortal, AnimusDatabaseData, HuntingData, MapFogData, MessagingDatabaseData, UITutorialData, PresentEmailsData, AccomplishmentManagerData, GameStatsSaveData, PlayerProgSaveData, SnapshotPlayerProgSaveData, SnapshotPlayerProgressionDelta, WorldUpgradeSaveData, SnapshotWorldUpgradeSaveData, `0eb2b23c`.

`Save003.sav` has a third container: ContextID and `5251e5ea`, an array of three `b4d5f6fc` entries, each holding `fcf44de9` (3, 1, ...), CurrentMission, CurrentWorld and CurrentPlayerPortal. [likely: one entry per world context, probably the Tyranny of King Washington / Frontier setup]

## Options file, containers 1 to 3
- Container 1: ContextID, PlayerOptionsSaveData, AccomplishmentManagerData, PSPState, UPlayMapUnlocked, UPlaySkinUnlocked, UniqueKeyMap1Unlocked to UniqueKeyMap4Unlocked, CompletedUplayActions.
- Container 2: ContextID, `3e2c66d9`, `5586b7fe`. Container 3 exists only in `Save001.sav`.

## Names
- Most keys resolve through earlier games' names (`Names/names.txt`).
- CRC32 dictionary matches of words and word pairs, each at a matching spot, not seen in a viewer [likely]: AnimalID, BlueprintsDateNumber, ConvoyType, CurrentHealth, CurrentPlayerPortal, IsLocked, LocationIndex, New, NewItemsUnlocked, NewSkillsUnlocked, PlayedCollectiblesGlyph, RankIndex, RegionID, SkillPoints, Status, Type. The ProgressionCharacterData fields (CurrentHealth, SkillPoints, RankIndex, Status, Type, NewItemsUnlocked, NewSkillsUnlocked) sit in the character progression array; IsLocked, LocationIndex and New sit in an array of `8363de15/65d04c04` entries.
- Open: 3 unnamed keys in `Save003.sav`, 0 in the other large game saves, 45 in the small profile saves (`ac3 <file> unknown`).
