using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NASA_CountDown.Helpers
{
    public class AudioSet
    {
        public AudioSet(string name)
        {
            Name = name;

            var sounds = GameDatabase.Instance.databaseAudio.Where(x => x.name.StartsWith(Localizer.Format("#LOC_NASA_19"), StringComparison.OrdinalIgnoreCase) && x.name.Contains(name)).ToList();

            TimerSounds = sounds.Where(x => x.name.Contains(Localizer.Format("#LOC_NASA_20"))).ToList();
            Abort = sounds.FirstOrDefault(x => x.name.EndsWith(Localizer.Format("#LOC_NASA_21"), StringComparison.OrdinalIgnoreCase));
            Hold = sounds.FirstOrDefault(x => x.name.EndsWith(Localizer.Format("#LOC_NASA_22"), StringComparison.OrdinalIgnoreCase));
            LiftOff = sounds.FirstOrDefault(x => x.name.EndsWith(Localizer.Format("#LOC_NASA_23"), StringComparison.OrdinalIgnoreCase));
            AllEngineRunnig = sounds.FirstOrDefault(x => x.name.EndsWith(Localizer.Format("#LOC_NASA_24"), StringComparison.OrdinalIgnoreCase));
            TowerCleared = sounds.FirstOrDefault(x => x.name.EndsWith(Localizer.Format("#LOC_NASA_25"), StringComparison.OrdinalIgnoreCase));
        }

        public List<AudioClip> TimerSounds { get; private set; }

        public string Name { get; private set; }

        public AudioClip Abort { get; private set; }

        public AudioClip Hold { get; private set; }

        public AudioClip LiftOff { get; private set; }

        public AudioClip TowerCleared { get; private set; }

        public AudioClip AllEngineRunnig { get; private set; }
    }
}
