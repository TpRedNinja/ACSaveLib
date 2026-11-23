# Games (mainline only; the list can grow, side games such as Chronicles are not planned)

| # | Short | Game | Status | Unnamed keys |
|---|---|---|---|---|
| 1 | AC1 | Assassin's Creed | DONE: parse/write byte-exact, mission states, accomplishments, collectibles (see FIELDS_AC1.md) | 3 hashes |
| 2 | AC2 | Assassin's Creed II | DONE: parse done on 33 saves (0 unparsed items, read-only); mission history, collectibles, accomplishments, money (see FORMAT_AC2.md, FIELDS_AC2.md) | 0 (in the 7 saves checked) |
| 3 | ACB | Assassin's Creed Brotherhood | DONE: read-only C# reader (container, LZO2A, property stream, typed models) verified on 6 saves and 1 options file (see FORMAT_ACB.md, FIELDS_ACB.md) | 8 (game saves), 5 (options) |
| 4 | ACR | Assassin's Creed Revelations | DONE: C# reader decrypts and parses game and options saves (`Games/Acr`), see FORMAT_ACR.md | 0 (game saves), 48 (options) |
| 5 | AC3 | Assassin's Creed III (original, PC) | DONE: C# reader decrypts (16 byte XOR) and parses eight internet saves with the Liberation stream (`Games/Ac3`, read-only), see FORMAT_AC3.md | 26 to 48 |
| 6 | AC3R | Assassin's Creed III Remastered | DONE: the AC3 reader decrypts and parses six saves (same layout, 0x224 header, 16 byte XOR), `ac3r` CLI alias, see FORMAT_AC3R.md | 0 to 3 (game), 14 (profile) |
| 7 | ACL | Assassin's Creed Liberation (original, HD on PC) | DONE: C# reader decodes containers (LZO1x) and parses the property stream of one game save and the OPTIONS file (`Games/Acl`, read-only, no typed models); not detected by ACViewer (see FORMAT_ACL.md, FIELDS_ACL.md) | 38 (game save), 61 (OPTIONS) |
| 8 | ACLR | Assassin's Creed Liberation Remastered (saves share the format of AC3R, to be verified) | not started | - |
| 9 | AC4 | Assassin's Creed IV Black Flag | DONE: decrypt/decompress/parse done; all known fields have typed models (see FIELDS_AC4.md) | 0 (in the 3 saves checked) |
| 10 | ACRogue | Assassin's Creed Rogue | not started | - |
| 11 | ACU | Assassin's Creed Unity | not started | - |
| 12 | ACS | Assassin's Creed Syndicate | not started | - |
| 13 | ACO | Assassin's Creed Origins | not started | - |
| 14 | ACOdy | Assassin's Creed Odyssey | not started | - |
| 15 | ACV | Assassin's Creed Valhalla | not started | - |
| 16 | ACM | Assassin's Creed Mirage | not started | - |
| 17 | ACShadows | Assassin's Creed Shadows | not started | - |
| 18 | ACBFR | Assassin's Creed Black Flag Resynced | not started | - |
