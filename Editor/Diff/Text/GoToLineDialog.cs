using System;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using PlasticGui;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class GoToLineDialog : EditorWindow
    {
        internal static void Show(
            int currentLine,
            int totalLines,
            Action<int> onGoToLine)
        {
            GoToLineDialog dialog = CreateInstance<GoToLineDialog>();
            dialog.mCurrentLine = currentLine;
            dialog.mTotalLines = totalLines;
            dialog.mOnGoToLine = onGoToLine;
            dialog.titleContent = new GUIContent(
                PlasticLocalization.GetString(PlasticLocalization.Name.GoToLine));

            Vector2 size = new Vector2(DIALOG_WIDTH, DIALOG_HEIGHT);
            dialog.minSize = size;
            dialog.maxSize = size;

            dialog.ShowUtility();
        }

        void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingTop = MARGIN;
            root.style.paddingBottom = MARGIN;
            root.style.paddingLeft = MARGIN;
            root.style.paddingRight = MARGIN;

            GoToLineDialogBody body = new GoToLineDialogBody(mCurrentLine, mTotalLines);
            body.Confirmed += OnConfirmed;
            body.Cancelled += Close;

            root.Add(body);
        }

        void OnConfirmed(int line)
        {
            mOnGoToLine(line);
            Close();
        }

        Action<int> mOnGoToLine;
        int mCurrentLine;
        int mTotalLines;

        const float DIALOG_WIDTH = 300f;
        const float DIALOG_HEIGHT = 105f;
        const float MARGIN = 12f;
    }
}
