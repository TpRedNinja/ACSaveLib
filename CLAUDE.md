# CLAUDE.md - ACSaveLib

Read this first in any new chat. It is the short version; details are in `docs/`.

## What this is
A .NET 8 library plus CLI that reads (and for AC1 writes) Assassin's Creed save files, for the owner's Route Tracker (WPF). The plugin asks the library what is complete and never touches raw bytes. Repo: https://github.com/TpRedNinja/ACSaveLib

Owner handle: TpRedNinja (do not use a real name anywhere, including credits and licences).

## How the owner works
- Works in normal chats, not agents. Each chat starts cold, so this file and the notes in `docs/` are the memory. Update them whenever something new is found.
- The owner decides what happens next. Do not pick or start the next task, and do not add things to notes or plans they did not ask for.
- Give simple, concrete steps (git has been a friction point: say exactly which commands to run).
- Builds in Visual Studio 2026; WPF cannot be built in the sandbox. Always write `using System.IO;` explicitly in C#.
- ACViewer (owner's viewer) works for AC4 only. It does not open AC1 saves. The owner sends screenshots as ground truth; its Dump button crashes.

## Notes rules (same as the owner's GameNotesGuide.md)
- One statement per fact. Write a new finding as GUESS or LIKELY, then rewrite the same line as confirmed when verified. No duplicate or contradicting lines; keep the higher-confidence one.
- No dead history ("used to be", "was wrong"). The docs state what is currently believed.
- Confirmed = a value decoded from a real save equals what the game or a viewer shows. Do not claim units or meanings without evidence.
- Do not "fix" the game's own spellings (property, class or mission names).

## Layout
- `src/AcSaveLib/` library: `Crypto`, `Compression`, `Container`, `Names`, `Stream` (AC4 property stream), `Games/Ac4`, `Games/Ac1`
- `src/AcSaveLib.Cli/` CLI: `ac4 <file> summary|missions|tree|models|unknown|named`, `ac1 <file> summary|roundtrip|models|object|unknown`, `crc <name>`
- `docs/`: `AGENT_GUIDE.md` (rules and how names were found), `ARCHITECTURE.md`, `BUILDING.md`, `ADDING_A_GAME.md`, `GAMES.md` (game list and status), `FORMAT_*.md`, `FIELDS_*.md`
- `tools/`: reference scripts (`ac4save_reference.py`, `ac1_sav_full.py`, `brute.c`)

## Build and check
```
mkdir -p /tmp/emptyfeed
dotnet restore --source /tmp/emptyfeed      # sandbox NuGet proxy returns 403
dotnet build --no-restore
dotnet src/AcSaveLib.Cli/bin/Debug/net8.0/AcSaveLib.Cli.dll ac4 <save> models
dotnet src/AcSaveLib.Cli/bin/Debug/net8.0/AcSaveLib.Cli.dll ac1 <save> roundtrip
```
Saves (`*.save`, `*.sav`) and `samples/` are gitignored. Never edit a user save in place; work on copies.

## Workflow for a new game (e.g. AC2)
1. Ask the owner for several saves from different progress points, plus their notes file for the game (e.g. `AC2_notes.md`) and any hash/name files.
2. Check the header, then encryption (repeating key, recover it from known plaintext), then compression (signature scan), then the object/property layout. AC1 was plain big-endian; AC4 was XOR + chunked LZO + CRC32-keyed properties. A new game may match either family or neither.
3. Make the parser round-trip byte-exact if the game's saves can be written back (it proves the layout is complete).
4. Name properties and IDs from the owner's hash tables first, then CRC32 dictionary matches, then screenshots. A 32-bit CRC match to a meaningful word at a matching spot is strong, but note which names were not seen in a viewer. Note that some hashes are not CRC32 of the exact name (AC1 `MissionStatus`), so keep a hash-to-name table.
5. Add `Games/<Game>/` (file class + models), CLI verb, `docs/FORMAT_<game>.md`, `docs/FIELDS_<game>.md`; update `docs/GAMES.md` and the README status table.
6. Verify on every available save and against ground truth. Re-send changed files or the zip to the owner.

## Current status (update when it changes)
- AC4 Black Flag: decrypt, decompress, parse, typed models for all known fields. Open: layouts of fields that are always empty in samples (MapFogInfo, six UI tutorial lists, community challenge progress, ArmorStates, BackupInventoryItems, WeaponAmmoCountData), ID meaning tables, TimePlayed unit, chunk checksum, 16-byte gap between containers. See `docs/FIELDS_AC4.md`.
- AC1: byte-exact parse/write, mission states, accomplishments (44 achievements), collectibles with flag IDs, names resolved except 3 hashes. Open: Templars-killed index list, meaning of `IsCompleted` vs mission state, configs 0/2/3/4/5 titles. See `docs/FIELDS_AC1.md`.
- Other games: not started (see `docs/GAMES.md`; mainline only, list can grow).
