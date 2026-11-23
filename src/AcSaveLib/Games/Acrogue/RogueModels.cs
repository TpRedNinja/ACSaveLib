using System;
using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acrogue
{
    /// <summary>Header container of a Rogue save. Names come from ACViewer; values are the raw numbers stored in the save.</summary>
    public sealed class RogueHeader
    {
        public uint Version { get; init; }
        public uint MagicNumber { get; init; }
        public uint DlcPackagesBits { get; init; }
        public uint CodeChangelist { get; init; }
        public uint SaveGameId { get; init; }
        public bool IsOptions { get; init; }
        public uint TimePlayed { get; init; }
        public uint PlayerGlobalSynch { get; init; }
        /// <summary>Unix time in seconds as stored.</summary>
        public uint SaveGameDate { get; init; }

        public static RogueHeader From(List<Node> nodes)
        {
            var p = new Props(nodes);
            return new RogueHeader
            {
                Version = p.U32("Version"), MagicNumber = p.U32("MagicNumber"), DlcPackagesBits = p.U32("DLCPackagesBits"),
                CodeChangelist = p.U32("CodeChangelist"), SaveGameId = p.U32("SaveGameID"), IsOptions = p.Bool("IsOptions"),
                TimePlayed = p.U32("TimePlayed"), PlayerGlobalSynch = p.U32("PlayerGlobalSynch"),
                SaveGameDate = p.U32("SaveGameDate"),
            };
        }
    }

    /// <summary>The main container of a Rogue game save: typed models of every named top-level field. Unnamed keys stay in the node tree.</summary>
    public sealed class RogueGameData
    {
        /// <summary>True when a NarrativeMissionHistory item is present (game saves), false for the profile save.</summary>
        public bool HasGameData { get; private set; }
        public MissionHistory Narrative { get; private set; } = new();
        public MissionHistory Side { get; private set; } = new();
        public ulong CurrentMission { get; private set; }
        public ulong World { get; private set; }
        public ulong PlayerPortal { get; private set; }
        public uint ContextId { get; private set; }
        public AnimusDatabase AnimusDatabase { get; private set; } = new();
        public MapIconData MapIcons { get; private set; } = new();
        public MapFogData MapFog { get; private set; } = new();
        public MessagingDatabase Messaging { get; private set; } = new();
        public UITutorialData UITutorial { get; private set; } = new();
        public PresentEmails PresentEmails { get; private set; } = new();
        public PresentRewards PresentRewards { get; private set; } = new();
        public AccomplishmentManager Accomplishments { get; private set; } = new();
        public GameStats GameStats { get; private set; } = new();
        public CommunityChallenge CommunityChallenge { get; private set; } = new();
        public PlayerProgression PlayerProgression { get; private set; } = new();
        public PlayerProgression SnapshotPlayerProgression { get; private set; } = new();
        public SnapshotDelta SnapshotDelta { get; private set; } = new();
        public FleetData Fleet { get; private set; } = new();
        public BusinessManager Business { get; private set; } = new();

        public static RogueGameData From(List<Node> root)
        {
            var r = new RogueGameData();
            foreach (var n in root)
            {
                switch (n.Name)
                {
                    case "NarrativeMissionHistory": r.HasGameData = true; r.Narrative = MissionHistory.From(n); break;
                    case "SideMissionHistory": r.Side = MissionHistory.From(n); break;
                    case "CurrentMission": r.CurrentMission = n.Value; break;
                    case "World": r.World = n.Value; break;
                    case "PlayerPortal": r.PlayerPortal = n.Value; break;
                    case "ContextID": r.ContextId = (uint)n.Value; break;
                    case "AnimusDatabaseData": r.AnimusDatabase = AnimusDatabase.From(new Props(n.Children)); break;
                    case "MapIconData": r.MapIcons = MapIconData.From(new Props(n.Children)); break;
                    case "MapFogData": r.MapFog = MapFogData.From(new Props(n.Children)); break;
                    case "MessagingDatabaseData": r.Messaging = MessagingDatabase.From(new Props(n.Children)); break;
                    case "UITutorialData": r.UITutorial = UITutorialData.From(new Props(n.Children)); break;
                    case "PresentEmailsData": r.PresentEmails = PresentEmails.From(new Props(n.Children)); break;
                    case "PresentRewardSaveData": r.PresentRewards = PresentRewards.From(new Props(n.Children)); break;
                    case "AccomplishmentManagerData": r.Accomplishments = AccomplishmentManager.From(new Props(n.Children)); break;
                    case "GameStatsSaveData": r.GameStats = GameStats.From(new Props(n.Children)); break;
                    case "CommunityChallengeSaveData": r.CommunityChallenge = CommunityChallenge.From(new Props(n.Children)); break;
                    case "PlayerProgSaveData": r.PlayerProgression = PlayerProgression.From(new Props(n.Children)); break;
                    case "SnapshotPlayerProgSaveData": r.SnapshotPlayerProgression = PlayerProgression.From(new Props(n.Children)); break;
                    case "SnapshotPlayerProgressionDelta": r.SnapshotDelta = SnapshotDelta.From(new Props(n.Children)); break;
                    case "FleetSaveGameData": r.Fleet = FleetData.From(new Props(n.Children)); break;
                    case "BusinessManagerData": r.Business = BusinessManager.From(new Props(n.Children)); break;
                }
            }
            return r;
        }
    }
}
