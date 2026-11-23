# Games (mainline only; the list can grow, side games such as Chronicles are not planned)

| # | Short | Game | Status |
|---|---|---|---|
| 1 | AC1 | Assassin's Creed | IN PROGRESS: parse/write byte-exact, mission states, accomplishments, collectibles (see FIELDS_AC1.md) |
| 2 | AC2 | Assassin's Creed II | IN PROGRESS: parse done on 33 saves (0 unparsed items, read-only); mission history, collectibles, accomplishments, money (see FORMAT_AC2.md, FIELDS_AC2.md) |
| 3 | ACB | Assassin's Creed Brotherhood | IN PROGRESS: read-only C# reader (container, LZO2A, property stream, typed models) verified on 6 saves and 1 options file; all but 8 keys named (see FORMAT_ACB.md, FIELDS_ACB.md) |
| 4 | ACR | Assassin's Creed Revelations | IN PROGRESS: C# reader decrypts and parses game and options saves (`Games/Acr`); about 60-80 keys per save still unnamed, see FORMAT_ACR.md |
| 5 | AC3 | Assassin's Creed III (original, PC) | IN PROGRESS: C# reader decrypts (16 byte XOR) and parses eight internet saves with the Liberation stream (`Games/Ac3`, read-only); 37 to 51 keys unnamed, see FORMAT_AC3.md |
| 6 | AC3R | Assassin's Creed III Remastered | not started |
| 7 | ACL | Assassin's Creed Liberation (original, HD on PC) | IN PROGRESS: C# reader decodes containers (LZO1x) and parses the property stream of one game save and the OPTIONS file (`Games/Acl`, read-only, no typed models); about 50 keys unnamed; not detected by ACViewer (see FORMAT_ACL.md, FIELDS_ACL.md) |
| 8 | ACLR | Assassin's Creed Liberation Remastered (saves share the format of AC3R, to be verified) | not started |
| 9 | AC4 | Assassin's Creed IV Black Flag | IN PROGRESS: decrypt/decompress/parse done; all known fields have typed models (see FIELDS_AC4.md) |
| 10 | ACRogue | Assassin's Creed Rogue | not started |
| 11 | ACU | Assassin's Creed Unity | not started |
| 12 | ACS | Assassin's Creed Syndicate | not started |
| 13 | ACO | Assassin's Creed Origins | not started |
| 14 | ACOdy | Assassin's Creed Odyssey | not started |
| 15 | ACV | Assassin's Creed Valhalla | not started |
| 16 | ACM | Assassin's Creed Mirage | not started |
| 17 | ACShadows | Assassin's Creed Shadows | not started |
| 18 | ACBFR | Assassin's Creed Black Flag Resynced | not started |
