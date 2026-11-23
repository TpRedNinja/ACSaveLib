# CLAUDE.md - ACSaveLib

Read this first in any new chat. It is the short version; details are in `docs/`.

## What this is
A .NET 8 library plus CLI that reads (and for AC1 writes) Assassin's Creed save files, for the owner's Route Tracker (WPF). The plugin asks the library what is complete and never touches raw bytes. Repo: https://github.com/TpRedNinja/ACSaveLib

Owner handle: TpRedNinja (do not use a real name anywhere, including credits and licences).

## How the owner works
- Works in normal chats, not agents. Each chat starts cold, so this file and the notes in `docs/` are the memory. Update them whenever something new is found.
- The owner decides what happens next. Do not pick or start the next task, and do not add things to notes or plans they did not ask for.
- Give simple, concrete steps (git has been a friction point: say exactly which commands to run).
- Never hard-code an encryption key for any game. Assume the key that decrypts a save differs for every user (and possibly every storefront) and always recover it at runtime: from known plaintext, or from a user-supplied value such as the legacy username or account id. A key may be fixed in code only when the owner says it is the same across saves of different users, and he has confirmed it on a save from another storefront as well.
- Builds in Visual Studio 2026; WPF cannot be built in the sandbox. Always write `using System.IO;` explicitly in C#.
- ACViewer (owner's viewer) opens saves of AC2 through Mirage (verified by the owner; Valhalla, Black Flag Resynced and Shadows are unverified). It does not open AC1 saves. The owner sends screenshots as ground truth; its Dump button crashes.

## Notes rules (same as the owner's GameNotesGuide.md)
- One statement per fact. Write a new finding as GUESS or LIKELY, then rewrite the same line as confirmed when verified. No duplicate or contradicting lines; keep the higher-confidence one.
- No dead history ("used to be", "was wrong"). The docs state what is currently believed.
- Confirmed = a value decoded from a real save equals what the game or a viewer shows. Do not claim units or meanings without evidence.
- Do not "fix" the game's own spellings (property, class or mission names).

## Layout
- `src/AcSaveLib/` library: `Crypto`, `Compression`, `Container`, `Names`, `Stream` (AC4 property stream), `Games/Ac4`, `Games/Ac1`, `Games/Ac2`, `Games/Acb`, `Games/Acr`, `Games/Acl`, `Games/Ac3`
- `src/AcSaveLib.Cli/` CLI: `ac4 <file> summary|missions|tree|models|unknown|named`, `ac1 <file> summary|roundtrip|models|object|unknown`, `ac2 <file> summary|models|missions|tree|raw|unknown`, `acb <file> summary|models|missions|accomplishments|trainees|tree|flat|unknown|named`, `acr <file> summary|models|tree|flat|unknown`, `acl <file> summary|flat|unknown|raw`, `ac3 <file> summary|flat|unknown|raw` (also `ac3r`), `crc <name>`
- `docs/`: `AGENT_GUIDE.md` (rules and how names were found), `ARCHITECTURE.md`, `BUILDING.md`, `ADDING_A_GAME.md`, `GAMES.md` (game list and status), `FORMAT_*.md`, `FIELDS_*.md`
- `tools/`: reference scripts (`ac4save_reference.py`, `acb_reference.py`, `ac1_sav_full.py`, `brute.c`)

## Build and check
The sandbox may have no dotnet: `apt-get update && apt-get install -y dotnet-sdk-8.0`.
```
mkdir -p /tmp/emptyfeed
dotnet restore --source /tmp/emptyfeed      # sandbox NuGet proxy returns 403
dotnet build --no-restore
dotnet src/AcSaveLib.Cli/bin/Debug/net8.0/AcSaveLib.Cli.dll ac4 <save> models
dotnet src/AcSaveLib.Cli/bin/Debug/net8.0/AcSaveLib.Cli.dll ac1 <save> roundtrip
dotnet src/AcSaveLib.Cli/bin/Debug/net8.0/AcSaveLib.Cli.dll acb <save> models
```
Saves (`*.save`, `*.sav`) and `samples/` are gitignored. Never edit a user save in place; work on copies.

## Workflow for a new game (e.g. AC2)
A Python script is allowed for a first pass at decryption, decompression or the container layout when it helps. Once it handles every provided save, write the C# implementation (the C# reader is the deliverable and must match the script's output). Do not leave the game at the script stage.
1. Ask the owner for several saves from different progress points, plus their notes file for the game (e.g. `AC2_notes.md`) and any hash/name files.
2. Check the header, then encryption (repeating key, recover it from known plaintext), then compression (signature scan), then the object/property layout. AC1 was plain big-endian; AC4 was XOR + chunked LZO + CRC32-keyed properties. A new game may match either family or neither.
3. Make the parser round-trip byte-exact if the game's saves can be written back (it proves the layout is complete).
4. Name properties and IDs from the owner's hash tables first, then CRC32 dictionary matches, then screenshots. A 32-bit CRC match to a meaningful word at a matching spot is strong, but note which names were not seen in a viewer. Note that some hashes are not CRC32 of the exact name (AC1 `MissionStatus`), so keep a hash-to-name table.
5. Add `Games/<Game>/` (file class + models), CLI verb, `docs/FORMAT_<game>.md`, `docs/FIELDS_<game>.md`; update `docs/GAMES.md` and the README status table.
6. Verify on every available save and against ground truth. Re-send changed files or the zip to the owner.

## Current status (update when it changes)
- AC4 Black Flag: decrypt, decompress, parse, typed models for all known fields. Open: layouts of fields that are always empty in samples (MapFogInfo, six UI tutorial lists, community challenge progress, ArmorStates, BackupInventoryItems, WeaponAmmoCountData), ID meaning tables, TimePlayed unit, chunk checksum, 16-byte gap between containers. See `docs/FIELDS_AC4.md`.
- AC1: byte-exact parse/write, mission states, accomplishments (44 achievements), collectibles with flag IDs, names resolved except 3 hashes. Open: Templars-killed index list, meaning of `IsCompleted` vs mission state, configs 0/2/3/4/5 titles. See `docs/FIELDS_AC1.md`.
- AC2: plain file, same CRC32-keyed property stream as AC4 (read-only). Mission history, collectible counters and gained item IDs, accomplishments, money. Stats, villa, animus and PSP names are confirmed in ACViewer. Options/profile names confirmed. Open: ID meaning tables, TimePlayed unit, header id, meaning of sync 0 entries. See `docs/FIELDS_AC2.md`.
- ACB Brotherhood: no encryption, LZO2A chunked containers (same layout as AC4), AC2-style property stream. C# reader `Games/Acb` (read-only) with typed models for every field; output identical to the earlier Python dump on 6 saves and 1 options file. All keys named by `names.txt` except 8 in game saves (and 5 in the options file); ID tables in `docs/tables/`; the 64 trainee names (ids 295888-295915 and 315757-315806) are embedded as `Names/trainee_names.tsv` and resolve through `Names/LocalizationPackage.Builtin`; `acb <file> trainees [xml]` can take a full LocalizationPackage_English.xml instead. Open: meaning of the 8 unnamed keys, Step and ContractAssignment handles. See `docs/FORMAT_ACB.md`, `docs/FIELDS_ACB.md`.
- ACR Revelations: C# reader `Games/Acr` (read-only). Game saves are XORed from 0x228 with a per-user repeating key (reversed legacy username, e.g. `amsirrahrats` for `starharrisma`) aligned to the end of the file, recovered from known plaintext with no user input (key lengths 1 to 16; `--user` is only an override); the options save is plain. Container layout is Brotherhood's; the game data reuses the Brotherhood models plus many unnamed keys. See `docs/FORMAT_ACR.md`.
- ACL Liberation HD: no encryption, LZO1x chunked containers (AC4 codec) with a Brotherhood-style header and AC2-style property stream; items start at offset 26 of a decoded container. C# reader `Games/Acl` (read-only, containers and raw items only, no typed models) parses the one game save and the OPTIONS file from the game's SAVES folder; ship, shop and fog keys named by CRC32 word matches (likely), 38 keys unnamed in the game save, 61 in OPTIONS. ACViewer does not detect these files. See `docs/FORMAT_ACL.md`, `docs/FIELDS_ACL.md`.
- AC3 (original, PC): `.save` files have a plain header of 0x204 or 0x224 bytes and the stream after it, `.sav` files have no header; all are XORed with a 16 byte key anchored to the end of the file that differs per pack (account or storefront) and is recovered from known plaintext, and the decrypted stream is Liberation's (LZO1x containers, AC2-style property stream, 64-bit object ids). C# reader `Games/Ac3` (read-only, reuses `Games/Acl`) parses all eight internet saves (base game, base game plus DLC, and a Sequence 6 save); 16 keys named by CRC32 word matches (likely), 26 to 48 keys unnamed. See `docs/FORMAT_AC3.md`, `docs/FIELDS_AC3.md`.
- AC3R (Remastered, PC): same layout as AC3 original with the 0x224 header (title `AC3RM[slot]-tm_...`), read by `Games/Ac3` (`IsRemastered`), CLI `ac3r`; the key is the MD5 of the Ubisoft account id (confirmed against `ACIII.exe`), recovered from the save with no input, and all six saves share it although their header ids differ; 24 keys named from ACViewer (confirmed) and `FriendRanks` (likely), 31 to 64 unnamed. See `docs/FORMAT_AC3R.md`, `docs/FIELDS_AC3R.md`.
- Other games: not started (see `docs/GAMES.md`; mainline only, list can grow).
