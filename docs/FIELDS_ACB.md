# Brotherhood (ACB) save fields

Layout and container details are in `FORMAT_ACB.md`. Tables in `docs/tables/` are generated from the six sample saves and the forge index files.
Names come from CRC32 matches to property keys; ACViewer screenshots of the no-progress save confirm all non-Snapshot fields of that save. Names found only by CRC word search are listed in `FORMAT_ACB.md`.

## ID tables
- `tables/ACB_ids.tsv`: class, id (hex and decimal), name for Mission (608), MissionGroup (84), Accomplishment (295), InventoryItemSettings (1379), EntityBuilder (214), World (26). Source: the forge `*.index.txt` files supplied by the owner. Handle values in the saves match these ids.
- `tables/ACB_missions_by_save.tsv`: HighestSynchronization per mission in each sample save (`*` = BriefingAccepted is 0; `-` = mission absent).
- `tables/ACB_accomplishments_by_save.tsv`: AccomplishmentEntrySavegame `Inactive/Completed/CurrentCount` per accomplishment id in each sample save (218 entries, all ids resolve to Accomplishment names).
- Handles that do not resolve from the supplied indexes: Step (79), ContractAssignment (44), and the keys 18190382, 91737f59, c0a01091 values.

## Populated fields seen in the 100% saves
- GameStatsSaveData: counters (TimePlayed, Kills, KillsCrossbow, KillsGun, HiddenBladeAssassinations, AirAssassinations, TotalMoneyEarned, TotalMoneySpent, Distance* ...) are populated. Units of TimePlayed and the Distance* values are not known.
- PlayerProgSaveData: CashFacebook, CashFightClub, OverridePlayerEntityBuilder (handle; set in the 100 save to an armor preorder EntityBuilder, 0 elsewhere).
- NarrativeMissionHistory / SideMissionHistory: MissionGroupSaveData > MissionSaveData with Mission handle, HighestSynchronization, BriefingDisplayedOnce, BriefingAccepted.
- AccomplishmentManagerData: AccomplishmentEntries (ObjectID, Inactive, Completed, CurrentCount).

## Open
- Unnamed keys: 00000012, 03030303, 0900aa54, 097a6081, 4f436d86, 55c432c6, 5f03fd21, 9ed74287, c0a01091, d99bbfde, dfaa4e68, ead92d83.
- Step and ContractAssignment handles, units of stats.
