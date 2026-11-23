# Assassin's Creed III Remastered (PC) save format

Reader: `Games/Ac3` (`Ac3SaveFile`, the same class as the original game; `IsRemastered` is true when the title starts with `AC3RM[`). CLI: `ac3r <file> summary|flat|unknown|raw` (alias of `ac3`). Read-only.

## Layout
- The layout is the 0x224 header variant of the original game (`FORMAT_AC3.md`): u32 0x224, u32 0, 32 character lowercase hex id at 0x08, UTF-16 title from 0x28, stream from 0x228. [confirmed]
- The title is `AC3RM[<slot>]-tm_<yyyy>_<mm>_<dd>_<hh>_<mm>_<ss>` (underscores in the time, the original uses spaces and colons). [confirmed]
- Slot -1 is the 2 KB profile/options save, slots 0, 1 and 6 are game saves; 6 is the small three-container save of the one-mission file. [likely]
- Encryption is the original game's 16 byte XOR anchored to the file end, key recovered from the known plaintext at the start of the stream. [confirmed]
- The decrypted stream is the Liberation stream (`FORMAT_ACL.md`): version 33, magic 0x00FEDBAC, LZO1x chunked containers, AC2-style property stream, 64-bit object ids. [confirmed]

## Key
- The key is the 16 byte MD5 of the Ubisoft Connect account id string (lowercase, dashed GUID text as `UPLAY_USER_GetAccountIdUtf8` returns it), stored in the save object and used by the game's save writer (`ACIII.exe` `sub_140A98970`, MD5 init constants 0x67452301...). [confirmed: MD5 of the owner's account id gives the recovered key]
- The writer XORs `data + CRC32` with `md5[(remaining) & 0xF]`, where `remaining` counts down from the data length plus 4 to 1. In the library's convention (index `(i - fileLength) mod 16`) the key is `key[i] = md5[(16 - i) mod 16]`, so the recovered key `2377b0045802ee899e79c12ef3947573` is the MD5 `23737594f32ec1799e89ee025804b077` read this way. [confirmed]
- All six sample saves use this one key although their header ids take three values (`ab6829c8...`, `fd3eaf6f...`, `a25bf069...`), so the header id is not the account id and the key does not depend on it. [confirmed]
- The key is therefore per account; the library still recovers it from each file's known plaintext and never stores it. [confirmed]
- Original AC3 keys are a different value from this account's (`FORMAT_AC3.md`); whether they derive from a Uplay value the same way is not known. [guess]
