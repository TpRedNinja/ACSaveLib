using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AcSaveLib.Games.Acrogue;
using AcSaveLib.Names;
using AcSaveLib.Stream;

/// <summary>CLI verbs for Rogue: acrogue &lt;file&gt; summary|models|header|tree [depth] [name]|flat|unknown</summary>
public static class AcrogueCli
{
    public static int Run(string[] args)
    {
        var save = AcrogueSaveFile.Load(args[1]);
        switch (args[2])
        {
            case "summary":
                Console.WriteLine($"title: {save.Title}");
                Console.WriteLine($"key:   {BitConverter.ToString(save.Key)}");
                Console.WriteLine($"containers: {save.Containers.Count} [{string.Join(", ", save.Containers.Select(c => c.Length))}] game save={save.IsGameSave}");
                break;
            case "models":
            {
                var g = save.Game; var h = save.Header;
                Console.WriteLine($"header: options={h.IsOptions} saveId={h.SaveGameId} timePlayed={h.TimePlayed} globalSynch={h.PlayerGlobalSynch} date={DateTimeOffset.FromUnixTimeSeconds(h.SaveGameDate):u}");
                if (!g.HasGameData) { Console.WriteLine("no game data (profile save)"); break; }
                foreach (var (label, hist) in new[] { ("Narrative", g.Narrative), ("Side", g.Side) })
                {
                    var ms = hist.AllMissions.ToList();
                    Console.WriteLine($"{label}: {hist.Groups.Count} groups, {ms.Count} missions, {ms.Count(m => m.Completed)} completed");
                }
                var pp = g.PlayerProgression; var pl = pp.Player;
                Console.WriteLine($"CurrentMission=0x{g.CurrentMission:X} World=0x{g.World:X} PlayerPortal=0x{g.PlayerPortal:X} ContextId={g.ContextId}");
                Console.WriteLine($"Characters={pp.Characters.Count} snapshots={pp.CharacterSnapshots.Count} money={pl?.Money} life={pl?.Life}/{pl?.MaxLife} shipLife={pl?.ShipLife}");
                Console.WriteLine($"Accomplishments={g.Accomplishments.Entries.Count} completed={g.Accomplishments.Entries.Count(e => e.Completed)} Stats={g.GameStats.StatEntries.Count}");
                Console.WriteLine($"Animus unlocked={g.AnimusDatabase.UnlockedEntries.Count} MapIcon worlds={g.MapIcons.SavedIcons.Count} Messages={g.Messaging.Messages.Count} Rewards={g.PresentRewards.RewardItems.Count} Fleet boats={g.Fleet.CapturedBoats.Count}");
                break;
            }
            case "header":
                foreach (var h in save.HeaderProperties) Console.WriteLine($"{h.Key:x8} {h.Name} = {(h.Text ?? h.Value.ToString())}");
                break;
            case "tree":
            {
                int depth = args.Length > 3 ? int.Parse(args[3]) : 2;
                string? filter = args.Length > 4 ? args[4] : null;
                foreach (var n in save.ProfileRoot.Concat(save.Root)) if (filter == null || n.Name == filter) Dump(n, 0, depth);
                break;
            }
            case "flat":
            {
                void Flat(IEnumerable<Node> ns, string path)
                {
                    foreach (var n in ns)
                    {
                        string p = path + "/" + n.Name;
                        if (n.Kind == NodeKind.Object) Flat(n.Children, p);
                        else if (n.Kind == NodeKind.Array && n.ElementSize == 0)
                            for (int i = 0; i < n.Elements.Count; i++) Flat(n.Elements[i].Children, $"{p}[{i}]");
                        else Console.WriteLine($"{p} = {(n.Text != null ? "\"" + n.Text + "\"" : n.Kind == NodeKind.Array ? "packed[" + n.Packed.Count + "]" : n.Kind == NodeKind.Raw ? "raw " + n.Bytes.Length : n.Value.ToString())}");
                    }
                }
                Flat(save.ProfileRoot.Concat(save.Root), "");
                break;
            }
            case "unknown":
            {
                var unknown = new SortedDictionary<uint, int>();
                void Walk(IEnumerable<Node> ns)
                {
                    foreach (var n in ns)
                    {
                        if (!PropertyNames.IsKnown(n.Key)) { unknown.TryGetValue(n.Key, out var c0); unknown[n.Key] = c0 + 1; }
                        Walk(n.Children);
                        foreach (var e in n.Elements) { if (!PropertyNames.IsKnown(e.Key)) { unknown.TryGetValue(e.Key, out var c1); unknown[e.Key] = c1 + 1; } Walk(e.Children); }
                    }
                }
                Walk(save.Root); Walk(save.ProfileRoot); Walk(save.HeaderProperties);
                foreach (var kv in unknown) Console.WriteLine($"{kv.Key:x8}  x{kv.Value}");
                Console.WriteLine($"{unknown.Count} unknown keys");
                break;
            }
        }
        return 0;
    }

    static void Dump(Node n, int depth, int maxDepth)
    {
        string pad = new string(' ', depth * 2);
        switch (n.Kind)
        {
            case NodeKind.Value when n.Text != null: Console.WriteLine($"{pad}{n.Name} = \"{n.Text}\""); break;
            case NodeKind.Value: Console.WriteLine($"{pad}{n.Name} = {n.Value}{(n.Value > 9999 ? $" (0x{n.Value:X})" : "")}"); break;
            case NodeKind.Raw: Console.WriteLine($"{pad}{n.Name} RAW {n.Bytes.Length}B"); break;
            case NodeKind.Object:
                Console.WriteLine($"{pad}{n.Name} {{object}}");
                if (depth < maxDepth) foreach (var c in n.Children) Dump(c, depth + 1, maxDepth);
                break;
            case NodeKind.Array:
                if (n.ElementSize > 0) { Console.WriteLine($"{pad}{n.Name}[{n.Packed.Count}] packed: {string.Join(" ", n.Packed.Take(12).Select(v => v.ToString("X")))}"); break; }
                Console.WriteLine($"{pad}{n.Name}[{n.Elements.Count}]");
                if (depth < maxDepth)
                    foreach (var e in n.Elements) { Console.WriteLine($"{pad}  - {e.Name}"); foreach (var c in e.Children) Dump(c, depth + 2, maxDepth); }
                break;
        }
    }
}
