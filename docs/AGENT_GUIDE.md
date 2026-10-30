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
- CRC32 dictionary brute-forcer: tools/brute.c (see below).

## Property names
Properties are stored as CRC32(name), not as text. Names come from src/AcSaveLib/Names/names.txt (confirmed only).
Unconfirmed candidates must NOT go in names.txt (put them in a separate candidates file with a note). ~120 keys
are still unnamed; see docs/FIELDS_AC4.md. A search of ACViewer's process memory for name strings found nothing.
Entity ID VALUES can be named via the ACViewer Forge index files (see FIELDS_AC4.md) - that does not name property keys.
Ways forward: owner screenshots of ACViewer, a schema file next to ACViewer, or dictionary attack
(`tools/brute.c targets.txt words.txt <maxTokens>`) with candidates verified by structure.

## Do not
- Do not edit saves in place or commit .save files (they are gitignored).
- Do not claim a unit/meaning without evidence (e.g. TimePlayed's unit is unconfirmed; chunk header checksum algorithm unknown and ignored).
