using System;
using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Ac3
{
    /// <summary>A game data container of an AC3 save: typed models of the named top-level fields. Other keys stay in the node tree.</summary>
    public sealed class Ac3GameData
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

        public static Ac3GameData From(List<Node> root)
        {
            var r = new Ac3GameData();
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
                }
            }
            return r;
        }
    }
}
