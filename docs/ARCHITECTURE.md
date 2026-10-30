# Architecture

```
src/AcSaveLib/
  Crypto/XorTailCipher.cs        RecoverKey(known plaintext) / Apply: repeating-key XOR aligned to END of file
  Compression/Lzo1x.cs           LZO1x decompressor (port of the reference algorithm)
  Container/ChunkContainer.cs    Ubisoft chunk container: signature, Read, ReadAll
  Names/PropertyNames.cs         CRC32 -> name lookup (+ Crc32 helper), names.txt embedded resource
  Names/names.txt                confirmed property names only
  Stream/Node.cs, PropertyReader.cs   generic property-stream tree (game independent)
  Games/Ac4/Ac4SaveFile.cs       AC4 pipeline: header, decrypt, unpack, parse, top-level fields
  Games/Ac4/MissionModels.cs     typed Mission/Group/History classes
src/AcSaveLib.Cli/Program.cs     inspection CLI
tools/                           Python reference implementation and brute-forcer
```
Layers: Crypto -> Container/Compression -> Stream (generic) -> Games/<Game> (typed models). Everything below
Games/ should be reusable by other titles; game-specific constants (offsets, key plaintext) live in Games/<Game>.
Target net8.0, nullable enabled, implicit usings disabled (write usings explicitly).
