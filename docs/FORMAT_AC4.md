# AC4 Black Flag .save format

CONFIRMED against ACViewer values on wr.save and Sequence 3/4/5/7 saves unless marked GUESS.

## File -> property stream
1. Bytes 0..551 plaintext header: first u32 = 548, then UTF-16 title.
2. From byte 552 the body is XORed with a repeating 16-byte key aligned to the END of the file:
   key index = (i - fileLength) mod 16. Recover the key from known plaintext at 552..567:
   `23 00 00 00 | ac db fe 00 | 00 x8` (version 35, magic 0x00FEDBAC).
3. Decrypted body is chunked LZO1x. Container: signature `33 AA FB 57 99 FA 04 10` | u32 1 | `80 00 00` |
   repeated { `01` | u32 compSize | u32 uncompSize | u32 checksum (algorithm unknown, ignored) | LZO data }.
   - Container 1 at offset 576: one 246-byte block = header properties (Version, MagicNumber, SaveGameID, TimePlayed, ...).
   - 16-byte raw gap, then container 2 = main property stream (32768-byte blocks). Use the LAST container.
4. Property stream: 14 zero bytes, `f1 f8 d6 94 | u32 | 01 | u32`, root items from offset 26.
Game loader references (AC4BFSP.exe, 32-bit, base 0x400000): 0x43D1B0 writer, 0x43D4A0 reader/header check,
0x40DED0 stream init, 0xA473F0/0xA46D30 compress, 0xA47100 signature check, 0xA45990 decompress.

## MissionHistory (Narrative and Side share the class)
Identifier, GroupSaveData[] (Group u64, MissionSaveData[]), LastConcurrentStepPortal.
MissionSaveData: Mission u64, HighestSynchronization u8 (0 not complete; 100 base; 101/102 optional objectives),
TimePlayed u32 (unit unconfirmed), BriefingDisplayedOnce, BriefingAccepted, MissionStepSaveData[] (Step u64,
IsCheckpoint, Succeeded), MissionConstraintSaveData[] (ConstraintID, PlayerSelected, IsRevealed, BestRunSucceeded,
CurrentRunState enum 0 undecided/1 succeeded/2 failed, ConditionData[] = ConditionID, MetaValue).
Completed = sync >= 100. Missions not yet reached are absent.
Sample counts, wr.save: Narrative 24 groups/123 missions/121 completed; Side 21/279/238.
Notes: 0x6B945BC3C is a Group ID (not a Mission); 0x669E09D88 is not present as a Mission.

## Reference values (Sequence 3 save)
Lively Havana, ...And My Sugar?, Mister Walpole, A Man They Call the Sage: sync 102/101/102/101, times 446603/636350/491812/446764.
This Tyro Captain 0x8F9362F2C: sync 0, time 28131, steps 0x8F9362F33 and 0x8F9F3BC34 succeeded.
CurrentMission 0x8F9362F2C, World 0x373CA8CEE, PlayerPortal 0x9C0355015. wr.save: CurrentMission 0, World 0x6B39FBC72, PlayerPortal 0x6B39FBC76.
Runtime (from the .asl, not the save): CurrentMissionState byte bits 2..5 (5 in progress, 6 completed). The mission ID table is in the owner's AC4BFSP.asl.

## Open
- TimePlayed unit; chunk checksum algorithm; contents of the 16-byte gap; meaning of unfinished groups in wr.save (side 0x79f38cc30, 0xd99735086, 0x85630008e, 0x78a271d31, 0x7cdd6dd3e, 0x7529cf9bf; narrative 0x6b945bc41, all sync 0).
