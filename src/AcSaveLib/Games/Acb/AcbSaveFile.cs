using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AcSaveLib.Container;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acb
{
    /// <summary>
    /// Assassin's Creed Brotherhood (.SAV, PC). Layout is documented in docs/FORMAT_ACB.md.
    /// No encryption. The file holds LZO2A chunked containers: container 0 is the header (SaveGame root), container 1 the
    /// main property stream (or PlayerOptionsSaveData in an options file). Read-only.
    /// </summary>
    public sealed class AcbSaveFile
    {
        public const uint FileMagic = 0x00FEDBAC;

        public byte[] Data { get; private set; } = Array.Empty<byte>();
        /// <summary>Decompressed containers in file order (2 in a game save, 3 in an options file).</summary>
        public List<byte[]> Containers { get; private set; } = new();
        /// <summary>Items of container 0 (header properties).</summary>
        public List<Node> HeaderProperties { get; private set; } = new();
        /// <summary>Top-level items of container 1.</summary>
        public List<Node> Root { get; private set; } = new();
        /// <summary>Top-level items of container 2 (options file only; six Bool/U64 values whose names are not known).</summary>
        public List<Node> ExtraProperties { get; private set; } = new();

        public AcbHeader Header { get; private set; } = new();
        public bool IsOptions => Header.IsOptions;
        /// <summary>True when container 1 holds game data (World, missions, progression) rather than options.</summary>
        public bool HasGameData { get; private set; }

        public AcbOptions? Options { get; private set; }

        public AcbMissionHistory Narrative { get; private set; } = new();
        public AcbMissionHistory Side { get; private set; } = new();
        /// <summary>World handle of the saved position (top-level World item).</summary>
        public ulong World { get; private set; }
        public ulong PlayerPortal { get; private set; }
        public AcbPspState Psp { get; private set; } = new();
        public AcbPspState PspSnapshot { get; private set; } = new();
        public List<AcbAnimusEntry> AnimusEntries { get; private set; } = new();
        public AcbUiTutorial UiTutorial { get; private set; } = new();
        public AcbEmails Emails { get; private set; } = new();
        public List<AcbAccomplishment> Accomplishments { get; private set; } = new();
        public AcbGameStats GameStats { get; private set; } = new();
        public AcbTraineesGuild TraineesGuild { get; private set; } = new();
        public AcbFacebookData Facebook { get; private set; } = new();
        public AcbPlayerProgression Progression { get; private set; } = new();
        public AcbPlayerProgression ProgressionSnapshot { get; private set; } = new();
        public AcbProgressionDelta ProgressionDelta { get; private set; } = new();
        public AcbVilla Villa { get; private set; } = new();
        public AcbVilla VillaSnapshot { get; private set; } = new();
        public AcbWorldUpgrade WorldUpgrade { get; private set; } = new();
        public AcbWorldUpgrade WorldUpgradeSnapshot { get; private set; } = new();

        public static AcbSaveFile Load(string path) => Parse(File.ReadAllBytes(path));

        public static AcbSaveFile Parse(byte[] file)
        {
            if (file.Length < 64 || BitConverter.ToUInt32(file, 4) != FileMagic)
                throw new InvalidDataException("Not a Brotherhood save: magic 0x00FEDBAC not found at offset 4.");
            var r = new AcbSaveFile { Data = file };
            r.Containers = ChunkContainer.ReadAll(file, 0);
            if (r.Containers.Count < 2) throw new InvalidDataException("Expected at least two containers.");
            r.HeaderProperties = ParseContainer(r.Containers[0]);
            r.Header = AcbHeader.From(r.HeaderProperties);
            r.Root = ParseContainer(r.Containers[1]);
            if (r.Containers.Count > 2) r.ExtraProperties = ParseContainer(r.Containers[2]);
            r.Fill();
            return r;
        }

        /// <summary>A decoded container starts with 10 zero bytes, the root key, two u32 sizes, then the items at offset 22.</summary>
        private static List<Node> ParseContainer(byte[] c)
        {
            if (c.Length < 22) throw new InvalidDataException("Container too short.");
            return PropertyReader.Parse(c, 22, c.Length, true);
        }

        private void Fill()
        {
            var g = new Props(Root);
            var opt = g.Get("PlayerOptionsSaveData");
            if (opt != null) Options = AcbOptions.From(opt);
            HasGameData = g.Get("NarrativeMissionHistory") != null;
            if (!HasGameData) return;
            Narrative = AcbMissionHistory.From(g.Obj("NarrativeMissionHistory"));
            Side = AcbMissionHistory.From(g.Obj("SideMissionHistory"));
            World = g.U64("World");
            PlayerPortal = g.U64("PlayerPortal");
            Psp = AcbPspState.From(g.Obj("PSPState"));
            PspSnapshot = AcbPspState.From(g.Obj("SnapshotPSPState"));
            foreach (var e in g.Obj("AnimusDatabaseData").Elements("ADBEntries"))
                AnimusEntries.Add(new AcbAnimusEntry
                {
                    EntryFoundOrderNumber = e.U32("EntryFoundOrderNumber"), GlyphFoundOrderNumber = e.U32("GlyphFoundOrderNumber"),
                    AfsDisplayedCount = e.U32("AFSDisplayedCount"), IsLgsSolved = e.Bool("IsLGSSolved"),
                });
            UiTutorial = AcbUiTutorial.From(g.Obj("UITutorialData"));
            Emails = AcbEmails.From(g.Obj("PresentEmailsData"));
            foreach (var e in g.Obj("AccomplishmentManagerData").Elements("AccomplishmentEntries"))
                Accomplishments.Add(new AcbAccomplishment { ObjectId = e.U32("ObjectID"), Inactive = e.Bool("Inactive"), Completed = e.Bool("Completed"), CurrentCount = e.U32("CurrentCount") });
            GameStats = AcbGameStats.From(g.Obj("GameStatsSaveData"));
            TraineesGuild = AcbTraineesGuild.From(g.Obj("TraineesGuildManagerSaveData"));
            Facebook = AcbFacebookData.From(g.Obj("FacebookSaveGameData"));
            Progression = AcbPlayerProgression.From(g.Obj("PlayerProgSaveData"));
            ProgressionSnapshot = AcbPlayerProgression.From(g.Obj("SnapshotPlayerProgSaveData"));
            ProgressionDelta = AcbProgressionDelta.From(g.Obj("SnapshotPlayerProgressionDelta"));
            Villa = AcbVilla.From(g.Obj("VillaSaveData"));
            VillaSnapshot = AcbVilla.From(g.Obj("SnapshotVillaSaveData"));
            WorldUpgrade = AcbWorldUpgrade.From(g.Obj("WorldUpgradeSaveData"));
            WorldUpgradeSnapshot = AcbWorldUpgrade.From(g.Obj("SnapshotWorldUpgradeSaveData"));
        }
    }
}
