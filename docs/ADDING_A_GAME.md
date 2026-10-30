# Adding a game

1. Collect several saves (different progress points). Hex-dump the header: plaintext? title? length fields?
2. Encryption: look for a repeating key; try recovering it from predictable plaintext (version, magic, zeros) like XorTailCipher does. Check whether key alignment is from start or end.
3. Compression: scan the decrypted bytes for a signature; AC4 uses `33 AA FB 57 99 FA 04 10` chunked LZO. If the game differs, add a codec under Compression/.
4. Parse with Stream/PropertyReader; if the item layout differs, document the difference in docs/FORMAT_<game>.md.
5. Create Games/<Game>/<Game>SaveFile.cs mirroring Ac4SaveFile, add model classes, add a CLI verb in Program.cs.
6. Name properties: CRC32 the candidate names, add confirmed ones to Names/names.txt.
7. Write docs/FORMAT_<game>.md and docs/FIELDS_<game>.md, update docs/GAMES.md and README status table.
8. Confirm against ground truth (game UI / viewer) and say so in the notes.
