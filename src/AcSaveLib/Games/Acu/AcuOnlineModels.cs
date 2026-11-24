using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acu
{
    public sealed class AcuDistrict
    {
        public ulong Id { get; init; }
        public uint ContractMissionsCompleted { get; init; }
        public uint TreasureHuntMissionsCompleted { get; init; }
        public uint MurderMysteryMissionsCompleted { get; init; }
        public uint ChestsCollected { get; init; }
        public uint CockadeCollected { get; init; }
        public uint ArtifactCollected { get; init; }
        /// <summary>ACViewer shows this key as Property_0x521DD66A (S32).</summary>
        public int Property521DD66A { get; init; }
        public int NodeState { get; init; }
        public List<ulong> SubDistrictsUnfogged { get; init; } = new();
    }

    public sealed class AcuWorldMap
    {
        public ulong Id { get; init; }
        public List<AcuDistrict> Districts { get; init; } = new();
    }

    public sealed class AcuOnlineConstraint
    {
        public ulong Id { get; init; }
        /// <summary>Key db779010 is named Progress in ACViewer.</summary>
        public uint Progress { get; init; }
        public bool Verified { get; init; }
    }

    public sealed class AcuOnlineMission
    {
        public ulong Id { get; init; }
        public bool IsCompleted { get; init; }
        public List<AcuOnlineConstraint> Constraints { get; init; } = new();
        public List<ulong> Rewards { get; init; } = new();
    }

    public sealed class AcuGearItem
    {
        public ulong Id { get; init; }
        public uint Status { get; init; }
        public uint Quantity { get; init; }
        public bool IsUpgraded { get; init; }
    }

    public sealed class AcuGears
    {
        public uint Attack { get; init; }
        public uint Defense { get; init; }
        public uint Stealth { get; init; }
        public uint Support { get; init; }
        public uint Rating { get; init; }
        public ulong PlayStyle { get; init; }
        public List<AcuGearItem> Items { get; init; } = new();

        public static AcuGears From(Props p)
        {
            var g = new AcuGears
            {
                Attack = p.U32("Attack"), Defense = p.U32("Defense"), Stealth = p.U32("Stealth"), Support = p.U32("Support"),
                Rating = p.U32("Rating"), PlayStyle = p.U64("PlayStyle"),
            };
            foreach (var e in p.Elements("Items"))
                g.Items.Add(new AcuGearItem { Id = e.U64("Id"), Status = e.U32("Status"), Quantity = e.U32("Quantity"), IsUpgraded = e.Bool("IsUpgraded") });
            return g;
        }
    }

    public sealed class AcuMemoryCategory
    {
        public uint TitleOasisLineId { get; init; }
        public uint ProgressPercentage { get; init; }
        public uint LowSyncMedalPercentage { get; init; }
        public uint MediumSyncMedalPercentage { get; init; }
        public uint HighSyncMedalPercentage { get; init; }
        public ulong QuestLogCategoryType { get; init; }
    }

    public sealed class AcuPlayerProfile
    {
        public uint ClanId { get; init; }
        public ulong ClanTag { get; init; }
        public AcuGears Gears { get; init; } = new();
        public List<AcuMemoryCategory> MemoryCategories { get; init; } = new();
        public int BoosterPurchaseCount { get; init; }
        public ulong FavoriteActivity { get; init; }
        public uint PlayerLevel { get; init; }
        public uint PlayerTitle { get; init; }
        public ulong ClubWarDivision { get; init; }
        public ulong FavoriteCoopSkill { get; init; }
        public ulong PlayerPlayStyle { get; init; }
    }

    /// <summary>OnlineGameEntitySaveData: the online-service view of the player (world map progress, missions, gears, profile).</summary>
    public sealed class AcuOnlineGameEntity
    {
        public List<AcuWorldMap> Maps { get; init; } = new();
        public List<ulong> Hdbs { get; init; } = new();
        public ulong InProgressMissionId { get; init; }
        public List<ulong> CompanionRewardsClaimed { get; init; } = new();
        public List<AcuOnlineMission> Missions { get; init; } = new();
        public AcuGears Gears { get; init; } = new();
        public uint GlitchCollectedLocal { get; init; }
        public uint GlitchCollectedGlobal { get; init; }
        public uint BookmarkVersion { get; init; }
        public AcuPlayerProfile Profile { get; init; } = new();

        public static AcuOnlineGameEntity From(Props p)
        {
            var maps = new List<AcuWorldMap>();
            foreach (var m in p.Obj("WorldMapOnlineEntity").Elements("Maps"))
            {
                var map = new AcuWorldMap { Id = m.U64("Id") };
                foreach (var d in m.Elements("Districts"))
                    map.Districts.Add(new AcuDistrict
                    {
                        Id = d.U64("Id"), ContractMissionsCompleted = d.U32("ContractMissionsCompleted"),
                        TreasureHuntMissionsCompleted = d.U32("TreasureHuntMissionsCompleted"), MurderMysteryMissionsCompleted = d.U32("MurderMysteryMissionsCompleted"),
                        ChestsCollected = d.U32("ChestsCollected"), CockadeCollected = d.U32("CockadeCollected"), ArtifactCollected = d.U32("ArtifactCollected"),
                        Property521DD66A = unchecked((int)(uint)(d.GetByKey(0x521DD66A)?.Value ?? 0)), NodeState = d.S32("NodeState"),
                        SubDistrictsUnfogged = d.Packed("SubDistrictsUnfogged"),
                    });
                maps.Add(map);
            }
            var prog = p.Obj("PlayerProgressionOnlineEntity");
            var missions = new List<AcuOnlineMission>();
            foreach (var m in prog.Elements("Missions"))
            {
                var om = new AcuOnlineMission { Id = m.U64("Id"), IsCompleted = m.Bool("IsCompleted"), Rewards = m.Packed("Rewards") };
                foreach (var c in m.Elements("Constraints"))
                    om.Constraints.Add(new AcuOnlineConstraint { Id = c.U64("Id"), Progress = c.U32("Progress"), Verified = c.Bool("Verified") });
                missions.Add(om);
            }
            var prof = p.Obj("PlayerProfileOnlineEntity");
            var cats = new List<AcuMemoryCategory>();
            foreach (var c in prof.Elements("MemoryCategories"))
                cats.Add(new AcuMemoryCategory
                {
                    TitleOasisLineId = c.Obj("Title").U32("OasisLineID"), ProgressPercentage = c.U32("ProgressPercentage"),
                    LowSyncMedalPercentage = c.U32("LowSyncMedalPercentage"), MediumSyncMedalPercentage = c.U32("MediumSyncMedalPercentage"),
                    HighSyncMedalPercentage = c.U32("HighSyncMedalPercentage"), QuestLogCategoryType = c.U64("QuestLogCategoryType"),
                });
            var clan = prof.Obj("Clan");
            return new AcuOnlineGameEntity
            {
                Maps = maps, Hdbs = prog.Packed("Hdbs"), InProgressMissionId = prog.U64("inProgressMissionId"),
                CompanionRewardsClaimed = prog.Packed("CompanionRewardsClaimed"), Missions = missions, Gears = AcuGears.From(prog.Obj("Gears")),
                GlitchCollectedLocal = prog.U32("GlitchCollectedLocal"), GlitchCollectedGlobal = prog.U32("GlitchCollectedGlobal"),
                BookmarkVersion = p.Obj("BookmarkOnlineEntity").U32("Version"),
                Profile = new AcuPlayerProfile
                {
                    ClanId = clan.U32("Id"), ClanTag = clan.U64("Tag"), Gears = AcuGears.From(prof.Obj("Gears")), MemoryCategories = cats,
                    BoosterPurchaseCount = prof.ElementCount("BoosterPurchaseHistory"), FavoriteActivity = prof.U64("FavoriteActivity"),
                    PlayerLevel = prof.U32("PlayerLevel"), PlayerTitle = prof.U32("PlayerTitle"), ClubWarDivision = prof.U64("ClubWarDivision"),
                    FavoriteCoopSkill = prof.U64("FavoriteCoopSkill"), PlayerPlayStyle = prof.U64("PlayerPlayStyle"),
                },
            };
        }
    }
}
