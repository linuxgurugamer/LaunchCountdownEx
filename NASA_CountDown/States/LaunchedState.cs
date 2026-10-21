using KSP.Localization;
using System;

using System.Linq;
using System.Collections;
using NASA_CountDown.Config;
using NASA_CountDown.Helpers;
using NASA_CountDown.StateMachine;
using UnityEngine;

namespace NASA_CountDown.States
{
    public class LaunchedState : BaseGuiState
    {
        private AudioSource _audioSource;
        private GameObject _obj;
        protected DummyComponent _dummy;

        public LaunchedState(string name, KerbalFsmEx machine) : base(name, machine)
        {
            Log.Info("LaunchedState");
            OnEnter = EnterState;
            OnLeave = OnLeaveFromState;
            updateMode = KFSMUpdateMode.MANUAL_TRIGGER;
        }

        private void OnLeaveFromState(KFSMState kfsmState)
        {
            Log.Info("OnLeaveFromState: LaunchedState");
            _dummy.StopAllCoroutines();
            GameObject.Destroy(_obj);

            //_obj.DestroyGameObjectImmediate();
     
        }

        private void EnterState(KFSMState kfsmState)
        {
            Log.Info("EnterState: LaunchedState");
            if (ConfigInfo.Instance.CurrentAudio == null) return;

            _obj = new GameObject();
            _audioSource = _obj.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 0;
            _audioSource.volume = GameSettings.VOICE_VOLUME;
            
 
            //_obj.AddComponent<MonoBehaviour>().StartCoroutine(LaunchedSuccess());
            _dummy = _obj.AddComponent<DummyComponent>();
            _dummy.StartCoroutine(LaunchedSuccess());
        }

        private IEnumerator LaunchedSuccess()
        {
            var clip = ConfigInfo.Instance.CurrentAudio.LiftOff;

            if (clip != null)
            {
                _audioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(clip.length);
            }

            clip = ConfigInfo.Instance.CurrentAudio.AllEngineRunnig;

            if (clip != null)
            {
                _audioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(clip.length);
            }

            clip = ConfigInfo.Instance.CurrentAudio.TowerCleared;

            if (clip != null)
            {
                _audioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(clip.length);
            }
            
            Machine.RunEvent(Localizer.Format("Finish"));
        }

        public override void Draw()
        {
        }
    }
}
