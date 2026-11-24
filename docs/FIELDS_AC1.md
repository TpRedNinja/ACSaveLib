# AC1 fields: status
| Part | Status | API (Games/Ac1) |
|---|---|---|
| Container (header, objects, fields, values) | DONE, byte-exact round trip on 8 saves | Ac1SaveFile.Parse / Write |
| Mission item states (939 items) | DONE (state 0-3, names resolved); IsCompleted and one unnamed bool exposed | SaveExtensions.MissionItems() |
| Accomplishments (6 configs, 96 slots) | DONE for State, IsResolved, Count, UsedEvents; config 1 has titles | Accomplishments(), Ac1Achievements |
| Collectibles (flags, Templars killed) | DONE as counts plus flag hash IDs for all lists except Templars killed | Collectibles() |
| PersistedIDs | DONE (matches the owner's notes: B1F21396, 507E28BA on Any% done) | PersistedIds() |
| Object 2092 MapManager (city, compass, Altair and player markers) | DONE, all 12 fields | MapData() |
| Object 2093 DNAMemoryManager (sync bar) | DONE, all 5 fields | DnaData() |
| Object 3003 SaveGameDataManager | DONE, all 23 fields; world IDs named for the 12 known worlds; rank, weapon and dagger values are raw numbers | SaveGameData() |
| Object 3004 HorseData | DONE, all 7 fields | HorseData() |
| Object 3009 TutorialManager | DONE, all 3 fields | TutorialData() |
| The four map objects (class Menu_Ingame_001: marker known flags, fog cells) | DONE; which world each belongs to is not decoded | MapObjects() |
| Extra mission item fields (MissionMarker, DestinationMarker, SceneAlreadyPlayed, HarassersKilled, ObjectiveBriefed, TargetSpawned, TargetReady, AssassinationDone, CurStepIdx, MissionActions, Targets, Clones, Kiosks, Flags, 73970a36) | DONE | MissionItems() properties |

## Names
- Property names: `Games/Ac1/ac1_props.tsv` (hash, name) from the owner's prop_hashes.json plus CRC32-confirmed names. The table is authoritative: a few hashes
  (MissionStatus 882350721, IsCompleted 1379586241) are NOT the CRC32 of those exact names. In the 8 sample saves all 88 hashes resolve except 3
  (`ac1 <file> unknown`): 6dcf11f9 (the owner's table: "Unknown (mission)"), 73970a36 (a 4-byte integer on Post-MM Emir) and de936275 (placeholder only).
- Class (mission item) names: `Games/Ac1/ac1_classes.tsv` from the owner's class_names.json. All 950 class IDs in the samples are named.
- Flag lists: `Games/Ac1/ac1_flags.tsv` (category, index, flag hash) from the owner's AC1_DX9_collectibles_map.csv. Spot-checked against AC1_notes.md 8.3
  (Kingdom 85 = 6258CB2F, Masyaf 16 = 62E0EA76, Damascus 74 = 5E8B413A).

## Typed sections (Games/Ac1/Ac1ManagerModels.cs)
- Models read all 50 AC1 saves checked (every save in the owner's Ac1 folder, Memory Blocks 1 to 7, autosplitter test saves and the 100% save), each with round trip exact, all five sections and the four map objects present; values match the owner's notes (sync blocks 13/13 at the start, 7/7 and 8/8 later; Kingdom world 1BCB96C4 on a Kingdom save; world IDs read as named worlds).
- MapManager city order is the order AC Viewer lists the five CitiesInKingdom entries (Acre, Damascus, Masyaf, Jerusalem, Arsuf); the known flags follow the story (Arsuf is the last to become known).
- SaveGameDataManager: world ID `1ED8758C` (Arsuf, seen in the Memory Block 6 saves) is named from the owner's statement; it is not in the notes' world table 6.2.
- The Tutorial Start saves have Current and Main world ID `00000000` (no world yet) and Always world `377AC858` (ParadiseFlashback); every other save names a world.
- SetByPlayerMarker hash `de936275` is exposed as IsMissionMarker (a CRC32 dictionary match, likely).
- AssassinMatrix is stored as 16 big-endian floats; the last four are `x y z 1` in every save read, so they look like the position.

## Still open
- Templars killed: only 30 of 60 entries are in the CSV and indices are per city world, so the list is not imported (count only). The owner's ac1_collectible_lists.json was not found.
- Meaning of `IsCompleted` (523ad0c1) versus the mission `State`, and of the 6dcf11f9 bool.
- Slots in configs 0, 2, 3, 4, 5 have no titles (see owner's notes 9.3).
- Types 6 and 7 signed or unsigned.
