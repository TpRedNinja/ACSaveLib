using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AcSaveLib.Games.Acr;
using AcSaveLib.Names;
using AcSaveLib.Stream;

/// <summary>CLI verbs for Revelations: acr &lt;file&gt; summary|models|tree [depth] [name]|flat|unknown [--user legacy_name]</summary>
public static class AcrCli
{
    public static int Run(string[] args)
    {
        // optional: --user <legacy username> (otherwise the key is recovered from the file)
        int u = Array.IndexOf(args, "--user");
        var save = AcrSaveFile.Load(args[1], u >= 0 && u + 1 < args.Length ? args[u + 1] : null);
        var h = save.Header;
        switch (args[2])
        {
            case "summary":
                Console.WriteLine($"title={save.Title} id={save.HeaderId} encrypted={save.WasEncrypted} headerLength=0x{save.HeaderLength:X}");
                Console.WriteLine($"name={h.SaveGameName} options={h.IsOptions} saveId={h.SaveGameId} dlcBits={h.DlcPackagesBits} timePlayed={h.TimePlayed}s globalSynch={h.PlayerGlobalSynch} date={DateTimeOffset.FromUnixTimeSeconds(h.SaveGameDate):u}");
                Console.WriteLine($"world=0x{h.CurrentWorld:X} currentMission=0x{h.CurrentMission:X}");
                Console.WriteLine($"containers={save.Containers.Count} [{string.Join(", ", save.Containers.Select(c => c.Length))}]");
                for (int i = 0; i < save.Streams.Count; i++)
                    Console.WriteLine($"stream {i + 1}: root=0x{save.Streams[i].RootKey:X8} {save.Streams[i].Length} bytes, {save.Streams[i].Items.Count} items, game data={save.Streams[i].Game.HasGameData}");
                break;
            case "models":
                for (int i = 0; i < save.Streams.Count; i++)
                {
                    var s = save.Streams[i]; var g = s.Game;
                    Console.WriteLine($"-- stream {i + 1} root=0x{s.RootKey:X8}");
                    if (s.Options != null) Console.WriteLine($"options: subtitles={s.Options.DisplaySubtitles} brightness={s.Options.Brightness} skins={s.Options.Skin01Unlocked}/{s.Options.Skin02Unlocked}/{s.Options.Skin03Unlocked}/{s.Options.Skin04Unlocked}");
                    if (!g.HasGameData) continue;
                    foreach (var (label, hist) in new[] { ("Narrative", g.Narrative), ("Side", g.Side) })
                    {
                        var ms = hist.AllMissions.ToList();
                        Console.WriteLine($"{label}: {hist.Groups.Count} groups, {ms.Count} missions, {ms.Count(m => m.Completed)} at 100% sync");
                    }
                    var pl = g.Progression.Player;
                    var st = g.GameStats;
                    Console.WriteLine($"world=0x{g.World:X} portal=0x{g.PlayerPortal:X} money={pl?.Money} life={pl?.Life}/{pl?.MaxLife} characters={g.Progression.Characters.Count} collectibles=[{string.Join(",", g.Progression.QuestItemsCollectedAmounts)}]");
                    Console.WriteLine($"stats: time={st.TimePlayed} kills={st.Kills} earned={st.TotalMoneyEarned} spent={st.TotalMoneySpent} climbed={st.HeightClimbed}");
                    Console.WriteLine($"accomplishments={g.Accomplishments.Count} completed={g.Accomplishments.Count(a => a.Completed)} animus={g.AnimusEntries.Count} trainees={g.TraineesGuild.AliveTrainees.Count} contractHistory={g.TraineesGuild.ContractHistory.Count}");
                    Console.WriteLine($"world upgrade: gained={g.WorldUpgrade.GainedWorldUpgradeSettings.Count} buildings={g.WorldUpgrade.Buildings.Count} shops={g.WorldUpgrade.Shops.Count}");
                }
                break;
            case "tree":
            {
                int depth = args.Length > 3 ? int.Parse(args[3]) : 2;
                string? filter = args.Length > 4 ? args[4] : null;
                foreach (var s in save.Streams) foreach (var n in s.Items) if (filter == null || n.Name == filter) Dump(n, 0, depth);
                break;
            }
            case "flat":
            {
                void Flat(IEnumerable<Node> ns, string path)
                {
                    foreach (var n in ns)
                    {
                        string p = path + "/" + n.Name;
                        switch (n.Kind)
                        {
                            case NodeKind.Value: Console.WriteLine($"{p} = {(n.Text != null ? "\"" + n.Text + "\"" : n.Value.ToString())}"); break;
                            case NodeKind.Raw: Console.WriteLine($"{p} RAW {n.Bytes.Length}"); break;
                            case NodeKind.Object: Console.WriteLine($"{p} {{}}"); Flat(n.Children, p); break;
                            case NodeKind.Array:
                                if (n.ElementSize > 0) Console.WriteLine($"{p} packed{n.ElementSize} [{string.Join(",", n.Packed)}]");
                                else
                                {
                                    Console.WriteLine($"{p} [{n.Elements.Count}]");
                                    for (int i = 0; i < n.Elements.Count; i++) { Console.WriteLine($"{p}[{i}] {n.Elements[i].Name}"); Flat(n.Elements[i].Children, $"{p}[{i}]"); }
                                }
                                break;
                        }
                    }
                }
                Flat(save.HeaderProperties, "H");
                for (int i = 0; i < save.Streams.Count; i++) Flat(save.Streams[i].Items, $"S{i + 1}");
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
                Walk(save.HeaderProperties);
                foreach (var s in save.Streams) Walk(s.Items);
                foreach (var kv in unknown) Console.WriteLine($"{kv.Key:x8}  x{kv.Value}");
                Console.WriteLine($"{unknown.Count} unknown keys");
                break;
            }
            default:
                Console.WriteLine("usage: AcSaveLib.Cli acr <file.save> summary|models|tree [depth] [name]|flat|unknown");
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
