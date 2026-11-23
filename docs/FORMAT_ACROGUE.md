# Assassin's Creed Rogue (PC) save format

## Files
- Game saves (`3.save` to `8.save` in the Ubisoft savegames folder `...\savegames\<account id>\934`) and profile saves (`1.save`, `2.save`) share one layout; the profile saves are about 3 KB. [confirmed]
- The title is UTF-16 at offset 4 and is built as `ACC[<slot>]-pr_<id>-tm_<yyyy_mm_dd hh:mm:ss>` (function `Save_MakeTitle`, `0x14000B560` in `ACC.exe`). [confirmed]
- The first 552 bytes (0x228) are plain (title and padding); the rest is encrypted. [confirmed]

## Encryption
- The file is XORed from offset 552 to the end with a 16 byte key, indexed by the number of bytes remaining: byte `i` uses `key[(fileLength - i) & 0xF]`. [confirmed]
- This is the same cipher as AC4 (`XorTailCipher`) with the key listed in the opposite order (AC4 code indexes `key[(i - fileLength) mod 16]`). [confirmed]
- The decrypted data starts at 552 with version `0x24` (36; Black Flag is 35), magic `0x00FEDBAC` and 8 zero bytes, so the key can be recovered from known plaintext like AC4. [confirmed]
- The save code is `Save_ReadSlot` (`0x1400199D0`) and `Save_WriteSlot` (`0x1400195A0`); both XOR the whole buffer with the 16 bytes at `this+100`. [confirmed]

## The game key
- The key is the MD5 of the first Uplay CD key string with a leading `ORBIT` removed (`Save_GetUplayCdKeys` `0x14001EDB0` builds the string, `Save_MakeKeyFromCdKey` `0x14000B4D0` hashes it and stores the 16 bytes at `this+100`). [confirmed]
- In all 8 saves tested the key is the MD5 of the empty string: `d41d8cd98f00b204e9800998ecf8427e`, used with the index above. The saves come from two sources: the owner's Ubisoft savegames folder and a downloaded save pack. [confirmed]
- The first CD key string is therefore empty (or only `ORBIT`) for these saves. [likely]
- Saves from an account that returns a real CD key would use a different key; the library must recover the key from the known plaintext, not hard-code it. [likely]

## Header and container
- Decrypted offset 576 starts the first chunk container with the same layout as AC4 (`33 aa fb 57 99 fa 04 10 01 00 00 00 ...`). [confirmed]
- The AC4 reader (`Games/Ac4`) does not yet read these saves because its known plaintext expects version 35. [confirmed]
- Decompression and the property stream are not checked yet. [guess]
