# Syndicate save fields

Layout, key and containers are in `FORMAT_ACS.md`. The reader is `Games/Acs` (read-only); typed models are in `AcsModels.cs` (`AcsGameData`), `AcsMissionModels.cs`, `AcsPlayerProgressionModels.cs` and `AcsOptionsModels.cs`; the header is `AcsHeader`. Names follow `Names/names.txt`. A key whose name is shared with Unity was read in ACViewer on a Unity save and matches the same CRC32 key here (confirmed). 20 more are strings in `ACS.exe` whose CRC32 equals a key (likely; ACViewer has not been used on Syndicate).

## Game save, main container
- Top-level items: ContextID, NarrativeMissionHistory, SideMissionHistory, CurrentRoot, CurrentMission, World, PlayerPortal, TransitionProfile, AnimusDatabaseData, MessagingDatabaseData, AccomplishmentManagerData, PlayerProgSaveData, SnapshotPlayerProgSaveData, SnapshotPlayerProgressionDelta, WorldUpgradeSaveData, OnlineGameSaveData, PermanentIconSaveData, CrowdEventsSaveData, DistrictManagerSaveData, ProgressionLogSaveGameData, plus four unnamed objects (`f4116813`, `88670f57`, `e41874ad`, `8ee20463`). [confirmed names for all but the last five]
- Not present compared with Unity: MapFogData, TrainingData, CaseFileData, PresentEmailsData, VillaManagerSaveData, TimedCollectibleSaveData, ContactOnlineServiceSaveData, OnlineGameEntitySaveData, ClanManagerData, PushMatchmakingData. Present only in Syndicate: DistrictManagerSaveData, ProgressionLogSaveGameData. [confirmed on the three game saves]

## Missions
- NarrativeMissionHistory and SideMissionHistory have the Unity layout (`Identifier`, `GroupSaveData`, mission steps and constraints) plus `MissionOutputSaveData` entries (`Output` and an unnamed key). The identifiers are the same as in Unity. [confirmed]
- HighestSynchronization holds small values (0, 1, 2, ...); its meaning is not confirmed. The 100% save has 30 narrative groups with 274 missions and 7 side groups with 73 missions. [confirmed against the file]

## Player progression
- `PlayerProgSaveData` and the Snapshot twin: UncollectedInventoryItemsOnMap, ProgressionCharacterData (3 in the 100% save, 5 in the DLC save), DataInitialized, SavedInventoryItems (IsLock, InventoryItems). Each character: ProgressionCharacterSelector (ProgressionCharacter), InventorySpecification (InventoryContainerList), GainedInventoryItems, LockedInventoryItems, SelectedWeapon, SelectedEquipment, SelectedRanged, NewInventoryItems, TagAIAction, BrokenHealth, Life, MaxLife, LifetimeCreedPoints, CurrentCreedPointsIncrease, DiscoveredMissions, XP, Tier, and an unnamed object `a0da2041` holding `Tags` (`Tag`). [confirmed names, except `Tags`, `Tier`: likely]
- The Unity fields AvatarSkills, AvatarSkillsUnlock, SpendedAvatarSkillPoint, MainAvatarLoadout, BackupInventoryItems, QuestItemsCollectedAmounts, WorldNotorieties and the horses are not present. [confirmed]
- ID values are the 64-bit ids as stored; the meaning tables are not built.

## Other objects
- `DistrictManagerSaveData`: DistrictAssociateSaveData (Associate, Influence, Level, Activated; 6 in the 100% save), FightClubSaveData (8) and CartRaceSaveData (3), each with unnamed fields. [likely names]
- `OnlineGameSaveData`: PlayerProfileSaveData and HeroStatsSaveData (Statistics of StatId and StatValue; 345 and 18 entries in the 100% save), SocialCollectablesSaveData, SecondaryStoreSaveData, LeaderboardsSaveData, CommunityHeroStatsSaveData, OnlineCoreGameSaveData. [names confirmed for Statistics, likely for the others]
- `WorldUpgradeSaveData`: CurrentGoldChestAmount, CurrentIncomeDelay, PendingIncomeValueForAfs. `PermanentIconSaveData` (DiscoveredIcons), `CrowdEventsSaveData` (CompletedCrowdEventsNb), `AnimusDatabaseData`, `AccomplishmentManagerData` (330 entries in the 100% save, 371 in the DLC save). [confirmed names]

## Options save (slot 3)
- Profile container: PlayerOptionsSaveData (as Unity plus TextLanguage and GPSVisibility), OnlineProfileSaveData (UplayRewards, UplayActions, UplayWelcomeBackDone, SecondaryStoreProfileSaveData, OnlineCoreProfileSaveData with EventSaveData). Last container: SinglePlayerOptionsSaveData. [names confirmed for the Unity ones, likely for the others]

## Open
- 56 distinct unnamed keys over the six saves; they need ACViewer on a Syndicate save to be named. By object: PlayerProgSaveData and its Snapshot twin 11 each, OnlineProfileSaveData 10, OnlineGameSaveData 8, SnapshotPlayerProgressionDelta 7, the mission histories 6 and 5, SinglePlayerOptionsSaveData 4, DistrictManagerSaveData 4, the four unnamed top-level objects 9, ProgressionLogSaveGameData 3, PlayerOptionsSaveData 1 (keys shared between objects are counted once per object).
- HighestSynchronization values, ID meaning tables, TimePlayed unit, layouts of fields empty in the samples.
