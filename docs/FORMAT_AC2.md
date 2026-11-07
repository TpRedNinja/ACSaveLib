# AC2 (Assassin's Creed II, PC) .save format
Implemented in `src/AcSaveLib/Games/Ac2/Ac2SaveFile.cs`. Parse is read-only. CONFIRMED on 33 saves (Steam and pirated/SKIDROW builds, all story sequences, Free Roam, 100%): every item of the property stream parses, 0 unparsed items (`ac2 <file> raw`).
No encryption, no compression. Little-endian.

```
u32 headerLength | header (headerLength bytes) | property stream
header:   4 bytes | [32-char ASCII id] | UTF-16 title "ACII N" | zero padding
stream:   u32 version (17) | u32 magic 0x00FEDBAC | 16 byte preamble | root key u32 | u32 size | u32 size | items
```
- headerLength is 0x224 (stream at 0x228) in Steam saves and 0x204 (stream at 0x208) in SKIDROW saves. One SKIDROW sample (`4.save`) has a headerLength that is 2 bytes too long: the library finds the magic `AC DB FE 00` when the computed position does not hold it.
- Title is at 0x28 when a 32-char ASCII id sits at 0x08 (the id is not the slot, several slots share one id), otherwise at 0x08. N is the slot number minus 1 (slot 1 = "ACII 0", slot 3 = "ACII 2"); options-only saves hold "ACII -1" (CONFIRMED: Sequence 1-slot 1/2 and Sequence 2 save slot 3 read 0/1/2). The digit appears once in the file and not in the property stream. Rewriting "-1" with one digit shortens the file by 2 bytes and shifts the stream.
- The root object key 0xBDBE3B52 is `SaveGame`. Its items start 36 bytes after the stream start (`RootItemsOffset`).
- The stream uses the item framing of `FORMAT_PROPERTY_STREAM.md` (`u32 L | key | ext | 00 00 | type | 0b | value`, CRC32 property names, same name file as AC4). AC2 differences, handled by `PropertyReader.Parse(..., ac2Layout: true)`:
  - Objects (type 0x13 / 0x16): the second copy of the key is at body+23 instead of body+27; children start at body+35.
  - Fixed packed arrays of type 0x0017 (seen: 6 x u8): `u32 count` at body+13, then count elements; the element size is payload / count.
- Root items: header properties (Version, MagicNumber, DLCPackagesBits, CodeChangelist, SaveGameID, IsOptions, SaveGameName, CurrentWorld, CurrentMission, TimePlayed, PlayerGlobalSynch, SaveGameDate), then `SaveGameObjects` (empty) and `SaveGameDataObjects`, an array whose elements are `AssassinSaveGameData` (the game) and/or `AssassinGlobalProfileData` (options). The 2 KB saves hold only the profile.
