using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AcSaveLib.Games.Acb;
using AcSaveLib.Names;
using AcSaveLib.Stream;

/// <summary>CLI verbs for Brotherhood: acb &lt;file&gt; summary|models|missions|accomplishments|trainees [localization xml]|tree [depth] [name]|flat|unknown|named &lt;forge-index-folder&gt;</summary>
public static class AcbCli
{
    public static int Run(string[] args)
    {
        var save = AcbSaveFile.Load(args[1]);
        var h = save.Header;
        switch (args[2])
        {
            case "summary":
                Console.WriteLine($"name={h.SaveGameName} options={h.IsOptions} version={h.Version} dlcBits={h.DlcPackagesBits} changelist={h.CodeChangelist:X} saveId={h.SaveGameId}");
                Console.WriteLine($"timePlayed={h.TimePlayed}s globalSynch={h.PlayerGlobalSynch} date={DateTimeOffset.FromUnixTimeSeconds(h.SaveGameDate):u}");
                Console.WriteLine($"world=0x{h.CurrentWorld:X} currentMission=0x{h.CurrentMission:X}");
                Console.WriteLine($"containers={save.Containers.Count} [{string.Join(", ", save.Containers.Select(c => c.Length))}] header items={save.HeaderProperties.Count} root items={save.Root.Count} extra items={save.ExtraProperties.Count} game data={save.HasGameData}");
                break;
            case "models":
            {
                Console.WriteLine($"name={h.SaveGameName} timePlayed={h.TimePlayed} globalSynch={h.PlayerGlobalSynch} date={DateTimeOffset.FromUnixTimeSeconds(h.SaveGameDate):u}");
                if (save.Options != null)
                {
                    var o = save.Options;
                    Console.WriteLine($"options: subtitles={o.DisplaySubtitles} brightness={o.Brightness} skins={o.Skin01Unlocked}/{o.Skin02Unlocked}/{o.Skin03Unlocked}/{o.Skin04Unlocked} lgs={o.Lgs01Unlocked}/{o.Lgs02Unlocked}/{o.Lgs03Unlocked}/{o.Lgs04Unlocked} fanPack={o.FanPackUnlocked} sharpestBlades={o.SharpestBladesUnlocked} ezioDisguise={o.EziosDisguiseUnlocked} mansion={o.MansionUnlocked} unnamed=[{string.Join(" ", o.UnnamedValues.Select(kv => $"{kv.Key:x8}={kv.Value}"))}]");
                }
                if (!save.HasGameData) { Console.WriteLine("no game data (options file)"); break; }
                foreach (var (label, hist) in new[] { ("Narrative", save.Narrative), ("Side", save.Side) })
                {
                    var ms = hist.AllMissions.ToList();
                    Console.WriteLine($"{label}: {hist.Groups.Count} groups, {ms.Count} missions, {ms.Count(m => m.Completed)} at 100% sync, steps={ms.Sum(m => m.Steps.Count)} constraints={ms.Sum(m => m.Constraints.Count)}");
                }
                var p = save.Progression; var pl = p.Player;
                Console.WriteLine($"world=0x{save.World:X} portal=0x{save.PlayerPortal:X} money={pl?.Money} life={pl?.Life}/{pl?.MaxLife} characters={p.Characters.Count} snapshots={p.CharacterSnapshots.Count} containers={pl?.Containers.Count} gainedItems={pl?.GainedInventoryItems.Count}");
                Console.WriteLine($"collectibles=[{string.Join(",", p.QuestItemsCollectedAmounts)}] notorious={p.Notoriety.Count(n => n.IsNotorious)}/{p.Notoriety.Count} cashFightClub={p.CashFightClub}");
                var s = save.GameStats;
                Console.WriteLine($"stats: time={s.TimePlayed} kills={s.Kills} earned={s.TotalMoneyEarned} spent={s.TotalMoneySpent} climbed={s.HeightClimbed} weapons={s.WeaponsUsed.Count}");
                Console.WriteLine($"accomplishments={save.Accomplishments.Count} completed={save.Accomplishments.Count(a => a.Completed)} animus={save.AnimusEntries.Count} found={save.AnimusEntries.Count(a => a.EntryFoundOrderNumber != 0)}");
                var t = save.TraineesGuild;
                Console.WriteLine($"trainees={t.AliveTrainees.Count} contractHistory={t.ContractHistory.Count} availableContracts={t.AvailableGenericContracts.Count} recruited={t.TrackingTraineesRecruited} lost={t.TrackingTraineesLost} contracts={t.TrackingContractsSucceeded}/{t.TrackingContractsAttempted} xp={t.TrackingXpGained}");
                var w = save.WorldUpgrade; var ws = save.WorldUpgradeSnapshot;
                Console.WriteLine($"world upgrade: gained={w.GainedWorldUpgradeSettings.Count} buildings={w.Buildings.Count} rules={w.BuildingRules.Count} shops={w.Shops.Count} vault={w.CurrentGoldChestAmount} delay={w.CurrentIncomeDelay} (snapshot vault={ws.CurrentGoldChestAmount} d99bbfde={ws.Keyd99bbfde})");
                Console.WriteLine($"villa value={save.Villa.StrongholdValue} emails deleted={save.Emails.IsEmailDeleted.Count(x => x)} vr times set={save.UiTutorial.VRMissionBestTimes.Count(x => x != 0xFFFFFFFF)}");
                break;
            }
            case "missions":
            {
                var fi = args.Length > 3 ? ForgeIndex.LoadFolder(args[3]) : null;
                string D(ulong id) => fi?.Describe(id) ?? $"0x{id:X}";
                foreach (var (label, hist) in new[] { ("Narrative", save.Narrative), ("Side", save.Side) })
                    foreach (var g in hist.Groups)
                        foreach (var m in g.Missions)
                            Console.WriteLine($"{label} {D(g.Id)} {D(m.Id)} sync={m.HighestSynchronization} accepted={m.BriefingAccepted} steps={m.Steps.Count} constraints={m.Constraints.Count}");
                break;
            }
            case "accomplishments":
            {
                var fi = args.Length > 3 ? ForgeIndex.LoadFolder(args[3]) : null;
                foreach (var a in save.Accomplishments)
                    Console.WriteLine($"{(fi?.Describe(a.ObjectId) ?? a.ObjectId.ToString())} inactive={a.Inactive} completed={a.Completed} count={a.CurrentCount}");
                break;
            }
            case "trainees":
            {
                // acb <file> trainees [LocalizationPackage_English.xml]: names come from the OasisLineIDs in the save
                var loc = args.Length > 3 ? LocalizationPackage.Load(args[3]) : null;
                string N(uint id) => loc?.Describe(id) ?? id.ToString();
                foreach (var t in save.TraineesGuild.AliveTrainees)
                    Console.WriteLine($"#{t.UniqueId} name={N((uint)t.NameOasisLineId)} level={t.Level} xp={t.Xp} health={t.Health} male={t.IsMale} onContract=0x{t.CurrentContractAssignment:X} abilities={t.Abilities.Count(a => a.Value)}/{t.Abilities.Count}");
                foreach (var c in save.TraineesGuild.ContractHistory)
                    Console.WriteLine($"contract 0x{c.ContractAssignment:X} #{c.ContractCompletionOrderIndex} succeeded={c.ContractSucceeded} location={c.ContractLocation} florins={c.ContractFlorinsReward} xp={c.ContractXpReward} trainees=[{string.Join(", ", c.Trainees.Select(x => N(x.TraineeNameOasisId) + (x.Dead ? " (died)" : "")))}]");
                break;
            }
            case "tree":
            {
                int depth = args.Length > 3 ? int.Parse(args[3]) : 2;
                string? filter = args.Length > 4 ? args[4] : null;
                foreach (var n in save.Root) if (filter == null || n.Name == filter) Dump(n, 0, depth);
                break;
            }
            case "flat":
            {
                // One line per item, in stream order: path = value. Used to compare readers.
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
                Flat(save.Root, "");
                Flat(save.ExtraProperties, "X");
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
                Walk(save.HeaderProperties); Walk(save.Root); Walk(save.ExtraProperties);
                foreach (var kv in unknown) Console.WriteLine($"{kv.Key:x8}  x{kv.Value}");
                Console.WriteLine($"{unknown.Count} unknown keys");
                break;
            }
            case "named":
            {
                var fi = ForgeIndex.LoadFolder(args[3]);
                Console.WriteLine($"index: {fi.Count} ids");
                Console.WriteLine($"world={fi.Describe(h.CurrentWorld)} portal={fi.Describe(save.PlayerPortal)}");
                var pl = save.Progression.Player;
                if (pl != null) foreach (var id in pl.GainedInventoryItems) Console.WriteLine($"item {fi.Describe(id)}");
                break;
            }
            default:
                Console.WriteLine("usage: AcSaveLib.Cli acb <file.SAV> summary|models|missions [index]|accomplishments [index]|trainees [localization xml]|tree [depth] [name]|flat|unknown|named <index folder>");
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
