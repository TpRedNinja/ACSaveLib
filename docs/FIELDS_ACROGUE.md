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
- Missions entries (FleetMissionGameData): OwnerProfileId, MissionId, HelpedByProfileId, TemplateId, DurationSeconds, AssignedBoatIds, Odds, Roll, RemainingTimeSeconds, StartTime, FetchTimeMilliseconds. Rewards entries (FleetRewardGameData): RewardType (enum FleetRewardID, 1 = FleetCollectible_01), Value, TimeUnlocked. [confirmed names]
- Typed: `FleetData`, `FleetPlayer`, `FleetBoat`, `FleetMission`, `FleetReward`, `FleetRoute`, `BusinessManager`. Icebreaker and Events have no typed model yet.

- `PlayerProgSaveData` holds `SupplyCampSaveData` (between `NavalConvoySaveData` and `KillInfoSaveData`), an `ItemArray` of `Camp` (64-bit handle) and `Discovered` entries; 12 entries in the 100% save, each an object of class `SupplyCampSaveDataItem` (confirmed). `SupplyCampSaveData` is confirmed in ACViewer, `Camp` is a likely CRC32 word match. [confirmed / likely]
- `KillInfoSaveData` holds `WeaponKills` (4), `ToolKills` (4) and an array shown as `Property_0xBDE923E9` (6), each entry a `KillItemSaveData` with `OasisWeaponName` (S32) and `KillCount` (U32). [confirmed]
- `EventsSaveData` holds `PoppedEvents`, a JSON string (`gameSessionId`, `playerSessionId`, `areEventsOnline`, `eventsList`). [confirmed]
- `cd986d96` (0 in all 45 mission entries) and `c3a87f61` (resource entries) are not shown by ACViewer; they may be enums. [guess]

## Profile save
- Header container, then a global profile container (`PlayerOptionsSaveData`, `AccomplishmentManagerData`) and last `SinglePlayerOptionsSaveData`. See `FORMAT_ACROGUE.md`.

## Open
- Unnamed keys: 2 in the 100% game save (`cd986d96` in each fleet mission entry, `c3a87f61` in each resource entry), 1 in the two test saves (`c3a87f61`), 18 in the options save (the profile containers have not been opened in ACViewer). ACViewer shows `0xD44828A8`, `0x56BDD03A`, `0x9AC35A61`, `0x5D9984AF`, `0x2FD9481C`, `0x93E7E001`, `0xDD74D4AC`, `0xBDE923E9`, `0x510BDEDB`, `0x1C011834` as `Property_0x<HASH>`, so they count as named (types: U8, U64, Bool, Bool, Bool[5], U32, array of PresentDayTargetEntityState).
- Layouts of fields that are empty in the samples (MapFogInfo and others, as in Black Flag).
- ID meaning tables, TimePlayed unit.
