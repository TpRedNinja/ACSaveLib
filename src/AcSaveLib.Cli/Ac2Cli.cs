using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AcSaveLib.Games.Ac2;
using AcSaveLib.Names;
using AcSaveLib.Stream;

/// <summary>CLI verbs for AC2: ac2 &lt;file&gt; summary|tree [depth] [name]|unknown|raw</summary>
public static class Ac2Cli
{
    public static int Run(string[] args)
    {
        var save = Ac2SaveFile.Load(args[1]);
        switch (args[2])
        {
            case "summary":
                Console.WriteLine($"title: {save.Title}");
                Console.WriteLine($"headerLength={save.HeaderLength} streamVersion={save.StreamVersion} magic=0x{save.StreamMagic:X8}");
                Console.WriteLine($"top-level items: {save.Root.Count}  game data: {save.HasGameData}");
                break;
            case "models":
            {
                Console.WriteLine($"title={save.Title} version={save.Version} dlcBits={save.DlcPackagesBits} changelist={save.CodeChangelist}");
                Console.WriteLine($"name={save.SaveGameName} valid={save.IsValid} corrupt={save.IsCorrupt} optionsFailed={save.LoadOrSaveOptionsFailed} options={save.IsOptions}");
                Console.WriteLine($"timePlayed={save.TimePlayed} globalSynch={save.PlayerGlobalSynch} date={DateTimeOffset.FromUnixTimeSeconds(save.SaveGameDate):u}");
                if (!save.HasGameData) { Console.WriteLine("profile-only save (no game data)"); break; }
                Console.WriteLine($"world=0x{save.CurrentWorld:X} currentMission=0x{save.CurrentMission:X} portal=0x{save.PlayerPortal:X}");
                foreach (var (label, h) in new[] { ("Narrative", save.Narrative), ("Side", save.Side) })
                {
                    var ms = h.AllMissions.ToList();
                    Console.WriteLine($"{label}: {h.Groups.Count} groups, {ms.Count} missions, {ms.Count(m => m.Completed)} completed");
                }
                var c = save.CollectedAmounts;
                Console.WriteLine($"money={save.Money} gainedItems={save.GainedInventoryItems.Count} codex={c.CodexPages} feathers={c.Feathers} statuettes={c.Statuettes} seals={c.AssassinSeals} chests={c.Chests} counters=[{string.Join(",", c.Values)}]");
                Console.WriteLine($"stats: kills={save.GameStats.Kills} earned={save.GameStats.TotalMoneyEarned} spent={save.GameStats.TotalMoneySpent} height={save.GameStats.HeightClimbed} villaValue={save.Villa.StrongholdValue}");
                Console.WriteLine($"accomplishments={save.Accomplishments.Count} completed={save.Accomplishments.Count(a => a.Completed)}");
                break;
            }
            case "missions":
                foreach (var (label, h) in new[] { ("Narrative", save.Narrative), ("Side", save.Side) })
                    foreach (var g in h.Groups)
                        foreach (var m in g.Missions)
                            Console.WriteLine($"{label} group 0x{g.Id:X} mission 0x{m.Id:X} sync={m.HighestSynchronization} completed={m.Completed} steps={m.Steps.Count} constraints={m.ConstraintCount}");
                break;
            case "tree":
            {
                int depth = args.Length > 3 ? int.Parse(args[3]) : 2;
                string? filter = args.Length > 4 ? args[4] : null;
                foreach (var n in save.Root) if (filter == null || n.Name == filter) Dump(n, 0, depth);
                break;
            }
            case "raw":
            {
                int c = 0;
                void Walk(IEnumerable<Node> ns, string path)
                {
                    foreach (var n in ns)
                    {
                        if (n.Kind == NodeKind.Raw) { c++; Console.WriteLine($"{path}/{n.Name} RAW {n.Bytes.Length}B t{n.TypeId:x4} @{n.Offset}"); }
                        Walk(n.Children, path + "/" + n.Name);
                        foreach (var e in n.Elements) Walk(e.Children, path + "/" + n.Name + "[" + e.Name + "]");
                    }
                }
                Walk(save.Root, "");
                Console.WriteLine($"{c} raw nodes");
                break;
            }
            case "unknown":
            {
                var unknown = new SortedDictionary<uint, int>();
                void Walk(IEnumerable<Node> ns)
                {
                    foreach (var n in ns)
                    {
                        if (!PropertyNames.IsKnown(n.Key)) { unknown.TryGetValue(n.Key, out var a); unknown[n.Key] = a + 1; }
                        Walk(n.Children);
                        foreach (var e in n.Elements) { if (!PropertyNames.IsKnown(e.Key)) { unknown.TryGetValue(e.Key, out var b); unknown[e.Key] = b + 1; } Walk(e.Children); }
                    }
                }
                Walk(save.Root);
                foreach (var kv in unknown) Console.WriteLine($"{kv.Key:x8}  x{kv.Value}");
                Console.WriteLine($"{unknown.Count} unknown keys");
                break;
            }
            default:
                Console.WriteLine("usage: AcSaveLib.Cli ac2 <file.save> summary|models|missions|tree [depth] [name]|raw|unknown");
                return 1;
        }
        return 0;
    }

    private static void Dump(Node n, int depth, int maxDepth)
    {
        string pad = new string(' ', depth * 2);
        switch (n.Kind)
        {
            case NodeKind.Value when n.Text != null: Console.WriteLine($"{pad}{n.Name} = \"{n.Text}\""); break;
            case NodeKind.Value: Console.WriteLine($"{pad}{n.Name} = {n.Value}{(n.Value > 9999 ? $" (0x{n.Value:X})" : "")} [t{n.TypeId:x4}]"); break;
            case NodeKind.Raw: Console.WriteLine($"{pad}{n.Name} RAW {n.Bytes.Length}B t{n.TypeId:x4}"); break;
            case NodeKind.Object:
                Console.WriteLine($"{pad}{n.Name} {{object}}");
                if (depth < maxDepth) foreach (var c in n.Children) Dump(c, depth + 1, maxDepth);
                break;
            case NodeKind.Array:
                if (n.ElementSize > 0)
                {
                    Console.WriteLine($"{pad}{n.Name}[{n.Packed.Count}] packed u{n.ElementSize * 8}: {string.Join(" ", n.Packed.Take(12).Select(v => v.ToString("X")))}{(n.Packed.Count > 12 ? " ..." : "")}");
                    break;
                }
                Console.WriteLine($"{pad}{n.Name}[{n.Elements.Count}]");
                if (depth < maxDepth)
                    foreach (var e in n.Elements) { Console.WriteLine($"{pad}  - {e.Name}"); foreach (var c in e.Children) Dump(c, depth + 2, maxDepth); }
                break;
        }
    }
}
