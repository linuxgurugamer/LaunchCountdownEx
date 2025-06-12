using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using System.Diagnostics;
namespace NASA_CountDown
{
    public class ModuleNASACountdown : PartModule
    {
        [KSPField(guiActive = false, guiName = "craftName", isPersistant = true)]
        public string craftName;

        public static string CraftName(Vessel v)
        {
            Log.Info("CraftName");
#if false
            var stackTrace = new StackTrace(true);
            foreach (var r in stackTrace.GetFrames())
            {
                Console.WriteLine(Localizer.Format("Filename: {0} Method: {1} Line: {2} Column: {3}  "),
                    r.GetFileName(), r.GetMethod(), r.GetFileLineNumber(),
                    r.GetFileColumnNumber());
            }
#endif
            for (int i = 0; i < v.Parts.Count; i++)
            {
                var p = v.Parts[i];

                if (p != null && p.Modules.Contains<ModuleCommand>())
                {
                    ModuleNASACountdown m = p.Modules.GetModule<ModuleNASACountdown>();
                    return m.craftName;
                }
            }

            return v.vesselName;
        }

        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH && craftName == "")
            {
                craftName = FlightGlobals.ActiveVessel.vesselName;
                Log.Info("ModuleNASACountdown.OnAwake, part: " + this.part.partInfo.title + ", craftName: " + craftName);
            }
            else
            {
                if (HighLogic.LoadedSceneIsFlight)
                    Log.Info("ModuleNASACountdown.OnAwake, LoadSceneIsFlight, but no ActiveVessel");
            }
        }
    }    
}
