# AC1 (Assassin's Creed, 2007, PC) .sav format
Source: the owner's AC1_notes.md section 10 (byte-exact parser `tools/ac1_sav_full.py`) and Blox's AC1SaveTool. Implemented in
`src/AcSaveLib/Games/Ac1/Ac1SaveFile.cs`. CONFIRMED: parse + write is byte-exact on 8 saves (Parse then Write equals the input; `ac1 <file> roundtrip`).
No encryption, no compression. Everything is BIG-endian.

```
u32 fileSize (= file length) | u32 magic 0x1234FEDC | u16 version (15) | u32 objectCount
object: u32 classID | u32 fieldCount
        fieldCount x { u8 isArray | u32 pathLength | pathLength x { u32 nameHash, u16 index } }
        u32 metadataCount (= fieldCount) | metadataCount x { u32 typeWord, u32 enumType }
        u32 valueAreaSize (in-memory size, not the file byte count)
        values, one per field in field order, no padding
```
- A field is a PATH of property-name hashes down to one leaf value (e.g. `Configs[1].Accomplishments[5].Monitor.UsedEvents`). Index 65535 = not indexed.
- Name hash = CRC32 (zlib) of the exact property name (same function as AC4; verified: CRC32("UsedEvents") = 2005069724).
- Class IDs: 2092 MapManager, 2093 DNAMemoryManager, 3003 SaveGameDataManager, 3004 HorseData, 3008 Persistence, 3009 TutorialManager,
  3010 Accomplishment Manager. Any other class ID is a mission item (a hash ID; the name hash is NOT CRC32 of the item name).
- Field type = (typeWord >> 16) & 0x3F; element type (arrays) = (typeWord >> 23) & 0x3F. Byte sizes: types 0-3 = 1, 4-5 = 2, 6/7/10/17/18/20/21/25-28 = 4,
  8/9/11 = 8, 12 = 12, 13/14 = 16, 15 = 36, 16 = 64, 30-38 = 257/257/514/1028/2056/1028/4100/1040/1040.
- Arrays (types 29 and 24): `u32 length` then `length` elements. Length is an ELEMENT COUNT (not bytes). Type 23 is a fixed array (count = typeWord & 0x7FFF, no length).
- enumType is non-zero only for enum fields (type 25): CRC32 of the enum type name. 0x44B57418 MissionItemState, 0xEACDC2B9 Accomplishment state,
  0xF0E64C53 bracket-monitor state, 0xF09BB115 / 0xF9688796 tolerance modes, 0x7E394986 MapMarker::IconType.
- Not confirmed: whether types 6 and 7 are signed.

## Mission items
Every mission item has the enum field (enumType 0x44B57418) = state: 0 locked, 1 available, 2 active, 3 done, plus two bool fields
(hashes 523ad0c1, 6dcf11f9; names unknown). 939 items. Some have extra fields (Marker.IsKnown, bureau/assassination flags).

## Accomplishment Manager (3010)
`Configs[c].Accomplishments[a]` with `.State` (enum 0xEACDC2B9), `.Monitor.IsResolved`, `.Monitor.Count` (counter monitors), `.Monitor.UsedEvents` (byte array of collected indices).
Config 1 is the 44 achievements (slot order in Ac1Achievements). State 2 = completed, 1 = in progress: checked on 3 saves (fresh save: all 1; 100% save: 38 of 44 at 2).
Caveat seen in the 100% save: slot 24 (Eagle's Will) has State 2 but IsResolved false and Count 0, so use State, not IsResolved, for "completed".
Collectible slots (config 1, confirmed in the owner's notes 8.3): Kingdom 3, Masyaf 4, Jerusalem 5, Damascus 6, Acre Teutonic 38, Acre Templar 39, Acre Hospitalier 40, Templars killed 2.
Collected count = UsedEvents length (100% save: 100/20/100/100/33/34/33/60).
