using PlasticGui;

using UnityEngine;

namespace Unity.PlasticSCM.Editor.UI
{
    internal class CalcTextSize : ICalcTextSize
    {
        internal static ICalcTextSize FromStyle(GUIStyle style)
        {
            return new CalcTextSize(style);
        }

        CalcTextSize(GUIStyle style)
        {
            mStyle = style;
        }

        float ICalcTextSize.GetTextWidth(string text)
        {
            return mStyle.CalcSize(new GUIContent(text)).x;
        }

        readonly GUIStyle mStyle;
    }
}
