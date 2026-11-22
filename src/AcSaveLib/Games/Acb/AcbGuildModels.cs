using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acb
{
    // TraineesGuildManagerSaveData: the recruited assassins and the contract (assignment) history. Names confirmed in ACViewer.

    public sealed class AcbTrainee
    {
        public uint UniqueId { get; init; }
        public int NameOasisLineId { get; init; }
        public uint DescriptionOasisLineId { get; init; }
        public byte Level { get; init; }
        public uint Skill { get; init; }
        public uint Xp { get; init; }
        public uint UpgradePoints { get; init; }
        public uint Health { get; init; }
        public bool IsMale { get; init; }
        /// <summary>ContractAssignment handle of the contract the trainee is on (0 = none).</summary>
        public ulong CurrentContractAssignment { get; init; }
        public byte HeadIndex { get; init; }
        public byte EyesColorIndex { get; init; }
        public byte MaskIndex { get; init; }
        public byte ColorIndex { get; init; }
        /// <summary>The 24 Bool abilities and proficiencies by name (CounterDodgeAbility ... ThrowSmokeBombAbility), in stored order.</summary>
        public Dictionary<string, bool> Abilities { get; init; } = new();
        public bool UsePresetUpgrades { get; init; }

        public static readonly string[] AbilityNames =
        {
            "CounterDodgeAbility", "KickAttackAbility", "CounterKillAbility", "ComboAttackAbility", "TauntAbility", "SpecialAttackAbility",
            "WeaponU1Proficiency", "WeaponU2Proficiency", "WeaponU3Proficiency", "WeaponU4Proficiency", "WeaponU5Proficiency",
            "ArmorU1Proficiency", "ArmorU2Proficiency", "ArmorU3Proficiency", "ArmorU4Proficiency", "ArmorU5Proficiency",
            "HiddenBladeAbility", "SpecialAssassinationTechniqueAbility", "ThrowKnivesAbility", "CrossbowAbility", "GunAbility", "ThrowSmokeBombAbility",
        };

        public static AcbTrainee From(Props p)
        {
            var t = new AcbTrainee
            {
                UniqueId = p.U32("TraineeUniqueID"), NameOasisLineId = p.S32("NameOasisLineID"), DescriptionOasisLineId = p.U32("DescriptionOasisLineID"),
                Level = p.U8("Level"), Skill = p.U32("Skill"), Xp = p.U32("XP"), UpgradePoints = p.U32("UpgradePoints"), Health = p.U32("Health"),
                IsMale = p.Bool("IsMale"), CurrentContractAssignment = p.U64("CurrentContractAssignment"), HeadIndex = p.U8("HeadIndex"),
                EyesColorIndex = p.U8("EyesColorIndex"), MaskIndex = p.U8("MaskIndex"), ColorIndex = p.U8("ColorIndex"),
                UsePresetUpgrades = p.Bool("UsePresetUpgrades"),
            };
            foreach (var n in AbilityNames) t.Abilities[n] = p.Bool(n);
            return t;
        }
    }

    public sealed class AcbContractTrainee
    {
        public uint TraineeUniqueId { get; init; }
        /// <summary>TextureMapSpec handle of the portrait.</summary>
        public ulong ThumbnailImage { get; init; }
        public uint TraineeNameOasisId { get; init; }
        public byte Level { get; init; }
        public bool Dead { get; init; }
    }

    public sealed class AcbContractHistory
    {
        /// <summary>ContractAssignment handle (does not resolve from the supplied forge index files).</summary>
        public ulong ContractAssignment { get; init; }
        public bool ContractSucceeded { get; init; }
        public uint ContractDifficulty { get; init; }
        public uint ContractDuration { get; init; }
        public uint ContractXpReward { get; init; }
        public uint ContractFlorinsReward { get; init; }
        public int ContractLocation { get; init; }
        public byte ContractCompletionOrderIndex { get; init; }
        public List<AcbContractTrainee> Trainees { get; init; } = new();

        public static AcbContractHistory From(Props p)
        {
            var c = new AcbContractHistory
            {
                ContractAssignment = p.U64("ContractAssignment"), ContractSucceeded = p.Bool("ContractSucceeded"),
                ContractDifficulty = p.U32("ContractDifficulty"), ContractDuration = p.U32("ContractDuration"),
                ContractXpReward = p.U32("ContractXPReward"), ContractFlorinsReward = p.U32("ContractFlorinsReward"),
                ContractLocation = p.S32("ContractLocation"), ContractCompletionOrderIndex = p.U8("ContractCompletionOrderIndex"),
            };
            foreach (var t in p.Elements("AssignedTraineesInfos"))
                c.Trainees.Add(new AcbContractTrainee
                {
                    TraineeUniqueId = t.U32("TraineeUniqueID"), ThumbnailImage = t.U64("ThumbnailImage"),
                    TraineeNameOasisId = t.U32("TraineeNameOasisID"), Level = t.U8("Level"), Dead = t.Bool("Dead"),
                });
            return c;
        }
    }

    public sealed class AcbGenericContract
    {
        public ulong ContractAssignment { get; init; }
        /// <summary>Stored as an enum-style u64 (low 32 bits = index).</summary>
        public ulong RandomLocation { get; init; }
        public uint RandomXpReward { get; init; }
        public uint RandomFlorinReward { get; init; }
        public uint RandomDifficulty { get; init; }
        public uint RandomDuration { get; init; }
    }

    public sealed class AcbTraineesGuild
    {
        public uint TraineeUniqueIdCounter { get; init; }
        public List<uint> UsedMaleTraineeNamesOasisIds { get; init; } = new();
        public List<uint> UsedFemaleTraineeNamesOasisIds { get; init; } = new();
        public List<AcbTrainee> AliveTrainees { get; init; } = new();
        public List<byte> TraineesPromotedToAssassinIndexes { get; init; } = new();
        public uint TraineesLeveledUpSinceLastVisitCount { get; init; }
        public uint TraineesRecruitedSinceLastVisitCount { get; init; }
        // Contracts
        public uint ContractAssignmentHistoryIndex { get; init; }
        public uint UsedGenericContractToAddIndex { get; init; }
        public int ContractAssignmentsInProgressCount { get; init; }
        public List<ulong> StaticContractAssignmentUnlocked { get; init; } = new();
        public List<ulong> ContractsToUnlockVerifyPrerequisites { get; init; } = new();
        public List<AcbGenericContract> AvailableGenericContracts { get; init; } = new();
        public List<ulong> UsedGenericContractAssignments { get; init; } = new();
        public List<AcbContractHistory> ContractHistory { get; init; } = new();
        public int ContractHistorySinceLastVisitCount { get; init; }
        public List<ulong> NewContractAssignmentsSinceLastVisit { get; init; } = new();
        // Tracking
        public uint TrackingTraineesRecruited { get; init; }
        public uint TrackingTraineesLost { get; init; }
        public uint TrackingContractsAttempted { get; init; }
        public uint TrackingContractsSucceeded { get; init; }
        public uint TrackingXpGained { get; init; }

        public static AcbTraineesGuild From(Props p)
        {
            var c = p.Obj("Contracts");
            var g = new AcbTraineesGuild
            {
                TraineeUniqueIdCounter = p.U32("TraineeUniqueIDCounter"),
                UsedMaleTraineeNamesOasisIds = p.PackedU32("UsedMaleTraineeNamesOasisIDs"),
                UsedFemaleTraineeNamesOasisIds = p.PackedU32("UsedFemaleTraineeNamesOasisIDs"),
                TraineesPromotedToAssassinIndexes = p.PackedU8("TraineesPromotedToAssassinIndexes"),
                TraineesLeveledUpSinceLastVisitCount = (uint)p.ElementCount("TraineesLeveledUpSinceLastVisit"),
                TraineesRecruitedSinceLastVisitCount = (uint)p.ElementCount("TraineesRecruitedSinceLastVisit"),
                ContractAssignmentHistoryIndex = c.U32("ContractAssignmentHistoryIndex"),
                UsedGenericContractToAddIndex = c.U32("UsedGenericContractToAddIndex"),
                ContractAssignmentsInProgressCount = c.ElementCount("ContractAssignmentsInProgress"),
                StaticContractAssignmentUnlocked = c.Packed("StaticContractAssignementUnlocked"),
                ContractsToUnlockVerifyPrerequisites = c.Packed("ContractsToUnlockVerifyPrerequisites"),
                UsedGenericContractAssignments = c.Packed("UsedGenericContractAssignment"),
                ContractHistorySinceLastVisitCount = c.ElementCount("ContractHistorySinceLastVisit"),
                NewContractAssignmentsSinceLastVisit = c.Packed("NewContractAssignmentsSinceLastVisit"),
                TrackingTraineesRecruited = p.U32("TrackingCummulativeTraineesRecruited"),
                TrackingTraineesLost = p.U32("TrackingCummulativeTraineesLost"),
                TrackingContractsAttempted = p.U32("TrackingCummulativeContractsAttempted"),
                TrackingContractsSucceeded = p.U32("TrackingCummulativeContractsSucceeded"),
                TrackingXpGained = p.U32("TrackingCummulativeXPGained"),
            };
            foreach (var t in p.Elements("AliveTraineeSettings")) g.AliveTrainees.Add(AcbTrainee.From(t));
            foreach (var a in c.Elements("AvailableGenericContractAssignment"))
                g.AvailableGenericContracts.Add(new AcbGenericContract
                {
                    ContractAssignment = a.U64("ContractAssignment"), RandomLocation = a.U64("RandomLocation"), RandomXpReward = a.U32("RandomXPReward"),
                    RandomFlorinReward = a.U32("RandomFlorinReward"), RandomDifficulty = a.U32("RandomDifficulty"), RandomDuration = a.U32("RandomDuration"),
                });
            foreach (var h in c.Elements("ContractAssignmentHistory")) g.ContractHistory.Add(AcbContractHistory.From(h));
            return g;
        }
    }
}
