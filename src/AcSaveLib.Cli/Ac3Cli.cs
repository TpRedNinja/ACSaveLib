using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using AcSaveLib.Games.Ac3;
using AcSaveLib.Names;
using AcSaveLib.Stream;

/// <summary>CLI verbs for AC3 (original and Remastered): ac3 / ac3r &lt;file&gt; summary|models|flat|unknown|raw &lt;container index&gt; [bytes]</summary>
public static class Ac3Cli
{
    public static int Run(string[] args)
    {
        var save = Ac3SaveFile.Load(args[1]);
        if (args[2] == "summary") Console.WriteLine($"header=0x{save.HeaderLength:X} title=\"{save.Title}\" remastered={save.IsRemastered} id={save.HeaderId} key={Convert.ToHexString(save.Key).ToLowerInvariant()}");
        return RunSave(save, args);
    }

    public static int RunSave(Ac3SaveFile save, string[] args)
    {
        switch (args[2])
        {
            case "summary":
                Console.WriteLine($"name={save.Header.SaveGameName} options={save.Header.IsOptions} timePlayed={save.Header.TimePlayed}s\nfile={save.StreamBytes.Length} bytes containers={save.Containers.Count} [{string.Join(", ", save.Containers.Select(c => c.Length))}]");
                break;
            case "models":
                for (int i = 0; i < save.Streams.Count; i++)
                {
                    var st = save.Streams[i]; var g = st.Game;
                    Console.WriteLine($"-- stream {i + 1} root=0x{st.RootKey:X8}");
                    if (st.Options != null) Console.WriteLine($"options: subtitles={st.Options.DisplaySubtitles} brightness={st.Options.Brightness}");
                    if (!g.HasGameData) continue;
                    foreach (var (label, hist) in new[] { ("Narrative", g.Narrative), ("Side", g.Side) })
                    {
                        var ms = hist.AllMissions.ToList();
                        Console.WriteLine($"{label}: {hist.Groups.Count} groups, {ms.Count} missions, {ms.Count(m => m.Completed)} at 100% sync");
                    }
                    var pp = g.PlayerProgression; var pl = pp.Player;
                    Console.WriteLine($"CurrentMission=0x{g.CurrentMission:X} World=0x{g.World:X} PlayerPortal=0x{g.PlayerPortal:X}");
                    Console.WriteLine($"characters={pp.Characters.Count} snapshots={pp.CharacterSnapshots.Count} money={pl?.Money} life={pl?.Life}/{pl?.MaxLife}");
                    Console.WriteLine($"accomplishments={g.Accomplishments.Entries.Count} completed={g.Accomplishments.Entries.Count(e => e.Completed)} stats={g.GameStats.StatEntries.Count} animus unlocked={g.AnimusDatabase.UnlockedEntries.Count} messages={g.Messaging.Messages.Count}");
                }
                break;
            case "raw":
            {
                int n = args.Length > 3 ? int.Parse(args[3]) : 0;
                int max = args.Length > 4 ? int.Parse(args[4]) : 256;
                var c = save.Containers[n];
                for (int i = 0; i < Math.Min(c.Length, max); i += 16)
                {
                    var row = c.Skip(i).Take(16).ToArray();
                    Console.WriteLine($"{i:x4} {BitConverter.ToString(row).Replace('-', ' '),-48} {new string(row.Select(b => b >= 32 && b < 127 ? (char)b : '.').ToArray())}");
                }
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
                for (int i = 0; i < save.Items.Count; i++) Flat(save.Items[i], "C" + i);
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
                foreach (var items in save.Items) Walk(items);
                foreach (var kv in unknown) Console.WriteLine($"{kv.Key:x8}  x{kv.Value}");
                Console.WriteLine($"{unknown.Count} unknown keys");
                break;
            }
            default:
                Console.WriteLine("usage: AcSaveLib.Cli ac3 <file> summary|models|flat|unknown|raw <container> [bytes]");
                return 1;
        }
        return 0;
    }
}
