# Assassin's Creed Syndicate (PC) save format

Reader: `Games/Acs/AcsSaveFile.cs` (read-only: key recovery, decryption, decompression, property stream, typed header and typed game or options data), CLI verb `acs <file> summary|header|models|tree|flat|unknown`. Fields are in `FIELDS_ACS.md`.

## Files
- The six saves read are one account's, from the game's Ubisoft `savegames\<account id>\1957` folder: `1.save` and `2.save` (slot 3 options saves, titles `ACSyndicate[3]` and `ACSyndicate[53]`), `3.save` and `4.save` (game saves, slots 0 and 50, byte-identical after the header), `5.save` and `6.save` (game saves, slots 2 and 52, with the Jack the Ripper DLC bit set). [confirmed]
- A slot number plus 50 is a copy of the same save: the bytes after the header are identical and only the title slot differs. [confirmed on 3/4 and 5/6]
- The title is UTF-16 at offset `0x28` and is built as `ACSyndicate[<slot>]-tm_<yyyy>_<mm>_<dd>_<hh>_<mm>_<ss>`. [confirmed]
- Plain header of `0x228` (552) bytes, the same as Unity: u32 `0x224` | u32 `0` | 32 ASCII characters at `0x08` | UTF-16 title from `0x28`. [confirmed]

## Encryption
- From offset 552 to the end the data is XORed with a 16 byte key: byte `i` uses `key[(i - fileLength) mod 16]`. This is the Black Flag and Unity cipher (`XorTailCipher`). [confirmed]
- The key is the MD5 of the Ubisoft account id string (lowercase, dashed GUID text, the folder name under `savegames`), with the 16 MD5 bytes placed as `key[i] = md5[(16 - i) mod 16]`. The MD5 of the owner's account id equals the key recovered from all six saves, and it is the same key as the owner's Unity saves. [confirmed]
- The reader recovers the key from known plaintext (version `42`, magic `0x00FEDBAC`, then the container signature and the container version `02 00 00 00`) and never fixes it in code. [confirmed]
- The key derivation function in `ACS.exe` has not been looked at.

## Stream and containers
- Decrypted, the stream is: u32 version `42` | u32 `0x00FEDBAC` | u32 | u32 | u32 (uncompressed size of the header container) | first container at 572, as in Unity. [confirmed]
- Containers are chunked LZO1x (chunk size 32768) with the Black Flag layout (`ChunkContainer`): signature `33 AA FB 57 99 FA 04 10`, u16 container version, u8 codec, u32 chunk size, then blocks of marker `01`, compressed size, uncompressed size, checksum, data. The container version is `2` in Syndicate (Unity `1`); every block of the six saves decodes to its stated size. [confirmed]
- The container writer in `ACS.exe` (function at `0x142310ED0` in a memory dump) writes the signature, the version `2`, the codec byte and the chunk size, and for a flag it also writes a table of block sizes; the saves read have no such table. [confirmed for the header fields, likely for the optional table]
- Game saves have 2 containers (header, main). Options (slot 3) saves have 3: the header, a global profile container (`PlayerOptionsSaveData`, `OnlineProfileSaveData`) and `SinglePlayerOptionsSaveData`. [confirmed]
- Each container is a property stream with items starting at offset 26, as in Unity. [confirmed]

## Header
- Items: `Version`, `MagicNumber`, `DLCPackagesBits`, `CodeChangelist`, `SaveGameID`, `IsOptions`, `SaveGameName`, `TimePlayed`, `PlayerGlobalSynch`, `SaveGameDate`. [confirmed]
- `DLCPackagesBits` is `0x82800` in the base game saves and `0x182800` in the saves with the Jack the Ripper DLC. [likely]
- `CodeChangelist` is 9414067 in all six saves. `SaveGameDate` is Unix seconds in UTC and matches the title time. [confirmed]
- The options saves have `SaveGameID` 3, `IsOptions` 1 and `SaveGameName` `Options`; their `TimePlayed` and `SaveGameDate` are 0. [confirmed]

## Open
- 10 distinct keys are unnamed over the six saves (`acs <file> unknown`): 4 in an options save, 6 in the game saves; 2 of them are `Property_0x` keys in ACViewer. See `FIELDS_ACS.md`.
- Layouts of fields that are empty in the samples; the key derivation function in `ACS.exe`.
