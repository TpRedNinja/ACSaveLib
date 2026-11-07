# AC2 fields
Models: `src/AcSaveLib/Games/Ac2/Ac2Models.cs`, `Ac2SaveFile.cs`. CLI: `ac2 <file> models|missions`. Names are CRC32 of the property name; names found by matching strings in the game exe and the owner's notes are in `Names/names.txt` (several were not seen in a viewer).

## Header properties
- Version 17, MagicNumber 0x00FEDBAC, DLCPackagesBits (6 in all samples), CodeChangelist (236390 in all samples).
- IsOptions: 1 for options-only saves (SaveGameName "Options"), 0 for game saves (SaveGameName is a short string stored as a u32 pair, not decoded).
- PlayerGlobalSynch: 100 in the 100% save, 54 in the sequence 7 Benvenuto save. LIKELY the sync percentage. Not compared with the in-game value.
- SaveGameDate: Unix seconds (CONFIRMED plausible: 2017-08-17 for the 100% save).
- TimePlayed: unit not confirmed.

## Mission history (NarrativeMissionHistory, SideMissionHistory)
- `GroupSaveData[]` of { Group (hash), MissionSaveData[] of { Mission (hash), HighestSynchronization, BriefingDisplayedOnce, BriefingAccepted, MissionStepSaveData[] { Step, IsCheckpoint, Succeeded }, MissionConstraintSaveData[] } }.
- Mission and group hashes are the IDs of `AC2_HashNames.txt` (CONFIRMED: 217 of 221 named IDs are present in the 100% save; the 4 absent are "Leaving the Assassin hideout" and the three Leonardo-Codex entries).
- HighestSynchronization is 100 for finished missions. In the 100% save 204 of 206 narrative missions are 100. Entries with 0 and steps hold progress (the side-mission "RHP" high-point entries, 12 of 39 side entries in the 100% save; two narrative entries). LIKELY: 100 = completed. Not compared with the in-game mission state.
- Missions that were not started are absent from the file.

## Collectibles (PlayerProgSaveData)
- `GainedInventoryItems`: packed u32 of item hash IDs. CONFIRMED: 791 entries in the 100% save, the same set the owner read from memory (`PlayerProgressionManager+0x30`, 790 entries); IDs match the TSV (350 TreasureChest, 100 Feather, 30 Codex_Page, 30 Painting, 8 Statue_Codex, 6 VAULT_Assassins_SEAL, LGS chests).
- `QuestItemsCollectedAmounts`: packed u32 of 9 counters. CONFIRMED equal to the in-memory counters `[PlayerProgressionManager+0xF0]` (100% save: 30, 100, 8, 6, 20, 30, 500, 14, 0). Index 0 codex pages, 1 feathers, 2 statuettes, 3 assassin seals, 6 chests (CONFIRMED against the item counts of the Benvenuto save: 20, 54, 8, 4 and 247). Indices 4, 5, 7, 8: 5 matches the painting count (25 in the Benvenuto save); the others are not identified.
- `SnapshotPlayerProgSaveData` holds a second copy (one item fewer in the 100% save).

## Other
- Money: LIKELY `CurrentItemCount` of the first inventory container of the first PlayerCharacterData (category hash 0x12F6B6F1, the AC4 InvItem_Money hash); 953500 in the 100% save. Not compared with the in-game value.
- AccomplishmentManagerData.AccomplishmentEntries: 81 entries { ObjectID, Inactive, Completed, CurrentCount }; 48 completed in the 100% save. Not mapped to achievements.
- GameStatsSaveData: TimePlayed, Kills, Disarms and further unnamed counters.
- PlayerCharacterData (3 characters), armor states, PlayerAttributesBuilderData, WorldNotorietyList, AnimusDatabaseData, UITutorialData, VillaSaveData: present, unmodelled (`ac2 <file> tree`).
- Property key 0x2ABEA3BC (child object of PlayerCharacterData that holds InventoryContainerList) has no name.
- About 110 property keys still have no name (`ac2 <file> unknown`).
