using KSP.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KSP.UI.Screens;
using NASA_CountDown.Config;
using NASA_CountDown.Helpers;
using NASA_CountDown.StateMachine;
using UnityEngine;

namespace NASA_CountDown.States
{
    public class LaunchState : InitialState
    {
        private AudioSource _audioSource;
        private List<Action> _stages;

        public LaunchState(string name, KerbalFsmEx machine) : base(name, machine) { }

        protected override void OnEnterToState(KFSMState kfsmState)
        {
            Log.Info("OnEnterToState: LaunchState");
            base.OnEnterToState(kfsmState);

            _audioSource = _obj.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 0;
            _audioSource.volume = GameSettings.VOICE_VOLUME;
            
            if (ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].LaunchSequenceControl)
                FlightGlobals.ActiveVessel.OnFlyByWire = (FlightInputCallback)Delegate.Combine(FlightGlobals.ActiveVessel.OnFlyByWire, (FlightInputCallback)OnFlyByWire);

            GameEvents.onVesselSituationChange.Add(SituationChanged);

            if ( !ConfigInfo.Instance.Sequences.ContainsKey(ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)))
            {
                ConfigInfo.Instance.Sequences.Add(ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel), Enumerable.Repeat(-1, 10).ToArray());
                ConfigInfo.Instance.VesselOptions.Add(ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel), new PerVesselOptions());                
            }
            _stages = ConfigInfo.Instance.Sequences[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].Select(x => x < 0 ? new Action(() => { }) : new Action(() => { if (ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].LaunchSequenceControl) StageManager.ActivateStage(x); } )).ToList();
      

            _dummy.StartCoroutine(this.TickLaunch());
        }

        protected override void OnLeaveFromState(KFSMState kfsmState)
        {
            Log.Info("OnLeaveFromState: LaunchState");
            base.OnLeaveFromState(kfsmState);
            if (ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].LaunchSequenceControl)
            {
                Log.Info("Removing OnFlyByWire");
                FlightGlobals.ActiveVessel.OnFlyByWire = (FlightInputCallback)Delegate.Remove(FlightGlobals.ActiveVessel.OnFlyByWire, (FlightInputCallback)OnFlyByWire);
            }
            GameEvents.onVesselSituationChange.Remove(SituationChanged);
        }

        //bool hold = false;
        protected override void DrawButtons()
        {
            Log.Info("LaunchState.DrawButtons");

            var buttonWidth = StyleFactory.ButtonLaunchStyle.fixedWidth;
            var buttonHeight = StyleFactory.ButtonLaunchStyle.fixedHeight;

            if (GUI.Button(new Rect(_windowRect.xMin, _windowRect.yMax - _delta, buttonWidth, buttonHeight), string.Empty, StyleFactory.ButtonAbortStyle))
            {
                _dummy.StopAllCoroutines();
                _dummy.StartCoroutine(Abort());
            }
            if (!GravityTurnAPI.GravityTurnActive)
            {
                var b = paused;
                paused = GUI.Toggle(new Rect(_windowRect.xMin + buttonWidth, _windowRect.yMax - _delta, buttonWidth, buttonHeight),
                    paused, "", StyleFactory.ButtonHoldStyle);
                if (paused != b)
                {
                    //paused = !paused;
                    if (paused && !holdPlayed)
                    {
                        _dummy.StartCoroutine(Hold());
                    }
                    else
                        if (!paused)
                    {
                        holdPlayed = false;
                    }
                }
            }
        }

        private IEnumerator Abort()
        {
            TimeWarp.SetRate(0, false);
            var clip = ConfigInfo.Instance.CurrentAudio.Abort;
            if (GravityTurnAPI.GravityTurnActive)
                GravityTurnAPI.Kill();
            holdPlayed = true;


            if (clip != null)
            {
                CountDownMain.instance.PlaySound(clip);
            }

            if (ConfigInfo.Instance.AbortExecuted)
            {
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Abort, true);
                // FireAction added a new parameter, overridegroup with no idea what it's for
                //BaseAction.FireAction(FlightGlobals.ActiveVessel.Parts, KSPActionGroup.Abort,0, KSPActionType.Activate);
            }

            Machine.RunEvent(Localizer.Format("#LOC_NASA_2"));
            yield return null;
        }

        private IEnumerator Hold()
        {
            TimeWarp.SetRate(0, false);
            while (_audioSource != null && _audioSource.isPlaying)
                yield return new WaitForSeconds(0.1f);

            var clip = ConfigInfo.Instance.CurrentAudio.Hold;

            if (clip != null)
            {
                _audioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(clip.length);
            }
        }

        double countdownStartTime;
        static public bool paused = false;
        static bool holdPlayed = false;

        private IEnumerator TickLaunch()
        {
            Log.Info("TickLaunch");
            countdownStartTime = Planetarium.GetUniversalTime();
            paused = false;
            holdPlayed = false;
            int count = 10;
            if (ConfigInfo.Instance != null)
            {
                if (ConfigInfo.Instance.CurrentAudio != null)
                    count = ConfigInfo.Instance.IsSoundEnabled && ConfigInfo.Instance.CurrentAudio.TimerSounds.Any() ? ConfigInfo.Instance.CurrentAudio.TimerSounds.Count - 1 : 15;
            }


            for (var i = count; i >= 0; i--)
            {
                while (paused)
                    yield return new WaitForSeconds(1f);
//                while (paused && (_audioSource == null || (_audioSource != null && !_audioSource.isPlaying)))
//                        yield return new WaitForSeconds(1f);
                _tick = i;
                var oneShotStartTime = Planetarium.GetUniversalTime();

                if (_audioSource != null && ConfigInfo.Instance != null && ConfigInfo.Instance.CurrentAudio != null)
                    _audioSource.PlayOneShot(ConfigInfo.Instance.CurrentAudio.TimerSounds.FirstOrDefault(x => x.name.EndsWith($"/{i}")));

                if (_stages != null && _stages.Count > i)
                    _stages[i]();
                var oneShotEndTime = Planetarium.GetUniversalTime();
                Log.Info("tick: " + _tick.ToString() + ",  starttime/endtime: " + oneShotStartTime.ToString("n4") + "/" + oneShotEndTime.ToString("n4"));
                var oneShotElapsedTime = oneShotEndTime - oneShotStartTime;
                if (oneShotElapsedTime < 1.0f && _tick > 0)
                    yield return new WaitForSeconds(1.0f - (float)oneShotElapsedTime);
   
            }
            Log.Info("final _tic: " + _tick);
            GravityTurnAPI.Launch();
        }

        private void OnFlyByWire(FlightCtrlState st)
        {
            Log.Info("OnFlyByWire, tic: " + _tick);
            switch (_tick)
            {
                case 7:
                case 6:
                case 5:
                case 4:
                case 3:
                case 2:
                case 1:
                    st.mainThrottle = ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].defaultInitialThrottle;
                    break;
                case 0:
                    //st.mainThrottle = 1f;
                    Log.Info("OnFlyByWire, before GravityTurnAPI.Launch");
                    if (GravityTurnAPI.Launch())  // If GravityTurn is available
                        break;

                    st.mainThrottle = ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].defaultThrottle;
                    if (ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].enableSAS)
                        FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);

                    break;
                default:
                    if (ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].enableSAS)
                        FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
                    //st.mainThrottle = 0f;
                    st.mainThrottle = ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].defaultThrottle;
                    break;
            }
        }

        private void SituationChanged(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> data)
        {
            if (data.host != FlightGlobals.ActiveVessel)
                return;
            Log.Info("situationChanged, to.vessel: " + data.host.ToString() + ",  situation: " + data.to.ToString());
                
            switch (data.to)
            {
                case Vessel.Situations.FLYING:
                    Machine.RunEvent(Localizer.Format("#LOC_NASA_6"));
                    break;
                case Vessel.Situations.LANDED:
                    break;
                case Vessel.Situations.SPLASHED:
                case Vessel.Situations.SUB_ORBITAL:
                case Vessel.Situations.ORBITING:
                case Vessel.Situations.ESCAPING:
                case Vessel.Situations.DOCKED:
                    Machine.RunEvent(Localizer.Format("#LOC_NASA_1"));
                    break;
                case Vessel.Situations.PRELAUNCH:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
