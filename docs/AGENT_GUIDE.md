# Agent guide (read first)

## Mission
Reverse-engineer every AC save format, knowing ALL fields, then expose them through this library. The owner
(TpRedNinja) builds Route Tracker (WPF speedrun route tracker). The first session laid the foundation with AC4;
you continue it and extend it to other games.

## Rules from the owner
- Guess-and-verify is fine: treat a guess as right until something contradicts it, then re-evaluate. But label it GUESS in notes until verified.
- Brute force until exhausted. Use every save you have, not just one.
- Do NOT use the "current mission" approach (inferring progress from runtime memory) - this library reads the save file.
- Notes stay clean: CONFIRMED facts only in format docs; guesses go in the "Guesses / open" sections.
- Put RE findings in the game's notes file (docs/FORMAT_<game>.md, docs/FIELDS_<game>.md).
- In C# always write `using System.IO;` explicitly. Prefer simple concrete steps when talking to the owner.
- Do not pick big next tasks unprompted; ask.

## Verification standard
"Confirmed" = a value decoded from the save equals a value the owner sees in ACViewer (a save editor/viewer) or
in game, on at least one real save. Python reference (tools/ac4save_reference.py) and the C# port were checked to
give identical mission output on 5 saves.

## Tools available in past sessions
- Cheat Engine MCP on the owner's PC (static disassembly of AC4BFSP.exe worked; hardware breakpoints never hit).
- ACViewer (owner's tool) shows decoded fields; ask for screenshots when you need ground truth.
- Python `lzallright` for quick LZO experiments.
- CRC32 dictionary brute-forcer: tools/brute.c.

## AC1
AC1 is a different format family from AC4 (big-endian, plain, path-based fields). Read docs/FORMAT_AC1.md and FIELDS_AC1.md. The owner has AC1_notes.md (RE findings, 1400+ lines), AchievementProject.md and AC1Missions.json; ACViewer does not open AC1 saves.

## Property names
Properties are stored as CRC32(name), not as text. Confirmed names live in src/AcSaveLib/Names/names.txt (~239). Unconfirmed candidates must NOT go in names.txt.
All keys in the five sample saves are resolved. How the names were found (repeat this for a new game or new fields):
1. Owner screenshots of ACViewer's decrypted-save tab (`wr_decrypted.save`) show the real property names, in tree order. Add them, then run `ac4 <file> unknown`.
2. A CRC32 match is effectively certain (32-bit hash), so dictionary guesses are verified by `ac4 crc <name>` against the unknown list.
3. Byte search across all saves finds names hiding inside RAW blobs (this is how ItemEnumValue/ItemEnumType were confirmed).
4. Enum value names: the 4 bytes after an enum value are CRC32 of its name.
5. Entity ID VALUES are named with the ACViewer Forge index files (ForgeIndex, see FIELDS_AC4.md); that does not name property keys.
Do not rely on ACViewer's Dump button (it crashes) or on process-memory string searches (found nothing).

## Do not
- Do not edit saves in place or commit .save files (they are gitignored).
- Do not claim a unit/meaning without evidence (e.g. TimePlayed's unit is unconfirmed; chunk header checksum algorithm unknown and ignored).
