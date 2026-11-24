# Games (mainline only; the list can grow, side games such as Chronicles are not planned)

| # | Short | Game | Status | Unnamed keys |
|---|---|---|---|---|
| 1 | AC1 | Assassin's Creed | DONE: parse/write byte-exact, mission states, accomplishments, collectibles, typed models for every save section (see FIELDS_AC1.md) | 3 hashes |
| 2 | AC2 | Assassin's Creed II | DONE: parse done on 33 saves (0 unparsed items, read-only); mission history, collectibles, accomplishments, money (see FORMAT_AC2.md, FIELDS_AC2.md) | 0 (in the 7 saves checked) |
| 3 | ACB | Assassin's Creed Brotherhood | DONE: read-only C# reader (container, LZO2A, property stream, typed models) verified on 6 saves and 1 options file (see FORMAT_ACB.md, FIELDS_ACB.md) | 8 (game saves), 5 (options) |
| 4 | ACR | Assassin's Creed Revelations | DONE: C# reader decrypts and parses game and options saves (`Games/Acr`), see FORMAT_ACR.md | 0 (game saves), 48 (options) |
| 5 | AC3 / AC3R | Assassin's Creed III (original, PC) and Remastered | DONE: C# reader decrypts (16 byte XOR; original key differs per pack, Remastered key is the MD5 of the Ubisoft account id) and parses eight original and six Remastered saves with the Liberation stream (`Games/Ac3`, read-only, `ac3` and `ac3r` CLI), see FORMAT_AC3.md, FORMAT_AC3R.md | original 26 to 48; Remastered 0 (game), 11 (profile) |
| 6 | ACL / ACLR | Assassin's Creed Liberation (saves decoded from the Remastered; the original is assumed to use the same format) | DONE: C# reader decodes containers (LZO1x) and parses the property stream of one game save and the OPTIONS file (`Games/Acl`, read-only, typed models); not detected by ACViewer (see FORMAT_ACL.md, FIELDS_ACL.md) | 38 (game save), 61 (OPTIONS) |
| 7 | AC4 | Assassin's Creed IV Black Flag | DONE: decrypt/decompress/parse done; all known fields have typed models (see FIELDS_AC4.md) | 0 (in the 3 saves checked) |
| 8 | ACFC | Assassin's Creed Freedom Cry (standalone, PC) | DONE: read by the AC4 reader unchanged, see FORMAT_ACFC.md | 0 (game), 11 (profile) |
| 9 | ACRogue | Assassin's Creed Rogue | DONE: encryption solved, own reader and typed models `Games/Acrogue` (see FORMAT_ACROGUE.md, FIELDS_ACROGUE.md) | 20 keys (profile saves), 8 to 9 (game saves) |
| 10 | ACU | Assassin's Creed Unity | DONE: key recovered from known plaintext, decrypt, decompress, property stream and typed models by `Games/Acu` (see FORMAT_ACU.md, FIELDS_ACU.md) | 1 (game save), 5 (options save) |
| 11 | ACS | Assassin's Creed Syndicate | DONE for the named fields: key recovered from known plaintext (MD5 of the account id, confirmed), decrypt, decompress, property stream and typed models by `Games/Acs` (see FORMAT_ACS.md, FIELDS_ACS.md) | 10 distinct (4 in an options save, 6 in game saves; 2 are Property_0x in ACViewer) |
| 12 | ACO | Assassin's Creed Origins | not started | - |
| 13 | ACOdy | Assassin's Creed Odyssey | not started | - |
| 14 | ACV | Assassin's Creed Valhalla | not started | - |
| 15 | ACM | Assassin's Creed Mirage | not started | - |
| 16 | ACShadows | Assassin's Creed Shadows | not started | - |
| 17 | ACBFR | Assassin's Creed Black Flag Resynced | not started | - |
