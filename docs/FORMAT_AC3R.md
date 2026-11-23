# Assassin's Creed III Remastered (PC) save format

Reader: `Games/Ac3` (`Ac3SaveFile`, the same class as the original game; `IsRemastered` is true when the title starts with `AC3RM[`). CLI: `ac3r <file> summary|flat|unknown|raw` (alias of `ac3`). Read-only.

## Layout
- The layout is the 0x224 header variant of the original game (`FORMAT_AC3.md`): u32 0x224, u32 0, 32 character lowercase hex id at 0x08, UTF-16 title from 0x28, stream from 0x228. [confirmed]
- The title is `AC3RM[<slot>]-tm_<yyyy>_<mm>_<dd>_<hh>_<mm>_<ss>` (underscores in the time, the original uses spaces and colons). [confirmed]
- Slot -1 is the 2 KB profile/options save, slots 0, 1 and 6 are game saves; 6 is the small three-container save of the one-mission file. [likely]
- Encryption is the original game's 16 byte XOR anchored to the file end, key recovered from the known plaintext at the start of the stream. [confirmed]
- The decrypted stream is the Liberation stream (`FORMAT_ACL.md`): version 33, magic 0x00FEDBAC, LZO1x chunked containers, AC2-style property stream, 64-bit object ids. [confirmed]

## Key
- All six saves use the same key `2377b0045802ee899e79c12ef3947573`, although the header ids take three different values (`ab6829c8...`, `fd3eaf6f...`, `a25bf069...`). So the key is not derived from the header id. [confirmed on the six saves]
- Whether the key is the same for other users or storefronts is not known; the library recovers it from every file and never stores it. [guess]
- The original game's keys differ from this one (`FORMAT_AC3.md`).
- The saves in the sample set come from one source, so nothing is known about other accounts. [confirmed]
