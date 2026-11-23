# Assassin's Creed Freedom Cry (standalone, PC)

Saves are read by the existing AC4 reader (`Games/Ac4`, CLI `ac4`).

## Files
- `1.save` is the profile save (title `AC4BF[-1]-tm_...`, 3253 bytes). [confirmed]
- `2.save` is the game save of slot 0 (title `AC4BF[0]-tm_...`, 51952 bytes). [confirmed]
- The title prefix is `AC4BF`, the same as Black Flag, so the header does not tell the two games apart. [confirmed]

## Format
- Both saves decrypt with the same 16 byte key recovered from known plaintext, and decompress and parse with the AC4 reader with no changes. [confirmed]
- The game save has 0 unknown keys with the current `names.txt`. [confirmed]
- The profile save has 19 unknown keys: `5b95f10b`, `2a4e8a90`, `e2fd32bc`, `82768a63`, `832dff4e` (the same hashes as the AC3R profile `SinglePlayerOptionsSaveData`), and `921b3ca7`, `fb6f6396`, `d5ebf0e5`, `2fd79414`, `86484f04`, `7417fa16`, `20dfc649`, `fa124f25` plus six keys (`2abd43f2`, `5616caee`, `61903edf`, `7a211b1e`, `85fc627e`, `962493d4`) that occur 108 times each. [confirmed]
