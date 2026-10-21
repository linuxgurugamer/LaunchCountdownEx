using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using NASA_CountDown.Helpers;
using UnityEngine;

namespace NASA_CountDown.Config
{
    public class PerVesselOptions
    {
        internal bool LaunchSequenceControl { get; set; } = HighLogic.CurrentGame.Parameters.CustomParams<NC>().LaunchSequenceControl;
        internal bool enableSAS { get; set; } = HighLogic.CurrentGame.Parameters.CustomParams<NC>().enableSAS;
        internal float defaultInitialThrottle { get; set; } = HighLogic.CurrentGame.Parameters.CustomParams<NC>().defaultInitialThrottle;
        internal float defaultThrottle { get; set; } = HighLogic.CurrentGame.Parameters.CustomParams<NC>().defaultThrottle;
        internal bool useGravityTurn { get; set; } = false;

        public PerVesselOptions()
        {
            Log.Info("Initting new PerVesselOptions");
        }
    }

    class ConfigInfo //: IConfigNode
    {
        private readonly RectWrapper _wrapper = new RectWrapper();

        private static ConfigInfo _config;

        private ConfigInfo() { }

        internal bool IsSoundEnabled { get; set; }

        internal string SoundSet { get; set; }

        internal Dictionary<string, int[]> Sequences { get; set; } = new Dictionary<string, int[]>();

        internal Rect WindowPosition { get; set; } = GUIUtil.ScreenCenteredRect(459, 120);

        // Add the following to the Load/Save

        internal Dictionary<string, PerVesselOptions> VesselOptions { get; set; } = new Dictionary<string, PerVesselOptions>();
        //

        internal bool AbortExecuted { get; set; } = true;

        internal float Scale { get; set; } = 1;

        internal bool IsLoaded { get; private set; } = false;



        internal static ConfigInfo Instance => _config ?? (_config = new ConfigInfo());

        string SETTINGSNAME = Localizer.Format("#LOC_NASA_17");
        string PLUGINDATA = KSPUtil.ApplicationRootPath + "GameData/NASA_CountDown/PluginData/NASACountdown.cfg";

        public void Save()
        {
            Log.Info("ConfigInfo.Save");
            ConfigNode settingsFile = new ConfigNode();
            ConfigNode settings = new ConfigNode();

            settingsFile.SetNode(SETTINGSNAME, settings, true);
            if (SaveLoadWinPos.Instance.sequenceWindow.width > 0)
                SaveLoadWinPos.Instance.SaveWinPos(settings, Localizer.Format("#LOC_NASA_11"), SaveLoadWinPos.Instance.sequenceWindow);
            if (SaveLoadWinPos.Instance.initialWindow.width > 0)
                SaveLoadWinPos.Instance.SaveWinPos(settings, Localizer.Format("#LOC_NASA_12"), SaveLoadWinPos.Instance.initialWindow);
            if (SaveLoadWinPos.Instance.settingsWindow.width > 0)
                SaveLoadWinPos.Instance.SaveWinPos(settings, Localizer.Format("#LOC_NASA_13"), SaveLoadWinPos.Instance.settingsWindow);

            settings.AddValue("soundEnabled", IsSoundEnabled);
            settings.AddValue("scale", Scale);
            if (SoundSet != null)
                settings.AddValue("soundSet", SoundSet);



            foreach (var sequence in Sequences)
            {
                var seqNode = new ConfigNode(Localizer.Format("#LOC_NASA_18"));
                seqNode.AddValue("id", sequence.Key);

                seqNode.AddValue("useGravityTurn", VesselOptions[sequence.Key].useGravityTurn);
                seqNode.AddValue("LaunchSequenceControl", VesselOptions[sequence.Key].LaunchSequenceControl);
                seqNode.AddValue("enableSAS", VesselOptions[sequence.Key].enableSAS);
                seqNode.AddValue("defaultInitialThrottle", VesselOptions[sequence.Key].defaultInitialThrottle);
                seqNode.AddValue("defaultThrottle", VesselOptions[sequence.Key].defaultThrottle);


                string stages = "";
                for (int i = 0; i < sequence.Value.Length; i++)
                {
                    if (stages != "")
                        stages += ",";
                    stages += sequence.Value[i].ToString();
                }

                if (stages != "")
                {
                    seqNode.AddValue("stages", stages);

                    settings.AddNode(seqNode);
                }
            }

            settingsFile.Save(PLUGINDATA);
        }

        public ConfigNode Load()
        {
            LoadSounds();
            Log.Info("ConfigInfo.Load");
            ConfigNode settingsFile = ConfigNode.Load(PLUGINDATA);
            ConfigNode node = null;
            if (settingsFile != null)
            {
                 node = settingsFile.GetNode(SETTINGSNAME);
                if (node != null)
                {
                    if (node.HasValue("soundEnabled"))
                    {
                        IsSoundEnabled = bool.Parse(node.GetValue("soundEnabled"));
                    }

                    if (node.HasValue("abort"))
                    {
                        AbortExecuted = bool.Parse(node.GetValue("abort"));
                    }

                    if (node.HasValue("scale"))
                    {
                        Scale = float.Parse(node.GetValue("scale"));
                    }

                    if (node.HasValue("soundSet"))
                    {
                        SoundSet = node.GetValue("soundSet");
                    }

                    if (node.HasValue("position"))
                    {
                        WindowPosition = _wrapper.ToRect(node.GetValue("position"));
                        Debug.LogWarning("Position is" + WindowPosition);
                    }

                    if (node.HasNode("sequence"))
                    {
                        var sequences = node.GetNodes("sequence");
                        Sequences.Clear();
                        VesselOptions.Clear();

                        foreach (var sequence in sequences)
                        {
                            PerVesselOptions pvo = new PerVesselOptions();
                            if (sequence.HasValue("LaunchSequenceControl"))
                            {
                                pvo.LaunchSequenceControl = bool.Parse(sequence.GetValue("LaunchSequenceControl"));
                            }

                            if (sequence.HasValue("enableSAS"))
                            {
                                pvo.enableSAS = bool.Parse(sequence.GetValue("enableSAS"));
                            }

                            if (sequence.HasValue("defaultInitialThrottle"))
                            {
                                pvo.defaultInitialThrottle = float.Parse(sequence.GetValue("defaultInitialThrottle"));
                            }
                            if (sequence.HasValue("defaultThrottle"))
                            {
                                pvo.defaultThrottle = float.Parse(sequence.GetValue("defaultThrottle"));
                            }

                            if (sequence.HasValue("useGravityTurn"))
                            {
                                pvo.useGravityTurn = bool.Parse(sequence.GetValue("useGravityTurn"));
                            }
                            VesselOptions.Add(sequence.GetValue("id"), pvo);

                            int[] ar = Enumerable.Repeat(-1, 10).ToArray();
                            var d = sequence.GetValue("stages").Split(',').Select(int.Parse).ToArray();
                            for (int i = 0; i < d.Length; i++)
                            {
                                //Log.Info("Load, i: " + i + ", " + d[i]);
                                ar[i] = d[i];
                            }
                            Sequences.Add(sequence.GetValue("id"), ar);
                        }
                    }
                    else
                    {
                        Sequences.Clear();
                        VesselOptions.Clear();
                        if (FlightGlobals.ActiveVessel != null && !ConfigInfo.Instance.Sequences.ContainsKey(ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)))
                        {
                            Sequences.Add(ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel), Enumerable.Repeat(-1, 10).ToArray());
                            VesselOptions.Add(ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel), new PerVesselOptions());
                        }
                    }

                    IsLoaded = true;
                }
            }
            else
            {
                Sequences.Clear();
                VesselOptions.Clear();
            }
            return node;
        }
        public void InitNewConfig()
        {
            if (FlightGlobals.ActiveVessel != null && !ConfigInfo.Instance.Sequences.ContainsKey(ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)))
            {
                Log.Info("InitNewConfig for " + ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel));
                Sequences.Add(ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel), Enumerable.Repeat(-1, 10).ToArray());
                VesselOptions.Add(ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel), new PerVesselOptions());
            }
        }

        private void LoadSounds()
        {
            var soundsList =
                GameDatabase.Instance.databaseAudio.Where(x => x.name.StartsWith(Localizer.Format("#LOC_NASA_19"), StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.name)
                    .ToList();

            foreach (var name in soundsList.Select(x => x.Split('/')).Select(m => m[2]).Distinct())
            {
                if (!AudioSets.ContainsKey(name))
                    AudioSets.Add(name, new AudioSet(name));
            }
        }

        public Dictionary<string, AudioSet> AudioSets { get; } = new Dictionary<string, AudioSet>();

        public AudioSet CurrentAudio
        {
            get
            {
                if (!ConfigInfo.Instance.IsSoundEnabled || SoundSet == null)
                    return null;
                return AudioSets.ContainsKey(SoundSet) ? AudioSets[SoundSet] : null;
            }
        }
    }
}
