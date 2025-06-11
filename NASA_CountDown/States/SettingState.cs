using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using NASA_CountDown.Config;
using NASA_CountDown.Helpers;
using NASA_CountDown.StateMachine;
using UnityEngine;

using ClickThroughFix;


namespace NASA_CountDown.States
{
    public sealed class SettingState : BaseGuiState
    {
        public Rect _windowRect;
        internal List<string> _soundsList;
        private int _audioSet;

       
        void InitSettingState()
        {
            _soundsList = ConfigInfo.Instance.AudioSets.Keys.ToList();
        }
        public SettingState(string name, KerbalFsmEx machine) : base(name, machine)
        {
            // _windowRect = GUIUtil.ScreenCenteredRect(200, 290);
            _windowRect = CountDownMain.instance.saveLoadWinPos.settingsWindow;
            OnEnter = OnEnterToState;
        }

        private void OnEnterToState(KFSMState kfsmState)
        {
            Log.Info("OnEnterToState: SettingState");
            InitSettingState();
        }

        private void DrawSettingsWindow(int id)
        {
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            ConfigInfo.Instance.AbortExecuted = GUILayout.Toggle(ConfigInfo.Instance.AbortExecuted, Localizer.Format("#LOC_NASA_44"), StyleFactory.ToggleStyle);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Scale window {ConfigInfo.Instance.Scale:0.0}", StyleFactory.LabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            ConfigInfo.Instance.Scale = GUILayout.HorizontalSlider(ConfigInfo.Instance.Scale, .2f, 1f, GUILayout.MaxWidth(160f), GUILayout.MinWidth(140f));

            GUI.enabled = _soundsList.Any();

            ConfigInfo.Instance.IsSoundEnabled = _soundsList.Any() && GUILayout.Toggle(ConfigInfo.Instance.IsSoundEnabled, Localizer.Format("#LOC_NASA_45"), StyleFactory.ToggleStyle);

            if (_soundsList.Any() && ConfigInfo.Instance.IsSoundEnabled)
            {
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.Label(Localizer.Format("#LOC_NASA_46"), StyleFactory.LabelStyle);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("", StyleFactory.ButtonSoundBackStyle))
                {
                    _audioSet = _audioSet <= 0 ? _soundsList.Count - 1 : _audioSet - 1;
                    ConfigInfo.Instance.SoundSet = _soundsList[_audioSet];
                }

                GUILayout.FlexibleSpace();

                GUILayout.Label(ConfigInfo.Instance.SoundSet, StyleFactory.LabelStyle, GUILayout.Width(100));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("", StyleFactory.ButtonSoundNextStyle))
                {
                    _audioSet = _audioSet >= _soundsList.Count - 1 ? 0 : _audioSet + 1;
                    ConfigInfo.Instance.SoundSet = _soundsList[_audioSet];
                }

                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            GUI.enabled = true;

            if (
                GUI.Button(
                    new Rect((StyleFactory.SettingsStyle.fixedWidth - StyleFactory.ButtonBackStyle.fixedWidth) / 2,
                        StyleFactory.SettingsStyle.fixedHeight - StyleFactory.ButtonBackStyle.fixedHeight - 10,
                        StyleFactory.ButtonBackStyle.fixedWidth, StyleFactory.ButtonBackStyle.fixedHeight), string.Empty,
                    StyleFactory.ButtonBackStyle))
            {
                CountDownMain.instance.saveLoadWinPos.SaveSettings();
                Machine.RunEvent(Localizer.Format("#LOC_NASA_2"));
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        public override void Draw()
        {
            _windowRect = KSPUtil.ClampRectToScreen(ClickThruBlocker.GUIWindow(InitialState.baseWindowID + 2, _windowRect, DrawSettingsWindow, "", StyleFactory.SettingsStyle));
            GUI.BringWindowToFront(InitialState.baseWindowID + 2);
        }
    }
}
