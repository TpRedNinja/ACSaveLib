# Rogue save fields

Layout, key and containers are in `FORMAT_ACROGUE.md`. The reader is `Games/Acrogue` (read-only); typed models are in `RogueModels.cs` (header and `RogueGameData`), `RogueFieldModels.cs`, `RogueMissionModels.cs` and `RoguePlayerProgressionModels.cs`. Names follow `Names/names.txt` (ACViewer names are confirmed, CRC32 word matches likely).

## Game save, main container
- Top-level items: ContextID, NarrativeMissionHistory, SideMissionHistory, CurrentMission, World, PlayerPortal, AnimusDatabaseData, MapIconData, MapFogData, MessagingDatabaseData, UITutorialData, PresentEmailsData, PresentRewardSaveData, AccomplishmentManagerData, GameStatsSaveData, CommunityChallengeSaveData, PlayerProgSaveData, SnapshotPlayerProgSaveData, SnapshotPlayerProgressionDelta, FleetSaveGameData, plus unnamed items. [confirmed on the 100% save]
- Same object names as Black Flag; the models are separate copies of the Black Flag layout. Not present in Rogue: `IsFirstGameCompletedBocce`, `OnlineMissionIDs`, `QuestItemsCollectedAmounts`. [confirmed on the 100% save]
- The 100% save reads as: Narrative 16 groups, 69 missions, 69 completed; Side 27 groups, 312 missions, 268 completed; 1293 accomplishment entries (1150 completed); 475 stat entries; 230 Animus entries; 25 captured boats; money 5163944. [confirmed against the file, not against a viewer]

## Profile save
- Header container, then a global profile container (`PlayerOptionsSaveData`, `AccomplishmentManagerData`) and last `SinglePlayerOptionsSaveData`. See `FORMAT_ACROGUE.md`.

## Open
- Unnamed keys: 13 in the profile save, 43 to 57 in game saves, header `0x510BDEDB`, `0x1C011834`, `0xB4D48994`.
- Layouts of fields that are empty in the samples (MapFogInfo and others, as in Black Flag).
- ID meaning tables, TimePlayed unit.
