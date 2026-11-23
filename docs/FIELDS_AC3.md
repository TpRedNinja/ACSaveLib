# AC3 (original) save fields

Layout, keys and containers are in `FORMAT_AC3.md`. The reader is `Games/Ac3` (read-only; it decrypts, decodes the containers itself and exposes raw property items and typed models: `Ac3Models.cs` header and options, `Ac3GameData.cs`, `Ac3FieldModels.cs`, `Ac3MissionModels.cs`, `Ac3PlayerProgressionModels.cs`). CLI `ac3 <file> models` (also `ac3r`). It does not use the Liberation reader. Fields without a model stay in the node tree.

## Game save, container 1
- NarrativeMissionHistory holds `GroupSaveData[]` (Group id, `MissionSaveData[]` with Mission, HighestSynchronization, TimePlayed, BriefingDisplayedOnce, BriefingAccepted, MissionStepSaveData, MissionConstraintSaveData). `3.save` has 27 groups, `Save003.sav` 4. Ids are 64-bit.
- SideMissionHistory, PSPState, SnapshotPSPState, PlayerPortal, AnimusDatabaseData, HuntingData, MapFogData, MessagingDatabaseData, UITutorialData, PresentEmailsData, AccomplishmentManagerData and GameStatsSaveData use the object names of Brotherhood, Revelations and Liberation.
- PlayerProgSaveData and SnapshotPlayerProgSaveData hold ProgressionCharacterData[] with CurrentHealth, SkillPoints, RankIndex, Status, Type, NewItemsUnlocked, NewSkillsUnlocked [likely names] and the other (unnamed) character values.
- The Snapshot* items repeat their counterpart. [guess: a copy taken at the last checkpoint]

## Open
- Unnamed keys: 3 in `Save003.sav`, 0 in the large game saves, 12 in the small profile saves.
- The names ACViewer shows for Remastered saves (`FIELDS_AC3R.md`) also resolve keys in the original saves. [likely]
- Typed models for the PSP state, hunting data, world upgrade and weather objects, ID tables for missions, worlds and accomplishments (no forge index files for AC3 were provided).
- Meaning of PlayerGlobalSynch 100 together with the 100% claim of the packs, of TimePlayed's unit, of the gap values and markers, and of the third container of `Save003.sav`.
- Which DLC episodes each `.sav` contains (DLCPackagesBits 686 in both).
