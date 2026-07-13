using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff
{
    internal static class EditedTextBlockMarker
    {
        internal static void AddMark(Label title)
        {
            if (title.text.EndsWith(EDITED_MARK))
                return;

            title.text += EDITED_MARK;
        }

        internal static void RemoveMark(Label title)
        {
            if (!title.text.EndsWith(EDITED_MARK))
                return;

            title.text = title.text.Substring(
                0, title.text.Length - EDITED_MARK.Length);
        }

        const string EDITED_MARK = " *";
    }
}
