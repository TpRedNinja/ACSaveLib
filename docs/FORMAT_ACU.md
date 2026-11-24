# Assassin's Creed Unity (PC) save format

Reader: `Games/Acu/AcuSaveFile.cs` (read-only: key recovery, decryption, decompression, property stream, typed header), CLI verb `acu <file> summary|header|tree|flat|unknown`. No typed models for the game data yet.

## Files
- The five saves read are one account's: two game saves of different progress (slot 0), one more game save, and two slot 3 saves that hold the options. [confirmed]
- The title is UTF-16 at offset `0x28` and is built as `AC4BF[<slot>]-tm_<yyyy>_<mm>_<dd>_<hh>_<mm>_<ss>` (the prefix `AC4BF` and the format `%d]-tm` are strings in `ACU.exe`). The time is local time. [confirmed]
- Plain header of `0x228` (552) bytes: u32 `0x224` | u32 `0` | 32 ASCII characters at `0x08` (a hex id in some saves, zeros in others) | UTF-16 title from `0x28`. [confirmed]

## Encryption
- From offset 552 to the end the data is XORed with a 16 byte key: byte `i` uses `key[(i - fileLength) mod 16]`. This is the Black Flag cipher (`XorTailCipher`). [confirmed]
- The key is the same in all five saves. The reader recovers it from known plaintext and never fixes it in code. [confirmed]
- Known plaintext: u32 version `42` and the magic `0x00FEDBAC` at 552, then the container signature `33 aa fb 57 99 fa 04 10` and `01 00 00 00` at 572. The two pieces together cover all 16 key positions. [confirmed]
- The key is the MD5 of the Ubisoft account id string (lowercase, dashed GUID text, the folder name under `savegames`), with the 16 MD5 bytes placed as `key[i] = md5[(16 - i) mod 16]` in the convention above. It is the same value as the AC3 Remastered key of the same account (`FORMAT_AC3R.md`). [confirmed: MD5 of the owner's account id gives the recovered key]
- The 32 character id in the save header is not the account id: its MD5 does not give the key. [confirmed]
- The key is therefore per account; the reader still recovers it from each file's known plaintext and never stores it. [confirmed]
- ACViewer decrypts Unity saves when given the account id, which fits the derivation. [confirmed by the owner]

## Stream and containers
- Decrypted, the stream is: u32 version `42` (Black Flag 35, Rogue 36) | u32 `0x00FEDBAC` | u32 (differs per slot type, meaning unknown) | u32 205, 203 or 213 | u32 237 or 245 (the uncompressed size of the header container) | first container at 572. [confirmed]
- Unlike Black Flag, the 8 bytes after the magic are not zero and the first container starts at 572, not 576. [confirmed]
- Containers are chunked LZO1x (chunk size 32768) as in Black Flag (`ChunkContainer`). Every block of the five saves decodes to its stated size. [confirmed]
- Game saves have 2 containers: the header and the main stream. Slot 3 (options) saves have 3: the header, a global profile container (`PlayerOptionsSaveData`, `AccomplishmentManagerData`, `OnlineProfileSaveData` and others) and `SinglePlayerOptionsSaveData`. [confirmed]
- Each container is a property stream with items starting at offset 26, the same layout as Black Flag. The reader takes the last container as `Stream` and the middle container of an options save as `ProfileRoot`. [confirmed]

## Header
- Items: `Version`, `MagicNumber`, `DLCPackagesBits` (`0x600` in all five saves), `CodeChangelist`, `SaveGameID`, `IsOptions`, `SaveGameName`, `TimePlayed`, `PlayerGlobalSynch`, `SaveGameDate`. [confirmed]
- `SaveGameDate` is Unix seconds in UTC and matches the title time. [confirmed]
- `CodeChangelist` is 1063710 (`0x103B1E`) in `Any% Done` and `Any% start`, and 1102304 (`0x10D1E0`) in the `50%+` saves. A slot 3 save written on 2026-11-24 shows 9523670, so the older values are older builds. [likely]
- Game saves have `SaveGameID` 0 and `IsOptions` 0. Slot 3 saves have `SaveGameID` 3, `IsOptions` 1 and `SaveGameName` `Options`. [confirmed]

## Open
- Property names: the main stream parses and most top-level names resolve from `names.txt`. `acu <file> unknown` lists 153 (`50%+ 1`), 135 (`Any% start 1`), 150 (`Any% Done`) and 37 (each options save) keys without a name.
- Typed models for the game data.
