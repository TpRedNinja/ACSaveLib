# Assassin's Creed Freedom Cry (standalone, PC)

Saves are read by the existing AC4 reader (`Games/Ac4`, CLI `ac4`).

## Files
- `1.save` is the profile save (title `AC4BF[-1]-tm_...`, 3253 bytes). [confirmed]
- `2.save` is the game save of slot 0 (title `AC4BF[0]-tm_...`, 51952 bytes). [confirmed]
- The title prefix is `AC4BF`, the same as Black Flag, so the header does not tell the two games apart. [confirmed]

## Format
- Both saves decrypt with the same 16 byte key recovered from known plaintext, and decompress and parse with the AC4 reader with no changes. [confirmed]
- The game save has 0 unknown keys with the current `names.txt`. [confirmed]
- The profile save has 11 unknown keys left (of 19): `921b3ca7`, `fb6f6396`, `d5ebf0e5`, `2fd79414`, `fa124f25`, `5616caee` (a per-item flag next to `IsFound` and `IsCompleted`, always 0) and the five hashes `5b95f10b`, `2a4e8a90`, `e2fd32bc`, `82768a63`, `832dff4e`. [confirmed]
- The five options hashes are the same as in the AC3R profile `SinglePlayerOptionsSaveData` (see `docs/FIELDS_AC3R.md`). [confirmed]

## Profile names
- `86484f04` is `ProductViewingInfo`, `61903edf` is `DiscoverySharingItemData` and `7417fa16` is `OnlineGameplaySaveData` (CRC32 matches to strings in `ACFC.exe`, not seen in a viewer). [likely]
- `20dfc649` is `Items`, an array of 108 `DiscoverySharingItemData` inside `OnlineGameplaySaveData`. [likely]
- Each item has `GlobalEventID` (`7a211b1e`, 0xFFFFFFFF in the sample), `Id` (`2abd43f2`, counting up from 232), `IsFound` (`962493d4`) and `IsCompleted` (`85fc627e`), both 0 in the sample, and `Category` = 0. [likely]
