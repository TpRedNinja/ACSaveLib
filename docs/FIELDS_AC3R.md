# AC3 Remastered fields

Same property stream as `FIELDS_AC3.md`; the Remastered saves contain every unnamed key of the original saves plus extra ones.

## Header (container 0)
- `C0/319e33d3` is a string holding the save label (`"100%"` in the 100% save), 0 otherwise. [likely]
- `C0/DLCPackagesBits` = 678 in the 100% save. [confirmed value, meaning unknown]

## Names
- ACViewer shows these names for keys that were unnamed hashes; each name's CRC32 equals the hash. [confirmed]
  - Profile save: `OnlineProfileSaveData` (`e1dee2e4`), `UplayULCRewards` (`5909456b`), `UplayWelcomeDone` (`1f2cb876`), `UplayWelcomeBackDone` (`14c1a643`), `OnlineCoreProfileSaveData` (`926723b3`), `EventSaveData` (`7107fa9a`), `EventSessions` (`2177fc62`), `LeaderboardsSaveData` (`4f44d65e`), `Statistics` (`ad8e88f2`), `Ranks` (`0a278f10`), `DisplaySubtitlesBackground` (`bef5e00a`), `UPlayNeverShowAgain` (`19e095df`).
  - Game save: `OnlineGameSaveData` (`56957182`), `SeasonWeatherSaveData` (`0eb2b23c`), `StackOfLootMoney` (`8f74fbf9`), `HuntingManagerSaveData` (`96d44411`), `MainWorldsHorse` (`07bf220c`), `OtherWorldsHorse` (`1b68ab06`), `ReplayHorse` (`6fe98045`), `LitTorches` (`ac2d9958`), `PristinePeltStores` (`799e3cbf`), `AccountingBookSaveData` (`66a27143`), `RecipeBookSaveData` (`1b36af04`), `BAMManagerSaveData` (`8363de15`).
- `4be4ac3c` = `FriendRanks`, the third array in `LeaderboardsSaveData` next to `Statistics` and `Ranks` (element type `LeaderboardRankSaveDataItem`). [likely, 2-word CRC32 match]
- ACViewer shows the other keys as `Property_0x<hash>`, so it has no name for them either. [confirmed]
- Types ACViewer shows for some unnamed profile keys: `3f207d6d` Bool, `e077a7e3` an enum of type 0x6D7F8969 (value 2: 0x706646B4), `b1ac89b6` U32, `4c54a5b3` and `c6ef608c` Language enums, `e1cfaaa2`, `d8429667`, `cf398224`, `10804df5`, `07fb59b6`, `449a84d3`, `83353401` Bool, `ddaffa5b` and `4b821c37` U32, `d8c65114` a U32 array of 29 values (256000 to 780800), `ca2549e9` Float 800, `e1f4e00b` Float 120, `d3222401` Bool; the header `319e33d3` is an LString. [confirmed]

## Header (container 0)
- `C0/319e33d3` is an LString holding the save label (`"100%"` in the 100% save), empty in the profile save. [likely]
- `C0/DLCPackagesBits` = 678 in the 100% save and in the profile save shown by ACViewer. [confirmed value, meaning unknown]

## Unnamed keys
- `ac3r <file> unknown` lists them: 31 to 50 in game saves, 64 in the slot -1 profile save (mostly `C1/PlayerOptionsSaveData` entries).
- Every key still unnamed in the original saves is also present in the Remastered saves.
