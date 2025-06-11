using KSP.Localization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NASA_CountDown.Helpers
{
    public static class Utils
    {
        private static Dictionary<int, bool> _elements;

        public static T EditableField<T>(string caption, T value, Func<T, string> toStringMethod, Func<string, T> parseMethod, Action<T> onSubmit)
        {
            bool flag = false;

            if (_elements == null)
            {
                _elements = new Dictionary<int, bool>();
            }

            if (_elements.ContainsKey(caption.GetHashCode()))
            {
                flag = _elements[caption.GetHashCode()];
            }
            else
            {
                _elements.Add(caption.GetHashCode(), false);
            }

            if (flag)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(caption + ": ");

                value = parseMethod.Invoke(GUILayout.TextField(toStringMethod.Invoke(value), GUILayout.MinWidth(40), GUILayout.ExpandWidth(true)));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(Localizer.Format("#LOC_NASA_28")))
                {
                    _elements[caption.GetHashCode()] = false;
                    onSubmit(value);
                }

                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(caption + ": " + toStringMethod.Invoke(value), GUILayout.ExpandWidth(true));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(Localizer.Format("#LOC_NASA_29")))
                {
                    _elements[caption.GetHashCode()] = true;
                }
                GUILayout.EndHorizontal();
            }
            return value;
        }

        public static int EditableIntField(string caption, int value, Action<int> onSubmit)
        {
            return EditableField<int>(caption, value, i => i.ToString(), int.Parse, onSubmit);
        }
    }
}
