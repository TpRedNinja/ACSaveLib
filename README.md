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
| AC2 | Assassin's Creed II | - | - | - | not started |
| ACB | Assassin's Creed Brotherhood | - | - | - | not started |
| ACR | Assassin's Creed Revelations | - | - | - | not started |
| AC3 | Assassin's Creed III Remastered | - | - | - | not started |
| ACL | Assassin's Creed Liberation (original) | - | - | - | not started |
| ACLR | Assassin's Creed Liberation Remastered (its saves share the format of AC3 Remastered, to be verified) | - | - | - | not started |
| AC4 | Assassin's Creed IV Black Flag | done | done | done | missions (Narrative + Side) and typed models for all known fields, see docs/FIELDS_AC4.md |
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
