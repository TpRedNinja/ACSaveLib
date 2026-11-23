# Liberation (ACL / ACLR) save fields

Layout and containers are in `FORMAT_ACL.md`. The reader is `Games/Acl` (read-only; decoded containers, raw property items and typed models: `AclModels.cs` header and options, `AclGameData.cs`, `AclFieldModels.cs`, `AclMissionModels.cs`, `AclPlayerProgressionModels.cs`). CLI `acl <file> models`. The models follow the Black Flag layout (GroupSaveData, ProgressionCharacterData), which is the layout the Liberation save uses; fields without a model stay in the node tree.

## Game save, container 1 (root `0x94D6F8F1`)
Top-level items, in order: ContextID, NarrativeMissionHistory, SideMissionHistory, CurrentMission, World, PSPState, SnapshotPSPState, PlayerPortal, AnimusDatabaseData, HuntingData [likely], MapFogData, MessagingDatabaseData, UITutorialData, PresentEmailsData, AccomplishmentManagerData, GameStatsSaveData, `56957182`, PlayerProgSaveData, SnapshotPlayerProgSaveData, SnapshotPlayerProgressionDelta, WorldUpgradeSaveData, SnapshotWorldUpgradeSaveData, `0eb2b23c`, `2557d813`, `9fd6d8a6`.
- Mission histories, PSP state, Animus data, tutorials, emails, accomplishments, game stats, progression and world upgrade use the same object names as Brotherhood and Revelations.
- `PlayerProgSaveData.ProgressionCharacterData` is an array (the first entries hold the attribute builder data `HatLevel` and `HolsterLevel` [likely]).
- `MapFogData.MapFogInfo[0]` holds a raw item `c1864dce` of 8210 bytes (the game save's only raw item) next to Width 256, Height 256 and `09c84024` 332.
- `2557d813` and `9fd6d8a6` have the same shape: an array `329e7889` (ShipListSave [likely]) of `5132f355` (ShipSaveData [likely]) entries with ShipName, ShipTypeSave, ShipContext, UsedSave, RouteSave and a price object (Price, BuyPrice, PriceMultiplier, Quantity, Locked), plus a `PendingEventsSave` array. The game save has one ship entry in each. Which of the two objects is the live state and which a snapshot is not known.
- `SaveGameName` "Aveline" and header TimePlayed 118 describe a save made at the start of the game.

## OPTIONS file
- Container 1: ContextID, PlayerOptionsSaveData, AccomplishmentManagerData, `e1dee2e4` (UplayActions [likely], LastNewsSeen [likely] and unnamed values), UPlayMapUnlocked, UPlaySkinUnlocked, UniqueKeyMap1Unlocked to UniqueKeyMap4Unlocked, CompletedUplayActions.
- PlayerOptionsSaveData holds the Brotherhood option names (language, volumes, camera, Skin01Unlocked, Skin02Unlocked, Stereoscopy settings) plus about 33 unnamed values (mostly Bool flags) set to 1 or 0 (probably unlock flags).
- Containers 2 and 3 have the roots `0xC9876D66` and `0xB4B55039` with unnamed keys only. Container 2 holds a raw item `d1c146c0` of 29 bytes inside `5586b7fe`.

## Open
- Unnamed keys: 38 in the game save, 61 in OPTIONS.
- Typed models for the ship, shop, hunting and PSP objects, ID tables for World and mission handles (no forge index files for Liberation were provided).
- Whether the second ship object is a snapshot.
