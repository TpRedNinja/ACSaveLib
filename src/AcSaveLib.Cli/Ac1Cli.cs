using System;
using System.IO;
using System.Linq;
using AcSaveLib.Games.Ac1;

// Usage:
//   AcSaveLib.Cli ac1 <file.sav> summary | roundtrip | models | object <classId> | unknown
internal static class Ac1Cli
{
    public static int Run(string[] args)
    {
        var bytes = File.ReadAllBytes(args[1]);
        var save = Ac1SaveFile.Parse(bytes);
        switch (args[2])
        {
            case "roundtrip":
                Console.WriteLine($"{save.Objects.Count} objects, round trip exact: {save.Write().AsSpan().SequenceEqual(bytes)}");
                break;
            case "summary":
                Console.WriteLine($"version {save.Version}, {save.Objects.Count} objects, {save.Objects.Sum(o => o.Fields.Count)} fields");
                break;
            case "models":
            {
                var items = save.MissionItems();
                Console.WriteLine($"mission items: {items.Count}, done: {items.Count(m => m.Done)}, active: {items.Count(m => m.State == Ac1MissionState.Active)}");
                foreach (var m in items.Where(m => m.Done).Take(args.Length > 3 ? int.Parse(args[3]) : 0)) Console.WriteLine($"  done: {m.Name}");
                var acc = save.Accomplishments();
                Console.WriteLine($"accomplishments: {acc.Count} slots in {acc.Select(a => a.Config).Distinct().Count()} configs");
                foreach (var a in acc.Where(a => a.Config == 1))
                    Console.WriteLine($"  [{a.Slot,2}] {(a.Completed ? "DONE" : "....")} {a.Info?.Title,-36} resolved={a.IsResolved} count={a.Count?.ToString() ?? "-"} used={a.UsedEvents.Count}");
                foreach (var c in save.Collectibles()) Console.WriteLine($"  {c.Category}: {c.Collected}/{c.Needed}");
                Console.WriteLine("PersistedIDs: " + string.Join(", ", save.PersistedIds().Select(i => i.ToString("X8"))));
                var map = save.MapData();
                if (map != null)
                {
                    Console.WriteLine("MapManager: cities known " + string.Join(", ", map.CityKnown.Select((k, i) => $"{(i < Ac1MapData.CityNames.Count ? Ac1MapData.CityNames[i] : i.ToString())}={k}")));
                    Console.WriteLine($"  compass known={map.DefaultCompassMarker.IsKnown}, altair known={map.AltairMarker.IsKnown}, set-by-player known={map.SetByPlayerMarker.IsKnown} type={map.SetByPlayerMarker.MarkerType} pos=({string.Join(", ", map.SetByPlayerMarker.MarkerPosition)})");
                }
                var dna = save.DnaData();
                if (dna != null) Console.WriteLine($"DNAMemory: sync blocks {dna.NumSynchBlocks}/{dna.TotalNumSynchBlocks}, unlocked {dna.UnlockedBlocksCount}, optional objectives {dna.OptionalObjectivesCount}, override={dna.NothingIsTrueEverythingIsPermitted}");
                var sg = save.SaveGameData();
                if (sg != null)
                {
                    Console.WriteLine($"SaveGameData: current world {sg.CurrentSaveWorldId:X8} ({sg.CurrentSaveWorldName}), main {sg.MainSaveWorldId:X8} ({sg.MainSaveWorldName}), always {sg.AlwaysSaveWorldId:X8} ({sg.AlwaysSaveWorldName}), memory block {sg.CurrentMemoryBlock}");
                    Console.WriteLine($"  rank {sg.AltairCurrentRank} -> {sg.AltairNextRank}, weapon {sg.SelectedWeapon}, daggers belt/shoulder/leg {sg.DaggerBeltCount}/{sg.DaggerShoulderCount}/{sg.DaggerLegCount}, matrix tail ({string.Join(", ", sg.AssassinMatrix.Skip(12))})");
                }
                var horse = save.HorseData();
                if (horse != null) Console.WriteLine($"HorseData: chosen={horse.MountIsChosen}, inWorld={horse.MountIsInWorld}, spawnMode={horse.SpawnMode}, mountIndex={horse.MountIndex}, onMount={horse.AssassinOnMount}");
                var tut = save.TutorialData();
                if (tut != null) Console.WriteLine($"TutorialManager: informers {tut.FreeMissionInformerCount}, scholars {tut.FreeMissionScholarsCount}, vigilantes {tut.FreeMissionVigilantesCount}");
                foreach (var mo in save.MapObjects())
                    foreach (var c in mo.Components)
                        Console.WriteLine($"Map object {mo.ClassId}: component {c.Index}, markers {c.Markers.Count(x => x)}/{c.Markers.Count}, automatic {c.AutomaticMarkers.Count(x => x)}/{c.AutomaticMarkers.Count}, fog cells {c.FogCells.Length}, all unfogged={c.IsAllUnfogged}");
                Console.WriteLine($"mission extras: marker known {items.Count(m => m.MissionMarkerKnown == true)}, scenes played {items.Count(m => m.SceneAlreadyPlayed == true)}, briefed {items.Count(m => m.ObjectiveBriefed == true)}, assassinations done {items.Count(m => m.AssassinationDone == true)}");
                break;
            }
            case "unknown":
                foreach (var o in save.Objects.Where(o => !Ac1Names.IsKnownClass(o.ClassId))) Console.WriteLine($"class {o.ClassId} unnamed");
                var seen = new System.Collections.Generic.SortedSet<uint>();
                foreach (var o in save.Objects) foreach (var f in o.Fields) foreach (var s in f.Path) if (!Ac1Names.IsKnownProperty(s.NameHash)) seen.Add(s.NameHash);
                foreach (var h in seen) Console.WriteLine($"{h} ({h:x8})");
                Console.WriteLine($"{seen.Count} unknown property hashes");
                break;
            case "object":
                uint id = args[3].StartsWith("0x") ? Convert.ToUInt32(args[3], 16) : uint.Parse(args[3]);
                var obj = save.Find(id);
                if (obj == null) { Console.WriteLine("not found"); return 1; }
                foreach (var f in obj.Fields)
                    Console.WriteLine($"{f.PathText} type={f.Type}{(f.IsArray ? $" arr[{f.Elements().Count}] elem={f.ElementType}" : "")} = {(f.IsArray ? "" : BitConverter.ToString(f.Raw))}");
                break;
            default: Console.WriteLine("usage: ac1 <file> summary|roundtrip|object <id>|unknown"); return 1;
        }
        return 0;
    }
}
