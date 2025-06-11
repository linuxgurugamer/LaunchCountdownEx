using KSP.Localization;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;



namespace NASA_CountDown
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class NC : GameParameters.CustomParameterNode
    {
        public override string Title { get { return Localizer.Format("#LOC_NASA_14"); } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return Localizer.Format("#LOC_NASA_15"); } }
        public override string DisplaySection { get { return Localizer.Format("#LOC_NASA_15"); } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return true; } }

        [GameParameters.CustomParameterUI("Mod Enabled")]
        public bool EnabledForSave = true;      // is enabled for this save file

        [GameParameters.CustomParameterUI("Keep buttons visible")]
        public bool keepButtonsVisible = true;

        [GameParameters.CustomParameterUI("Default Launch Sequence Control")]
        public bool LaunchSequenceControl = true;


        public float defaultInitialThrottle = 0.01f;
        [GameParameters.CustomFloatParameterUI("Default Initial Throttle (%)", displayFormat = "N0", minValue = 0, maxValue = 100, stepCount = 1, asPercentage = false)]
        public float defaultInitialThrottlePercent
        {
            get { return defaultInitialThrottle * 100; }
            set { defaultInitialThrottle = value / 100.0f; }
        }

        public float defaultThrottle = 1.0f;
        [GameParameters.CustomFloatParameterUI("Default Final Throttle (%)", displayFormat = "N0", minValue = 0, maxValue = 100, stepCount = 1, asPercentage = false)]
        public float defaultThrottlePercent
        {
            get { return defaultThrottle * 100; }
            set { defaultThrottle = value / 100.0f; }
        }

        [GameParameters.CustomParameterUI("Automatically enable SAS at launch")]
        public bool enableSAS = true;      


        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            EnabledForSave = true;      // is enabled for this save file
            keepButtonsVisible = true;
            defaultThrottle = 1.0f;
        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == Localizer.Format("#LOC_NASA_16")) //This Field must always be enabled.
                return true;

            return EnabledForSave; //otherwise return true
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }

}
