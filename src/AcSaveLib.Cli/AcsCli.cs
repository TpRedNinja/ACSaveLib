using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AcSaveLib.Games.Acs;
using AcSaveLib.Names;
using AcSaveLib.Stream;

/// <summary>CLI verbs for Syndicate: acs &lt;file&gt; summary|header|models|tree [depth] [name]|flat|unknown</summary>
public static class AcsCli
{
    public static int Run(string[] args)
    {
        var save = AcsSaveFile.Load(args[1]);
        switch (args[2])
        {
            case "summary":
                Console.WriteLine($"title: {save.Title}");
                { var h = save.Header; Console.WriteLine($"header: name={h.SaveGameName} options={h.IsOptions} saveId={h.SaveGameId} version={h.Version} dlcBits=0x{h.DlcPackagesBits:X} changelist={h.CodeChangelist} timePlayed={h.TimePlayed} globalSynch={h.PlayerGlobalSynch} date={DateTimeOffset.FromUnixTimeSeconds(h.SaveGameDate):u}"); }
                Console.WriteLine($"key:   {BitConverter.ToString(save.Key)}");
                Console.WriteLine($"containers: {save.Containers.Count} [{string.Join(", ", save.Containers.Select(c => c.Length))}] options save={save.IsOptions}");
                break;
            case "models":
                if (save.IsOptions) PrintOptions(save.Options);
                else PrintGame(save.Game);
                break;
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

    static void PrintMissions(string label, MissionHistory h)
    {
        int total = 0, nonZero = 0;
        foreach (var m in h.AllMissions) { total++; if (m.HighestSynchronization != 0) nonZero++; }
        Console.WriteLine($"{label}: {h.Groups.Count} groups, {total} missions, {nonZero} with HighestSynchronization != 0, identifier=0x{h.Identifier:X}");
    }

    static void PrintGame(AcsGameData g)
    {
        PrintMissions("narrative missions", g.Narrative);
        PrintMissions("side missions", g.Side);
        Console.WriteLine($"currentRoot={g.CurrentRoot} currentMission={g.CurrentMission} world={g.World} playerPortal={g.PlayerPortal}");
        Console.WriteLine($"animus: {g.AnimusUnlockedEntries.Count} unlocked entries, {g.AnimusNumRecentEntries} recent");
        Console.WriteLine($"accomplishments: {g.Accomplishments.Count(a => a.Completed)}/{g.Accomplishments.Count} completed");
        Console.WriteLine($"discovered icons: {g.DiscoveredIcons.Count}, crowd events: {g.CompletedCrowdEvents}");
        var pp = g.PlayerProgression;
        Console.WriteLine($"player progression: {pp.Characters.Count} characters, {pp.UncollectedInventoryItemsOnMap.Count} uncollected map items");
        for (int i = 0; i < pp.Characters.Count; i++)
        {
            var c = pp.Characters[i];
            Console.WriteLine($"  character {i} (id {c.ProgressionCharacter}): life {c.Life}/{c.MaxLife} xp {c.Xp} tier {c.Tier} creed points {c.LifetimeCreedPoints} gained items {c.GainedInventoryItems.Count} discovered missions {c.DiscoveredMissions.Count}");
        }
        Console.WriteLine($"world upgrade: gold chest {g.CurrentGoldChestAmount}, income delay {g.CurrentIncomeDelay}, pending income {g.PendingIncomeValueForAfs}");
        Console.WriteLine($"districts: {g.DistrictAssociates.Count} associates ({g.DistrictAssociates.Count(a => a.Activated)} activated), {g.FightClubCount} fight clubs, {g.CartRaceCount} cart races");
        Console.WriteLine($"statistics: {g.PlayerStatistics.Count} player, {g.HeroStatistics.Count} hero");
    }

    static void PrintOptions(AcsOptionsData o)
    {
        Console.WriteLine($"subtitles {o.DisplaySubtitles} vibration {o.Vibration} blood {o.Blood} brightness {o.Brightness} timeOfDay {o.TimeOfDay}");
        Console.WriteLine($"volume music {o.MusicVolume} voice {o.VoiceVolume} fx {o.SoundFxVolume}; camera sensitivity {o.CameraXAxisSensitivity}/{o.CameraYAxisSensitivity}");
        Console.WriteLine($"uplay rewards {o.UplayRewards} actions 0x{o.UplayActions:X} welcomeBackDone {o.UplayWelcomeBackDone}");
        Console.WriteLine($"flying machine controls {o.FlyingMachineControls}; hud flags set: {string.Join(", ", o.HudFlags.Where(kv => kv.Value).Select(kv => kv.Key))}");
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
