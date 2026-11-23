# AC3 Remastered fields

Same property stream as `FIELDS_AC3.md`; the Remastered saves contain every unnamed key of the original saves plus extra ones.

## Header (container 0)
- `C0/319e33d3` is a string holding the save label (`"100%"` in the 100% save), 0 otherwise. [likely]
- `C0/DLCPackagesBits` = 678 in the 100% save. [confirmed value, meaning unknown]

## Names
- `7107fa9a` = `EventSaveData`, found under `C1/e1dee2e4/926723b3`. [likely, 2-word CRC32 match, not seen in a viewer]
- Brute force of the keys unnamed in both games found no other match that is trustworthy (`e1dee2e4` matches only a 3-word combination, so it is not used).

## Unnamed keys
- `ac3r <file> unknown` lists them: 44 to 63 in game saves, 76 in the slot -1 profile save (mostly `C1/PlayerOptionsSaveData` entries and the `C1/e1dee2e4` and `C1/56957182` groups).
- Every key left unnamed in the original saves is also present in the Remastered saves, so the Remastered saves did not name any of them.
