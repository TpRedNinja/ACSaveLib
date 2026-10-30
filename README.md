# AcSaveLib

A .NET library (plus CLI) that decrypts, decompresses and parses Assassin's Creed save files, for use by the
Route Tracker plugins. Goal: one library for all 15 AC games. Started with **AC4 Black Flag**.

Same idea as the Arkham route tracker's save library: the plugin asks the library "which missions are complete?"
and never touches raw bytes.

## Status
| Game | Decrypt | Decompress | Property stream | Mission data | Other fields |
|---|---|---|---|---|---|
| AC4 Black Flag | done | done | done | done (Narrative + Side) | not named/decoded yet, see docs/FIELDS_AC4.md |
| the other 14 | not started | | | | see docs/GAMES.md |

## Quick start
```
dotnet build src/AcSaveLib.Cli --no-restore      # see docs/BUILDING.md if restore fails
dotnet src/AcSaveLib.Cli/bin/Debug/net8.0/AcSaveLib.Cli.dll ac4 <file.save> summary
... ac4 <file.save> missions
... ac4 <file.save> tree [depth] [name]
... ac4 <file.save> unknown
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
