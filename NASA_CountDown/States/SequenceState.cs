using KSP.Localization;
using System;
using System.Linq;
using KSP.UI;
using KSP.UI.Screens;
using NASA_CountDown.Config;
using NASA_CountDown.Helpers;
using NASA_CountDown.StateMachine;
using UnityEngine;
using UnityEngine.EventSystems;

using ClickThroughFix;


namespace NASA_CountDown.States
{
    public class SequenceState : BaseGuiState
    {
        public Rect _windowRect = GUIUtil.ScreenCenteredRect(270, 500);
        private bool _isEditorState;
        private int _stageIndex;


        public SequenceState(string name, KerbalFsmEx machine) : base(name, machine)
        {
            _windowRect = CountDownMain.instance.saveLoadWinPos.sequenceWindow;
            _windowRect.height = 500;

            OnEnter = state =>
            {
                if (!ConfigInfo.Instance.Sequences.ContainsKey(ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)))
                {
                    ConfigInfo.Instance.Sequences.Add(ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel), Enumerable.Repeat(-1, 10).ToArray());
                    ConfigInfo.Instance.VesselOptions.Add(ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel), new PerVesselOptions());
                }

                StageManager.Instance.Stages.ForEach(@group => group.Icons.ForEach(icon => icon.radioButton.onClick.AddListener(OnClickButton)));
            };

            OnLeave = state =>
            {
                StageManager.Instance.Stages.ForEach(@group => group.Icons.ForEach(icon => icon.radioButton.onClick.RemoveListener(OnClickButton)));
            };

            updateMode = KFSMUpdateMode.MANUAL_TRIGGER;
        }

        private void OnClickButton(PointerEventData arg0, UIRadioButton.State arg1, UIRadioButton.CallType arg2)
        {
            if (!_isEditorState) return;

            var stage = arg0.pointerPress.GetComponentInParent<StageGroup>();

            if (stage != null)
            {
                ConfigInfo.Instance.Sequences[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)][_stageIndex] = stage.inverseStageIndex;
                _isEditorState = false;
            }
        }

        public override void Draw()
        {
            _windowRect = KSPUtil.ClampRectToScreen(ClickThruBlocker.GUIWindow(InitialState.baseWindowID + 1, _windowRect, DrawSequenceWindow, "", StyleFactory.LaunchSequenceStyle));
            GUI.BringWindowToFront(InitialState.baseWindowID + 1);
        }

        private void DrawSequenceWindow(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.FlexibleSpace();

            ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].LaunchSequenceControl = GUILayout.Toggle(ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].LaunchSequenceControl, Localizer.Format("#LOC_NASA_34"), StyleFactory.ToggleStyle);
            ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].enableSAS = GUILayout.Toggle(ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].enableSAS, Localizer.Format("#LOC_NASA_35"), StyleFactory.ToggleStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(Localizer.Format("#LOC_NASA_36") + (ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].defaultInitialThrottle*100).ToString("F1") + "%):");
            GUILayout.FlexibleSpace();
            ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].defaultInitialThrottle = GUILayout.HorizontalSlider(ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].defaultInitialThrottle, 0.01f, 0.99f, GUILayout.Width(100));
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(Localizer.Format("#LOC_NASA_37") + (ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].defaultThrottle * 100).ToString("F1") + "%):");
            GUILayout.FlexibleSpace();
            ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].defaultThrottle = GUILayout.HorizontalSlider(ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].defaultThrottle, 0.01f, 1.0f, GUILayout.Width(100));
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            if (!ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].LaunchSequenceControl)
                GUI.enabled = false;
            if (GravityTurnAPI.GTAvailable)
            {
                ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].useGravityTurn = GUILayout.Toggle(ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].useGravityTurn, Localizer.Format("#LOC_NASA_38"), StyleFactory.ToggleStyle);
            }
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(Localizer.Format("#LOC_NASA_39"), StyleFactory.LabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(Localizer.Format("#LOC_NASA_40"), StyleFactory.LabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            for (int i = 0; i < 10; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.Label($"{i + 1} second", StyleFactory.LabelStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label(ConfigInfo.Instance.Sequences[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)][i] < 0 ? Localizer.Format("#LOC_NASA_41") : ConfigInfo.Instance.Sequences[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)][i].ToString(), StyleFactory.LabelStyle, GUILayout.MinWidth(40));

                bool flag = _isEditorState && i != _stageIndex;
                string label = _isEditorState && i == _stageIndex ? Localizer.Format("#LOC_NASA_28") : Localizer.Format("#LOC_NASA_42");

                GUI.enabled = !flag;

                if (GUILayout.Button(label))
                {
                    _isEditorState = !_isEditorState;
                    _stageIndex = i;
                }
                GUI.enabled = true;

                if (GUILayout.Button(Localizer.Format("#LOC_NASA_43")))
                {
                    _isEditorState = false;
                    _stageIndex = -1;
                    ConfigInfo.Instance.Sequences[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)][i] = -1;
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent(), StyleFactory.ButtonBackStyle))
            {
                CountDownMain.instance.saveLoadWinPos.SaveSettings();
                Machine.RunEvent(Localizer.Format("Init"));
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUI.DragWindow();
        }
    }
}
