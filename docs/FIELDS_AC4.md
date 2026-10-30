# AC4 top-level fields: status (the to-do list)

Order as in ACViewer. "Decoded" = typed model exists and values match ACViewer.

| Field | Status | Model (Games/Ac4) |
|---|---|---|
| ContextID | DECODED (0 in every sample) | Ac4SaveFile.ContextId |
| NarrativeMissionHistory / SideMissionHistory | DECODED | MissionModels.cs |
| CurrentMission, World, PlayerPortal (u64) | DECODED (ID names via ForgeIndex) | Ac4SaveFile |
| AnimusDatabaseData | DECODED | AnimusDatabase |
| MapIconData | DECODED | MapIconData |
| MapFogData | names known; MapFogInfo was empty in every sample, element layout UNKNOWN | MapFogData (count only) |
| MessagingDatabaseData | DECODED (checked on Sequence 3 save with 10 messages) | MessagingDatabase |
| UITutorialData | DECODED except the six tutorial lists (empty in samples, element layout UNKNOWN) | UITutorialData |
| PresentEmailsData | DECODED | PresentEmails |
| PresentRewardSaveData | DECODED; `IsAvailable` name matched by hash only, never seen on screen | PresentRewards |
| AccomplishmentManagerData | DECODED | AccomplishmentManager |
| GameStatsSaveData | DECODED | GameStats |
| CommunityChallengeSaveData | UnsavedProgress empty in every sample, layout UNKNOWN | CommunityChallenge (count only) |
| PlayerProgSaveData | DECODED except ArmorStates, BackupInventoryItems, WeaponAmmoCountData (empty or unseen, counts only) | PlayerProgression, ProgressionCharacterData |
| SnapshotPlayerProgSaveData | DECODED (same class as PlayerProgSaveData) | PlayerProgression |
| SnapshotPlayerProgressionDelta | DECODED (checked on Sequence 3) | SnapshotDelta |
| FleetSaveGameData | DECODED | FleetData |

All property NAMES in the five sample saves are resolved (`ac4 <file> unknown` prints 0). Verified values (wr.save, matches ACViewer):
money 199854 (ProgressionCharacterData[9], container category 1), 1044 accomplishment entries, 499 stat entries, 231 animus entries,
13 map icon worlds, 25 captured boat slots, 100 online mission IDs. `ac4 <file> models` prints these counts.

Not confirmed: meaning of stat IDs, accomplishment IDs, enum value names (only values are stored), `TimePlayed` unit,
`EntryTime` is a Unix time by magnitude only, `OasisWeaponName`/`OasisLineId` are text-table line IDs (not Forge IDs).

## Entity ID names (CONFIRMED source: ACViewer Forge index files)
The owner's ACViewer exports `DataPC*.index.txt` (lines `0x<16 hex>: Name.Type`, ~664k IDs, ~100 MB; NOT stored in the repo).
`ForgeIndex.LoadFolder(dir)` (Names/ForgeIndex.cs) maps any u64 ID in a save to its name; CLI: `ac4 <file> named <index-folder>`.
Verified: Sequence 3 CurrentMission = SQ03_M05_Hunting.Mission, World = CaribbeanSea.World, PlayerPortal = ...SQ03_M05_Hunting_MissionStart.WorldTransitionPortal,
mission and group IDs resolve to names (e.g. SQ02M10_Havana.Mission).
This names ID VALUES only. Property NAMES (the CRC keys) are still unnamed.
ID types found inside the other top-level fields (scan of 5 saves): Accomplishment, Entity (collectibles/world objects),
InventoryItemSettings, AnimusDatabaseEntry, EntityGroup, EntityBuilder, World, WorldTransitionPortal, ShipHealthData,
MapIconWorldManager, UIProgressTrackerSubCategory. Which field holds which type is not yet mapped (GUESS: AccomplishmentManagerData->Accomplishment,
AnimusDatabaseData->AnimusDatabaseEntry, PresentRewardSaveData->InventoryItemSettings, MapIconData/MapFogData->Entity/World).
