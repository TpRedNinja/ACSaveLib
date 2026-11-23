# ACR (Assassin's Creed Revelations, PC) .save format
The container and LZO2A codec are shared with Brotherhood (`ChunkContainer`, `Lzo2a`); the plain options save decodes with that codec. Reader: `Games/Acr/AcrSaveFile.cs` (C#, read-only), CLI verb `acr`; it has its own models (`AcrGameData`, same layout as Brotherhood). Findings below come from 3 saves and from `ACRSP.exe` in IDA (imagebase `0x400000`; `sub_...` are IDA's automatic names, nothing is renamed in the IDB).

## Header
- Same header as the AC2 Steam saves: u32 `0x224` (header length) | u32 `0` | 32 character ASCII id at `0x08` | UTF-16 title from `0x28` | zero padding up to `0x228` (see `FORMAT_AC2.md`).
- The title reads `AC2[N]-tm_<yyyy>_<mm>_<dd> <hh>:<mm>:<ss>`: the game writes the prefix `AC2`. N is `-1` in the options-only save, `0` and `1` in the two game saves.

## Plain save (options only, 4351 bytes)
- The stream starts at `0x228` with u32 `0x16` | `ac db fe 00` | u32 length | u32 uncompressed size | container signature | `01 00 02 00 80 00 00` | blocks, the same container as Brotherhood (`FORMAT_ACB.md`).
- Five containers (uncompressed 283, 1689, 162, 9268 and 68 bytes). Every block decodes with the LZO2A decompressor.
- The first block starts with the same bytes as Brotherhood's options save (`06 00 e1 00 52 3b be bd ...`) but is shorter (161 bytes compressed instead of 165).

## Encrypted saves
- The two game saves (slots 0 and 1) are enciphered from `0x228` to the end of the file with a repeating XOR key, for the owner the 12 ASCII bytes `amsirrahrats`. The key is aligned to the end of the file: byte `i` uses `Key[(i - fileLength + 1) mod L]`, so the last byte of the file uses `Key[0]`. CONFIRMED: after this every block of both game saves decodes with LZO2A and consumes exactly its stated size. The options save (`1.save`) is not enciphered.
- How the key was found: the plaintext at `0x228` is `16 00 00 00 ac db fe 00` and the container signature follows 16 bytes later, so the key stream is known at offsets 0..7, 16..23 and 24..30. The two saves have different file lengths and give the same key text at different rotations, which fixes the period (12) and the end alignment.
- The key text was not found as a plain string in `ACRSP.exe` (searched `amsirrahrats` and its reverse). The decrypt call is below the save loader (`sub_463B50` reads from a storage object through a vtable), not located in IDA.
- The AC4 scheme (16 byte key) does not apply.

## Containers
- Game saves hold 4 containers: the header container (root `0xBDBE3B52`, SaveGame, 283 to 292 bytes) and three game data containers with roots `0x94D6F8F1`, `0x409E7FAE` and `0x3EBD922B`. All three have the same 29 top-level items as each other. Container 1 (`0x94D6F8F1`) is the main game; in the saves seen, container 2 holds a second world with 5 missions in one save and is a 7136 byte default in the other, and container 3 is always the 7136 byte default [meaning of containers 2 and 3 not known].
- Between containers there is the 16 byte gap of Brotherhood (u32 length, u32 `1`, u32 `0x00CAFE00`, u32 size).
- The options save has five containers (283, 1689, 162, 9268, 68 bytes) with roots `0xBDBE3B52`, `0x305AE1A8` (AssassinGlobalProfileData), `0xC9876D66`, `0xB4B55039` and `0xDBEB7469`.
- Container 1 has the Brotherhood items (missions, stats, trainees, progression, villa, world upgrade) with the same names, plus unnamed keys. Header TimePlayed equals GameStatsSaveData TimePlayed (50395 in the long save).
- Key `27831dd4` (top level of the main game container) is a UTF-16 text, the save slot description: `SYNCHRONIZATION (%): 65.21`, `TIME PLAYED: 13:59:55`, `DISTANCE COVERED (m): 116238` on separate lines [likely: save slot text; values match the stats].

## Key origin
- ACViewer decrypts the saves with the legacy username `starharrisma` (confirmed). The ACST save tool (https://github.com/linuslin0/ACST, readme only) lists Revelations with ID type "Account (legacy) username", entered in lowercase.
- The 12 byte key `amsirrahrats` is that username reversed, because it is applied aligned to the end of the file. The key is different for every user (their legacy username), and its length is the username length.
- Library: `AcrSaveFile.Parse(file)` needs no input: the key is recovered from the known plaintext at the stream start (`16 00 00 00 ac db fe 00`, the container signature at +16 and `01 00 02 00 80 00 00 01` at +24, 24 bytes), for key lengths 1 to 16 (Ubisoft usernames have 3 to 15 characters). Recovery is CONFIRMED on both game saves. For other username lengths it was tested on re-encrypted copies of a game save (4, 12, 15 and 16 character names, key aligned to the end of the file), not on real saves of those users. `Parse(file, legacyUsername)` is an optional override that builds the key from a username.
- CLI: `acr <file> <verb>`; `--user <legacy username>` is an optional override.

## Names
- ACViewer opens the saves with the username `starharrisma` and shows property names. Names read from its screenshots of the options save and the main game container, then matched to the stored CRC32 keys, are confirmed: `Char01Unlocked` to `Char04Unlocked`, `Tutorials`, `SaveGameDetails`, `SaveGameSubtitle`, `ExtraSaveData1`, `ExtraSaveData2`, `CityEventSaveData`, `BombCraftingSaveData`, `DiceGameSaveGameData`, `PenroseSaveGameData`, the `Stereoscopy*` options, `AddC01Unlocked` to `AddC06Unlocked`, `MultiBonus01Unlocked` to `MultiBonus03Unlocked`, `MultiEmblemsUnlocked`, `SingleBonus01Unlocked`, `BonusEntityBuilderSkinsUnlocked`, `MedicineUpgradeUnlocked`, `BrotherhoodWeaponsUnlocked`, `SpecialMissionUnlocked`, `MultiplayerPackUnlocked`.
- 20 names are exact CRC32 matches of strings in `ACRSP.exe`; more came from two-word matches of exe-derived words (`Names/names.txt`, "Revelations" section). Those not seen in ACViewer are [likely].
- Three-word CRC matches are mostly chance (about 300 expected for 193 keys); `CurrentDLCMask`, `AlreadyEarnedRewards` are [guess].
- Also confirmed from ACViewer: `Weights` (CityEventSaveData, 27 values), the BombCraftingSaveData counters (`CasingCraftedCounter`, `EffectCraftedCounter`, `EffectUsedCounter`, `CraftedAllCasingsCounter`, `CraftedAllEffectsCounter`, `UsedAllEffectsCounter`, `UsedBombCounter`, `GuardVictimCounter`, `GuardTrappedCounter`, `TrackingCummulativeBombsCrafted`), `DenDefenseGameCounter`, `DenDefenseScenarioIndex`, `DenDefenseCurrentLoadout`, `BombContainers`, `DenDefenceChance`.
- Also confirmed from ACViewer: the `DenDefense*` stats (win and loss counts, per-trap placements, `DenDefenseTotalMoraleSpent`, `DenDefenseCanonKills`), `DistanceZipline`, `PrimaryWeaponUsedSavedInfo`, `SecondaryWeaponUsedSavedInfo`, the trainee guild names (`NationalitiesSaveData`, `TraineeDenMissionPairs`, `UnUsedMaleTraineeNamesOasisIDs`, `UnUsedFemaleTraineeNamesOasisIDs`, `ComboKillAbility`, `ThrowThunderBombAbility`, `WoundedHealingTimeLeft`, `MasterAssassinTutorialShown`, `AssignedTrainee*`) and the contract location fields (`CurrentControlValue`, `CurrentMaximumControlValue`, `ControllingFaction`, `UnlockedTraineeSlots`, `CurrentGoldRevenue`, `CurrentXPBonus`, `NumDaysPassedUnderCurrentControl`, `AssignedTrainees`).
- Also confirmed from ACViewer: `DiceGameSaveGameData` (`GameHasBeenPlayedBefore`, `TotalMoneyWon`, `TotalMoneyLost`) and `PenroseSaveGameData` (`MissionsCompletedInPenroseSingle`, a U64 bit set; 38463 in the long save).
- Also confirmed from ACViewer: `BombCasingSettings`, `BombExplosiveSettings`, `BombEffectSettings` (in `Bomb`), `AssassinSelectedEquipment` and `Count` (in `BombContainer`), `Containers` (in an `InventoryContainerList` entry), `NeedToBeBurned` (in `BuildingsRules`), `CurrentRunSucceeded` (in `MissionConstraintSaveData`).
- Array elements are wrapped in an object whose key is the element class name. CRC32 matches of exact names, each at a matching spot, not seen in a viewer [likely]: `NationalitySaveData`, `TraineeDenMissionPairSaveData`, `ContractLocationSaveData`, `BuildingRuleSaveData`, `ContractAssignmentSaveData`, `MultiItemInventoryContainer` (wrapper inside the `InventoryContainerList` entries that hold `Containers`; the name came from the owner, the CRC32 matches exactly).
- Options keys `InvertCannonHorizontalOnlyCameraControls` (`ed915bd4`) and `InvertCannonVerticalOnlyCameraControls` (`f20b5679`) are CRC32 matches of strings in ACRSP.exe, sitting after `FlyingMachineControls` in the options profile, not seen in a viewer [likely].
- Keys named by CRC32 dictionary match of word combinations, each at a matching spot, not seen in a viewer [likely]: `AbilityProfileData` (`94daaee6`, element wrapper of `AbilityProfiles`), `ActiveAbilityID` (`fbea68f7`, U32 list in each entry), `NameCustomized` (`3fdeb65c`, bool in each entry), `HudShowLogs` (`b6ba16bb`), `HudShowMessages` (`20ad7434`), `HudShowTips` (`221991ef`) (three bool flags after `DynamicHud`), `NewsID` (`25170d6b`, list of two U32), `StereoscopyIntensity` (`04365105`, float 5.0 after the other Stereoscopy keys).
- Also confirmed from ACViewer: `InventoryItemRechargeableContainerPair` (class of each `Containers` element, with `Item` and `Container`), `GainedDenWorldUpgradeSettings` (the empty handle list in `SnapshotPlayerProgressionDelta`).
- The ACViewer options screenshot shows `InvertCannonHorizontalOnlyCa...` and `InvertCannonVerticalOnlyCame...` (cut off); the full names are not yet matched.

## Game code (`ACRSP.exe`)
| VA | What |
| --- | --- |
| `0x463B50` | Save loader (`sub_463B50`). Checks the first u32 is `22` and the second is `0xFEDBAC`, reads a length at `+8`, builds a buffer from `+12`, then reads further length-prefixed buffers, one per object |
| `0x4649E0` | Save writer (`sub_4649E0`). Writes `22` and `0xFEDBAC` into the header |
| `0x1C2AFF0` | Container signature check: u32 at `+4` equals `0x57FBAA33` and u32 at `+8` equals `0x1004FA99` |
| `0x1C2BF60` | Decompresses a container: u32 uncompressed size, then the container |
| `0x1C2BB20` | Creates the chunk stream (chunk size `0x8000`, codec id at `+72`) |
| `0x1C2B400` | Decodes one chunk through the codec table |
| `0x2945484` | Codec table, 5 dwords per codec, first dword is the decompress function. Ids and names: 0 `LZO1X_1`, 1 `LZO1X_999`, 2 `LZO2A`, 3 `LZX` |
| `0x1C3E960` | Decompress function of codec 2 (LZO2A) |
