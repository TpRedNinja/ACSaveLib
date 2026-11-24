using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acs
{
    /// <summary>Contents of a slot 3 (options) save: PlayerOptionsSaveData and OnlineProfileSaveData (profile container) and
    /// SinglePlayerOptionsSaveData (last container). Unnamed keys stay in the node tree.</summary>
    public sealed class AcsOptionsData
    {
        public bool DisplaySubtitles { get; private set; }
        public ulong SoundLanguage { get; private set; }
        public ulong SubtitlesLanguage { get; private set; }
        public ulong TextLanguage { get; private set; }
        public float MusicVolume { get; private set; }
        public float VoiceVolume { get; private set; }
        public float SoundFxVolume { get; private set; }
        public bool Vibration { get; private set; }
        public float CameraXAxisSensitivity { get; private set; }
        public float CameraYAxisSensitivity { get; private set; }
        public bool InvertCameraHorizontalAxis { get; private set; }
        public bool InvertCameraVerticalAxis { get; private set; }
        public bool InvertAimingCameraHorizontalAxis { get; private set; }
        public bool InvertAimingCameraVerticalAxis { get; private set; }
        public uint CameraInserts { get; private set; }
        public uint Brightness { get; private set; }
        public bool Blood { get; private set; }
        public uint TimeOfDay { get; private set; }
        public ulong GpsVisibility { get; private set; }
        public ulong HudAllHidden { get; private set; }
        public Dictionary<string, bool> HudFlags { get; private set; } = new();
        public bool HudMissionTrackerHidden { get; private set; }
        public uint UlcUnlocksShown { get; private set; }
        public float TimeSaverXpBoostElapsedTime { get; private set; }
        public float TimeSaverXpBoostDuration { get; private set; }
        public uint TimeSaverXpBoostIncreasePercent { get; private set; }
        public float UlcXpBoostElapsedTime { get; private set; }
        public float UlcXpBoostDuration { get; private set; }
        public uint UlcXpBoostIncreasePercent { get; private set; }
        public uint LastHelixBalance { get; private set; }
        public ulong LastExpiredTransactionTimestamp { get; private set; }
        public bool VirtualCurrencyProfileUpdated { get; private set; }
        public uint UplayRewards { get; private set; }
        public ulong UplayActions { get; private set; }
        public bool UplayWelcomeBackDone { get; private set; }
        public int EventSessionCount { get; private set; }
        public bool FlyingMachineControls { get; private set; }
        public bool InvertCannonHorizontalOnlyCameraControls { get; private set; }
        public bool InvertCannonVerticalOnlyCameraControls { get; private set; }

        private static readonly string[] HudNames =
        {
            "HUDConflictHidden", "HUDPlayerWidgetHidden", "HUDWeaponIndicatorHidden", "HUDHighGroundHidden",
            "HUDTutorialsDisabled", "HUDUpdatesDisabled", "HUDPuppeteerHidden", "HighlightEffectsHidden", "HUDAdditionnalHidden",
        };

        /// <param name="profileRoot">Items of the middle container.</param>
        /// <param name="lastRoot">Items of the last container.</param>
        public static AcsOptionsData From(List<Node> profileRoot, List<Node> lastRoot)
        {
            var r = new AcsOptionsData();
            foreach (var n in profileRoot)
            {
                var p = new Props(n.Children);
                switch (n.Name)
                {
                    case "PlayerOptionsSaveData":
                        r.DisplaySubtitles = p.Bool("DisplaySubtitles"); r.SoundLanguage = p.U64("SoundLanguage"); r.SubtitlesLanguage = p.U64("SubtitlesLanguage"); r.TextLanguage = p.U64("TextLanguage");
                        r.MusicVolume = p.Float("MusicVolume"); r.VoiceVolume = p.Float("VoiceVolume"); r.SoundFxVolume = p.Float("SoundFXVolume");
                        r.Vibration = p.Bool("Vibration"); r.CameraXAxisSensitivity = p.Float("CameraXAxisSensitivity"); r.CameraYAxisSensitivity = p.Float("CameraYAxisSensitivity");
                        r.InvertCameraHorizontalAxis = p.Bool("InvertCameraHorizontalAxis"); r.InvertCameraVerticalAxis = p.Bool("InvertCameraVerticalAxis");
                        r.InvertAimingCameraHorizontalAxis = p.Bool("InvertAimingCameraHorizontalAxis"); r.InvertAimingCameraVerticalAxis = p.Bool("InvertAimingCameraVerticalAxis");
                        r.CameraInserts = p.U32("CameraInserts"); r.Brightness = p.U32("Brightness"); r.Blood = p.Bool("Blood"); r.TimeOfDay = p.U32("TimeOfDay");
                        r.GpsVisibility = p.U64("GPSVisibility"); r.HudAllHidden = p.U64("HUDAllHidden"); r.HudMissionTrackerHidden = p.Bool("HUDMissionTrackerHidden");
                        foreach (var name in HudNames) r.HudFlags[name] = p.Bool(name);
                        break;
                    case "OnlineProfileSaveData":
                        r.UplayRewards = p.U32("UplayRewards"); r.UplayActions = p.U64("UplayActions"); r.UplayWelcomeBackDone = p.Bool("UplayWelcomeBackDone");
                        r.UlcUnlocksShown = p.U32("ULCUnlocksShown");
                        r.TimeSaverXpBoostElapsedTime = p.Float("TimeSaverXPBoostElapsedTime"); r.TimeSaverXpBoostDuration = p.Float("TimeSaverXPBoostDuration"); r.TimeSaverXpBoostIncreasePercent = p.U32("TimeSaverXPBoostIncreasePercent");
                        r.UlcXpBoostElapsedTime = p.Float("ULCXPBoostElapsedTime"); r.UlcXpBoostDuration = p.Float("ULCXPBoostDuration"); r.UlcXpBoostIncreasePercent = p.U32("ULCXPBoostIncreasePercent");
                        r.LastHelixBalance = p.Obj("SecondaryStoreProfileSaveData").U32("LastHelixBalance"); r.LastExpiredTransactionTimestamp = p.Obj("SecondaryStoreProfileSaveData").U64("LastExpiredTransactionTimestamp");
                        r.VirtualCurrencyProfileUpdated = p.Obj("OnlineCoreProfileSaveData").Bool("VirtualCurrencyProfileUpdated");
                        r.EventSessionCount = p.Obj("OnlineCoreProfileSaveData").Obj("EventSaveData").Packed("EventSessions").Count;
                        break;
                }
            }
            foreach (var n in lastRoot)
            {
                if (n.Name != "SinglePlayerOptionsSaveData") continue;
                var p = new Props(n.Children);
                r.FlyingMachineControls = p.Bool("FlyingMachineControls");
                r.InvertCannonHorizontalOnlyCameraControls = p.Bool("InvertCannonHorizontalOnlyCameraControls");
                r.InvertCannonVerticalOnlyCameraControls = p.Bool("InvertCannonVerticalOnlyCameraControls");
            }
            return r;
        }
    }
}
