# AC2 fields
Models: `src/AcSaveLib/Games/Ac2/Ac2Models.cs`, `Ac2SaveFile.cs`. CLI: `ac2 <file> models|missions`. Names are CRC32 of the property name; names found by matching strings in the game exe and the owner's notes are in `Names/names.txt` (several were not seen in a viewer).

## Header properties
- Version 17, MagicNumber 0x00FEDBAC, DLCPackagesBits (6 in all samples), CodeChangelist (236390 in all samples).
- IsValid, IsOptions (1 for options-only saves, SaveGameName "Options"; 0 for game saves, SaveGameName "Desmond"), LoadOrSaveOptionsFailed and IsCorrupt: names and values CONFIRMED in ACViewer. IsCorrupt is 0 in all samples; a converter can read it to reject a damaged save.
- PlayerGlobalSynch: 100 in the 100% save, 54 in the sequence 7 Benvenuto save. LIKELY the sync percentage. Not compared with the in-game value.
- SaveGameDate: Unix seconds (CONFIRMED plausible: 2017-08-17 for the 100% save).
- TimePlayed: unit not confirmed (ACViewer shows 5629 for a sequence 2 save).

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
- GameStatsSaveData: 22 u32 counters, all named, names CONFIRMED in ACViewer (TimePlayed, TotalTimeBlended, DeathOccurrences, Kills, HiddenBladeAssassinations, AirAssassinations, DoubleAirAssassinations, Disarms, TotalMoneyEarned, TotalMoneySpent, NotoriousOccurrences, BorgiaCourrierKills, CourtesansHired, MercenariesHired, ThievesHired, OverallDistanceCovered, HeightClimbed, DistanceOnGround, DistanceSwam, DistanceHorse, DistanceGondolas, DistanceFlown) plus WeaponUsedSavedInfo[]. Units are not confirmed. Model: `Ac2GameStats`.
- VillaSaveData: StrongholdValue, CurrentGoldIncome, CurrentIncomeDelay, PendingIncomeValueForAfs, PendingStrValueForAfs, BuildingsLevels (u8 x 11), CodexPagesDecodedStatus (bool x 30), CodexPagesCurrentRotation (s16 x 30). Names CONFIRMED in ACViewer; `SnapshotVillaSaveData` is a second copy. Model: `Ac2Villa`.
- AnimusDatabaseData: ADBEntries, CodexFound (bool x 30), LetterFoundCountLeft (s8 x 18), ADBGlyphs and 14 tutorial/speech flags (ADBEntryFoundSpeechPlayed ... LGS04PresentationShown). Names CONFIRMED in ACViewer. Not modelled.
- PSPState and SnapshotPSPState (class PSPTransferredStuff): PSPConnectionDone, PSPGoldCoins, PSPSilverCoins, PSPBronzeCoins, PSPWeaponUnlocked, HiddenBladeArmorCodex, HiddenBladeSlimBladeCodex, HiddenBladeGunCodex, LifeCodex. Names CONFIRMED in ACViewer. Not modelled.
- PlayerCharacterData (3 characters), armor states, PlayerAttributesBuilderData, WorldNotorietyList, UITutorialData: present, unmodelled (`ac2 <file> tree`).
- Property key 0x2ABEA3BC (child object of PlayerCharacterData that holds InventoryContainerList) has no name.
- Options-only saves (`2.save`-style, IsOptions 1) hold AssassinGlobalProfileData: PlayerOptionsSaveData (37 fields: subtitles, languages, volumes, camera, HUD toggles, and the bonus unlock flags LGS01-04, Skin01-04, FanPack, SharpestBlades, EziosDisguise, Mansion), UPlayMapUnlocked, UPlaySkinUnlocked, UniqueKeyMap1-4Unlocked, CompletedUplayActions (bool x 4), AccomplishmentManagerData and PSPState. Names and values CONFIRMED in ACViewer (screenshots of a 2.save). Models: `Ac2Options`, `Ac2Profile`.
- Keys still without a name (15 across 33 saves): 0x2ABEA3BC (inventory object of PlayerCharacterData), 0xDB345970 (second PlayerCharacterData array), 0x9A2FFE03, 0xC87EC924 (inventory container), 0x3811A126 (WorldNotorietyValue), the fields of AnimusDatabaseEntrySaveGame (0xF977547D, 0x75758A0E, 0x768CAE23, 0x309E9CEF), AnimusDatabaseGlyphSaveGame (0xDAFFE03A, 0xAF605F1F), AFSTutorialInfo (0xDA47DC47, 0xC411B3D4) and WeaponUsedSavedInfo (0x8396D321, 0x7F0D4F7E). Run `ac2 <file> unknown`.
