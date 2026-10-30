# AC4 top-level fields: status (the to-do list)

Order as in ACViewer. "Decoded" = typed model exists and values match ACViewer.

| Field | Status |
|---|---|
| ContextID | present, not modeled |
| NarrativeMissionHistory | DECODED |
| SideMissionHistory | DECODED |
| CurrentMission (u64) | DECODED |
| World (u64) | DECODED (meaning of ID unmapped) |
| PlayerPortal (u64) | DECODED (meaning of ID unmapped) |
| AnimusDatabaseData | structure parses, children unnamed |
| MapIconData | structure parses, children unnamed |
| MapFogData | structure parses, children unnamed |
| MessagingDatabaseData | structure parses, children unnamed |
| UITutorialData | structure parses, children unnamed |
| PresentEmailsData | structure parses, children unnamed |
| PresentRewardSaveData | structure parses, children unnamed |
| AccomplishmentManagerData | structure parses, children unnamed |
| GameStatsSaveData | structure parses, children unnamed |
| CommunityChallengeSaveData | structure parses, children unnamed |
| PlayerProgSaveData | structure parses, children unnamed |
| SnapshotPlayerProgSaveData | structure parses, children unnamed |
| SnapshotPlayerProgressionDelta | structure parses, children unnamed |
| FleetSaveGameData | structure parses, children unnamed |

~146 distinct unnamed child keys across the sample saves (`... ac4 <file> unknown`). Next steps: name them
(see AGENT_GUIDE "Property names"), then add typed models under Games/Ac4/ and tick them off here.
Unverified brute-force name candidates (NOT confirmed, may be false positives): Item, Items, ItemList, WantedLevel,
NotorietyLevel, IsCompleted, IsFound, UnlockedEntries, AccomplishmentEntries, EmailGroupUnlocked, IsEmailNew, ItemsDelta,
RewardItems, StatEntries, EntriesData, SelectedWeapon, SelectedEquipment, LockedInventoryItems, CurrentItemCount,
ChestArmorLevel, ExtraData, Entity, EntityStateArray.

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
