# Brotherhood (ACB) save fields

Layout and container details are in `FORMAT_ACB.md`. Tables in `docs/tables/` are generated from the six sample saves and the forge index files.
Names come from CRC32 matches to property keys; ACViewer screenshots of the no-progress save confirm all non-Snapshot fields of that save. Names found only by CRC word search are listed in `FORMAT_ACB.md`.

## ID tables
- `tables/ACB_ids.tsv`: class, id (hex and decimal), name for Mission (608), MissionGroup (84), Accomplishment (295), InventoryItemSettings (1379), EntityBuilder (214), World (26). Source: the forge `*.index.txt` files supplied by the owner. Handle values in the saves match these ids.
- `tables/ACB_missions_by_save.tsv`: HighestSynchronization per mission in each sample save (`*` = BriefingAccepted is 0; `-` = mission absent).
- `tables/ACB_accomplishments_by_save.tsv`: AccomplishmentEntrySavegame `Inactive/Completed/CurrentCount` per accomplishment id in each sample save (218 entries, all ids resolve to Accomplishment names).
- Handles that do not resolve from the supplied indexes: Step (79), ContractAssignment (44), and the keys 18190382, 91737f59, c0a01091 values.

## Populated fields seen in the 100% saves
- GameStatsSaveData: see the stats table below.
- PlayerProgSaveData: CashFacebook, CashFightClub, OverridePlayerEntityBuilder (handle; set in the 100 save to an armor preorder EntityBuilder, 0 elsewhere).
- NarrativeMissionHistory / SideMissionHistory: MissionGroupSaveData > MissionSaveData with Mission handle, HighestSynchronization, BriefingDisplayedOnce, BriefingAccepted.
- AccomplishmentManagerData: AccomplishmentEntries (ObjectID, Inactive, Completed, CurrentCount).

## Open
- Unnamed keys in game saves: 00000012, 0900aa54, 4f436d86, 55c432c6, 9ed74287, d99bbfde, dfaa4e68, ead92d83; in the options file 11 more (see `FORMAT_ACB.md`).
- Step and ContractAssignment handles.

## GameStatsSaveData against the in-game Stats menu
Confirmed on the DeathHound100 save (in-game values match the decoded values; the game screenshot was taken a few seconds after loading, so TimePlayed and the distances are 6 s and 3 m higher).
- Units: TimePlayed and TotalTimeBlended are seconds (89292 shows as 24:48:12 vs 24:48:18 in game; 881 shows as 00:14:41). Distance fields are metres.
- Overall: TimePlayed, OverallDistanceCovered.
- Fight: DeathOccurrences (Deaths), HiddenBladeAssassinations, AirAssassinations, DoubleAirAssassinations, LongestComboKillStreak (Longest execution streak), Kills (Enemies killed in fight), FromHorseAssassinations, PoisonedEnemies, KillsGun (Shot with the pistol), KillsCrossbow, Disarms.
- Navigation: HeightClimbed, DistanceOnGround, DistanceSwam, DistanceHorse, DistanceGondolas, DistanceParachute.
- Social: TotalTimeBlended, NotoriousOccurrences (Times notorious), BorgiaCourrierKills, CourtesansHired, MercenariesHired, ThievesHired.
- Economy: TotalMoneyEarned, TotalMoneySpent, TotalMoneyWonFightClub (Fight winnings), TotalMoneyLostFightClub (Fight losses).
- Assassins Guild: BAMUses (Call assassins triggered), KillsUsingBAM (Enemies killed by assassin recruits), RecruitedAssassins (Assassins recruited), DeadAssassins (Assassin recruits killed), ContractsCompleted (Contract assignments completed), MasterAssassins (Assassins sworn in).
- Memories: MemoryReplayed (Memories relived).
- Not shown in the menu: HorseRiderKills, DistanceFlown, FightClubWins, FightClubLoses.

## Economy (WorldUpgradeSaveData) against the in-game Bank menu
In-game values from the DeathHound100 save (Bank menu: total income 16680 florins per 20 minutes, 12300 shops + 3608 renovations + 772 landmarks).
- WorldEconomySaveData holds 64 WorldEconomyShopSaveData entries. The Bank menu lists 64 shops (17 blacksmith, 15 doctor, 12 tailor, 10 art merchant, 10 bank) [likely: same count].
- CurrentGoldChestAmount holds 66720 in the Snapshot copy of the DeathHound save (the main copy is 0): exactly 4 x 16680, the total income per 20 minutes [likely: the amount waiting in the vault]. Values per save, main / Snapshot: 100 save 0 / 25399, DeathHound 0 / 66720, Free Roam 0 / 0, Personal 80000 / 80000, True100 #0 0 / 0, True100 #1 16680 / 80000. 80000 appears in three copies [guess: the vault cap].
- CurrentIncomeDelay is at most 1200 in all six saves (20 minutes in seconds) [guess: seconds counted within the 20 minute income cycle].
- Per shop `dfaa4e68` and `0900aa54` are 0 in this save, and the in-game Rome Global Economy screen shows earnings 0 / investments 0 for every shop type [guess: per-shop earnings and investment totals; which field is which is unknown]. They are non-zero only in the 100 save. The game's online investment system (a mini stock market) was removed when its servers were shut down, so the investment half of the pair is probably unused in the base game.
- Not stored in the save as far as seen: income per shop or landmark, discounts (15% per shop type), vault totals.
