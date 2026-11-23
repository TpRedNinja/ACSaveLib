# Revelations (ACR) save fields

Layout, encryption and containers are in `FORMAT_ACR.md`. The reader is `Games/Acr` (read-only).

## Named fields
- The main game container (root `0x94D6F8F1`) is read with Revelations' own models (`Games/Acr/AcrGameData.cs`, `AcrModels.cs`, `AcrGuildModels.cs`, `AcrProgressModels.cs`; same layout and names as Brotherhood): mission histories, PSP state, Animus entries, tutorials, emails, accomplishments, game stats, trainees guild, progression, villa, world upgrade. Names come from `Names/names.txt`.
- Header: TimePlayed and global synchronization are in the header container; header TimePlayed equals GameStatsSaveData TimePlayed.

## Names
- 20 names are exact CRC32 matches of strings in `ACRSP.exe`; more came from two-word matches of exe-derived words. See the Names section of `FORMAT_ACR.md` for confidence.
- Not seen in a viewer yet: no Revelations field is confirmed against the game.

## Open
- Unnamed keys (`acr <file> unknown`): 0 in the two game saves, 48 in the options save.
- Meaning of containers 2 and 3 in game saves (roots `0x409E7FAE`, `0x3EBD922B`).
- Options save containers 3-5 (roots `0xC9876D66`, `0xB4B55039`, `0xDBEB7469`) are unnamed.
- Typed models for Revelations-only structures (bombs, crafting, Den defence, Penrose, dice game).
- ID tables for World and portal ids (the Brotherhood forge indexes do not resolve them).
