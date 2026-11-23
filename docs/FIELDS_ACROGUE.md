# Rogue save fields

Layout, key and containers are in `FORMAT_ACROGUE.md`. The reader is `Games/Acrogue` (read-only); typed models are in `RogueModels.cs` (header and `RogueGameData`), `RogueFieldModels.cs`, `RogueMissionModels.cs` and `RoguePlayerProgressionModels.cs`. Names follow `Names/names.txt` (ACViewer names are confirmed, CRC32 word matches likely).

## Game save, main container
- Top-level items: ContextID, NarrativeMissionHistory, SideMissionHistory, CurrentMission, World, PlayerPortal, AnimusDatabaseData, MapIconData, MapFogData, MessagingDatabaseData, UITutorialData, PresentEmailsData, PresentRewardSaveData, AccomplishmentManagerData, GameStatsSaveData, CommunityChallengeSaveData, PlayerProgSaveData, SnapshotPlayerProgSaveData, SnapshotPlayerProgressionDelta, FleetSaveGameData, plus unnamed items. [confirmed on the 100% save]
- Same object names as Black Flag; the models are separate copies of the Black Flag layout. Not present in Rogue: `IsFirstGameCompletedBocce`, `OnlineMissionIDs`, `QuestItemsCollectedAmounts`. [confirmed on the 100% save]
- The 100% save reads as: Narrative 16 groups, 69 missions, 69 completed; Side 27 groups, 312 missions, 268 completed; 1293 accomplishment entries (1150 completed); 475 stat entries; 230 Animus entries; 25 captured boats; money 5163944. [confirmed against the file, not against a viewer]

## Fleet (Rogue only)
- `FleetSaveGameData` holds OnlineMissionIDs [absent in Rogue], CaptureBoatCount, CaptureBoatNextSlot, CapturedBoats and `FleetPlayerGameData` (ProfileId, Status, UnlockedBoatSlots, Boats, Resources, Missions, Routes, Rewards, BoatAutoGenID, MissionAutoGenID). [confirmed: names read from ACViewer on a Rogue save and matched to the stored CRC32 keys]
- Boats entries (FleetBoatGameData): BoatId, BoatName, TemplateId, Health, Status, UnavailableTimeSeconds, FetchTimeMilliseconds. Routes entries (FleetRouteGameData): RouteId, DifficultyModifier, Status, LastUpdateTimeSeconds, LastUpdateTime. Resources entries (FleetResourceGameData): Value plus an unnamed key `c3a87f61`. [confirmed names]
- The 100% save has 12 boats, 33 routes, 45 missions and 7 resources; `BusinessManagerData` (PurchasedBusinessLevels, MoneyInBank, TimePassedSinceLastReward) and `IcebreakerSaveData` (IcebreakerState) follow the fleet; `EventsSaveData` follows `CommunityChallengeSaveData`. [confirmed names]
- Typed: `FleetData`, `FleetPlayer`, `FleetBoat`, `FleetRoute`, `BusinessManager`. Missions, Rewards, Icebreaker and Events have no typed model yet.

## Profile save
- Header container, then a global profile container (`PlayerOptionsSaveData`, `AccomplishmentManagerData`) and last `SinglePlayerOptionsSaveData`. See `FORMAT_ACROGUE.md`.

## Open
- Unnamed keys: 8 in the profile save, 20 in game saves (the children of `FleetMissionGameData` and `FleetRewardGameData` entries, `c3a87f61`, `ItemArray` entries of `9f835b4c`, `bde923e9`, `7e1ce9a9`, `61a6ed0e`), header `0x510BDEDB`, `0x1C011834`, `0xB4D48994`. ACViewer shows `0xD44828A8`, `0x56BDD03A`, `0x9AC35A61`, `0x5D9984AF`, `0x2FD9481C`, `0x93E7E001`, `0xDD74D4AC` as `Property_0x<HASH>`, so they count as named (types: U8, U64, Bool, Bool, Bool[5], U32, array of PresentDayTargetEntityState).
- Layouts of fields that are empty in the samples (MapFogInfo and others, as in Black Flag).
- ID meaning tables, TimePlayed unit.
