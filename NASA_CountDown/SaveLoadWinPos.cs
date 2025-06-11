using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using NASA_CountDown.Config;

namespace NASA_CountDown
{
    public class SaveLoadWinPos
    {
        public static SaveLoadWinPos Instance;

        public SaveLoadWinPos()
        {
            Instance = this;
            LoadWindowPositions();
        }

        Rect sequencewindow;
        public Rect sequenceWindow
        {
            get { return sequencewindow; }
            set
            {
                if (value.width == 0)
                    return;
                this.sequencewindow = value;
            }
        }
        Rect initialwindow;
        public Rect initialWindow
        {
            get { return this.initialwindow; }
            set
            {
                if (value.width == 0)
                    return;
                this.initialwindow = value;
            }
        }
        Rect settingswindow;
        public Rect settingsWindow
        {
            get { return settingswindow; }
            set
            {
                if (value.width == 0)
                    return;
                this.settingswindow = value;
            }
        }

        string PLUGINDATA = KSPUtil.ApplicationRootPath + "GameData/NASA_CountDown/PluginData/NASACountdown.cfg";

        internal void SaveWinPos(ConfigNode settings, string winName, Rect win)
        {
            settings.SetValue(winName + "X", (win.x + 1).ToString(), true);
            settings.SetValue(winName + "Y", (win.y + 1).ToString(), true);
        }

        public void SaveSettings()
        {
            ConfigInfo.Instance.Save();
        }

        static public string SafeLoad(string value, double oldvalue)
        {
            if (value == null)
                return oldvalue.ToString();
            return value;
        }
        private Rect ScaleRect(Rect r)
        {
            var factor = ConfigInfo.Instance.Scale;
            return new Rect(
                r.xMin * factor,
                r.yMin * factor,
                r.width * factor,
                r.height * factor
               );
        }
        Rect GetWinPos(ConfigNode settings, string winName, float width, float height, bool initialScaled = true)
        {
            double x = (Screen.width - width) / 2;
            double y = (Screen.height - height) / 2;
            Rect r;
            if (settings != null && settings.HasValue(winName + "X"))
            {
                var x1 = Double.Parse(SafeLoad(settings.GetValue(winName + "X"), x));
                var y1 = Double.Parse(SafeLoad(settings.GetValue(winName + "Y"), y));
                if (x1 > 0) x = x1 - 1;
                if (y1 > 0) y = y1 - 1;

                // var w = Double.Parse(SafeLoad(settings.GetValue(winName + "Y"), width));
                // var h = Double.Parse(SafeLoad(settings.GetValue(winName + "H"), height));
                if (initialScaled)
                    r = ScaleRect(new Rect((float)x, (float)y, (float)width, (float)height));
                else
                    r = new Rect((float)x, (float)y, (float)width, (float)height);
            }
            else
            {
                r = ScaleRect(GUIUtil.ScreenCenteredRect(width, height));
            }

            Log.Info("GetWinPos, winName: " + winName + ",  r.x: " + r.x + ", r.y: " + r.y);
            return r;
        }

        const float CNT_WIDTH = 459;
        const float CNT_HEIGHT = 120;

        const float CFG_WIDTH = 200;
        const float CFG_HEIGHT = 290;

        const float SEQ_WIDTH = 270;
        const float SEQ_HEIGHT = 400;

        bool loaded = false;
        public void LoadWindowPositions()
        {
            if (loaded)
            {
                return;
            }
            Log.Info("LoadWindowPositions");
            loaded = true;
            //ConfigNode settingsFile;
            ConfigNode settings = ConfigInfo.Instance.Load();

            if (settings != null)
            { 
                
                sequenceWindow = GetWinPos(settings, Localizer.Format("#LOC_NASA_11"), SEQ_WIDTH, SEQ_HEIGHT, false);
                initialWindow = GetWinPos(settings, Localizer.Format("#LOC_NASA_12"), CNT_WIDTH, CNT_HEIGHT);
                settingsWindow = GetWinPos(settings, Localizer.Format("#LOC_NASA_13"), CFG_WIDTH, CFG_HEIGHT, false);
            }
            else
            {
                sequenceWindow = GetWinPos(settings, Localizer.Format("#LOC_NASA_11"), SEQ_WIDTH, SEQ_HEIGHT, false);
                initialWindow = GetWinPos(settings, Localizer.Format("#LOC_NASA_12"), CNT_WIDTH, CNT_HEIGHT);
                settingsWindow = GetWinPos(settings, Localizer.Format("#LOC_NASA_13"), CFG_WIDTH, CFG_HEIGHT, false);
            }
        }
    }
}
