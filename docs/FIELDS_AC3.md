# AC3 (original) save fields

Layout, keys and containers are in `FORMAT_AC3.md`. The reader is `Games/Ac3` (read-only; it exposes the decrypted bytes and the Liberation reader's containers and raw property items, there are no typed models yet).

## Game save, container 1
- NarrativeMissionHistory holds `GroupSaveData[]` (Group id, `MissionSaveData[]` with Mission, HighestSynchronization, TimePlayed, BriefingDisplayedOnce, BriefingAccepted, MissionStepSaveData, MissionConstraintSaveData). `3.save` has 27 groups, `Save003.sav` 4. Ids are 64-bit.
- SideMissionHistory, PSPState, SnapshotPSPState, PlayerPortal, AnimusDatabaseData, HuntingData, MapFogData, MessagingDatabaseData, UITutorialData, PresentEmailsData, AccomplishmentManagerData and GameStatsSaveData use the object names of Brotherhood, Revelations and Liberation.
- PlayerProgSaveData and SnapshotPlayerProgSaveData hold ProgressionCharacterData[] with CurrentHealth, SkillPoints, RankIndex, Status, Type, NewItemsUnlocked, NewSkillsUnlocked [likely names] and the other (unnamed) character values.
- The Snapshot* items repeat their counterpart. [guess: a copy taken at the last checkpoint]

## Open
- Unnamed keys: 3 in `Save003.sav`, 0 in the large game saves, 45 in the small profile saves.
- The names ACViewer shows for Remastered saves (`FIELDS_AC3R.md`) also resolve keys in the original saves. [likely]
- Typed models, ID tables for missions, worlds and accomplishments (no forge index files for AC3 were provided).
- Meaning of PlayerGlobalSynch 100 together with the 100% claim of the packs, of TimePlayed's unit, of the gap values and markers, and of the third container of `Save003.sav`.
- Which DLC episodes each `.sav` contains (DLCPackagesBits 686 in both).
