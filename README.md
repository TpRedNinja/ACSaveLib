# AcSaveLib

A .NET library (plus CLI) that decrypts, decompresses and parses Assassin's Creed save files, for use by the
Route Tracker plugins. Goal: one library for the mainline Assassin's Creed games (list below, can grow later; side games such as Chronicles are not planned). Started with **AC4 Black Flag**.

Same idea as the Arkham route tracker's save library: the plugin asks the library "which missions are complete?"
and never touches raw bytes.

## Status
Mainline games in release order. Short name, full name, then progress.

| Short | Game | Encryption | Compression | Property stream | Data decoded |
|---|---|---|---|---|---|
| AC1 | Assassin's Creed | no encryption | no compression | done (byte-exact) | mission states, accomplishments, collectibles; names resolved (3 hashes open), see docs/FIELDS_AC1.md |
| AC2 | Assassin's Creed II | no encryption | no compression | done (same CRC32-keyed stream as AC4, read-only) | mission history, collectible counters and item IDs, accomplishments, money; see docs/FIELDS_AC2.md |
| ACB | Assassin's Creed Brotherhood | no encryption | LZO2A | done (AC2-style items, read-only) | missions, accomplishments, stats, trainees and contracts, player progression, economy; see docs/FIELDS_ACB.md |
| ACR | Assassin's Creed Revelations | options save plain, game saves XOR with a per-user repeating key (recovered from known plaintext, no input needed) | LZO2A (same container as ACB) | read-only reader (`acr` CLI) | see docs/FORMAT_ACR.md |
| AC3 | Assassin's Creed III (original, PC) | XOR, 16 byte key anchored to the file end (differs per pack, recovered from known plaintext) | LZO1x chunked containers (the Liberation stream) | read-only reader (`ac3` CLI), raw items only | eight internet saves decoded (three header variants); 26 to 48 keys unnamed; see docs/FORMAT_AC3.md |
| AC3R | Assassin's Creed III Remastered | XOR, 16 byte key anchored to the file end (recovered from known plaintext) | LZO1x chunked containers (the Liberation stream) | read-only reader (`ac3r` CLI, shares `Games/Ac3`), raw items only | six saves decoded; 0 keys unnamed in game saves, 11 in the profile save; see docs/FORMAT_AC3R.md |
| ACL | Assassin's Creed Liberation (original, HD on PC) | no encryption | LZO1x | read-only reader (`acl` CLI), raw items only | one game save and OPTIONS decoded; 38 keys unnamed in the game save, 61 in OPTIONS; see docs/FORMAT_ACL.md |
| ACLR | Assassin's Creed Liberation Remastered (its saves share the format of AC3R, to be verified) | - | - | - | not started |
| AC4 | Assassin's Creed IV Black Flag | XOR with a 16 byte key anchored to the file end (a game key that differs between accounts or storefronts, recovered from known plaintext) | LZO1x chunked containers | CRC32-keyed property stream with typed models | missions (Narrative + Side) and typed models for all known fields, see docs/FIELDS_AC4.md |
| ACRogue | Assassin's Creed Rogue | - | - | - | not started |
| ACU | Assassin's Creed Unity | - | - | - | not started |
| ACS | Assassin's Creed Syndicate | - | - | - | not started |
| ACO | Assassin's Creed Origins | - | - | - | not started |
| ACOdy | Assassin's Creed Odyssey | - | - | - | not started |
| ACV | Assassin's Creed Valhalla | - | - | - | not started |
| ACM | Assassin's Creed Mirage | - | - | - | not started |
| ACShadows | Assassin's Creed Shadows | - | - | - | not started |
| ACBFR | Assassin's Creed Black Flag Resynced | - | - | - | not started |

## Quick start
```
dotnet build src/AcSaveLib.Cli --no-restore      # see docs/BUILDING.md if restore fails
dotnet src/AcSaveLib.Cli/bin/Debug/net8.0/AcSaveLib.Cli.dll ac4 <file.save> summary
... ac4 <file.save> missions
... ac4 <file.save> tree [depth] [name]
... ac4 <file.save> models      # counts and key values of every typed model
... ac4 <file.save> unknown
... ac1 <file.sav> summary|roundtrip|models|object <classId>|unknown
... crc <PropertyName>
```
```csharp
using AcSaveLib.Games.Ac4;
var save = Ac4SaveFile.Load(path);
foreach (var g in save.Narrative.Groups)
    foreach (var m in g.Missions)
        if (m.Completed) { /* m.Id, m.HighestSynchronization */ }
```

## Read these (in order) if you are a new session
1. docs/AGENT_GUIDE.md - rules, workflow, what is confirmed vs guessed
2. docs/ARCHITECTURE.md - code layout
3. docs/FORMAT_AC4.md and docs/FORMAT_PROPERTY_STREAM.md - the byte formats
4. docs/FIELDS_AC4.md - field-by-field status (this is the to-do list)
5. docs/ADDING_A_GAME.md, docs/GAMES.md
