using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Codice.Utils;
using PlasticGui;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class DiffTextViewContextMenu
    {
        internal DiffTextViewContextMenu(Unity.CodeEditor.TextEditor textEditor)
        {
            mTextEditor = textEditor;

            CreateGUI();

            mTextEditor.TextArea.RegisterCallback<PointerUpEvent>(OnPointerUp);
            mTextEditor.TextArea.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        internal void Dispose()
        {
            mTextEditor.TextArea.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            mTextEditor.TextArea.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        void OnKeyDown(KeyDownEvent e)
        {
            if (KeyboardEvents.MatchesShortcut(e, GetPlasticShortcut.ForGoToLine()))
            {
                e.StopPropagation();
                OnGoToLine();
            }
        }

        void OnPointerUp(PointerUpEvent e)
        {
            if (e.button != 1)
                return;

            GenericMenu menu = new GenericMenu();
            UpdateMenuItems(menu);
            menu.ShowAsContext();
        }

        void UpdateMenuItems(GenericMenu menu)
        {
            if (mTextEditor.CanUndo)
                menu.AddItem(mUndoContent, false, OnUndo);
            else
                menu.AddDisabledItem(mUndoContent, false);

            if (mTextEditor.CanRedo)
                menu.AddItem(mRedoContent, false, OnRedo);
            else
                menu.AddDisabledItem(mRedoContent, false);

            menu.AddSeparator(string.Empty);

            if (mTextEditor.CanCopy)
                menu.AddItem(mCopyContent, false, OnCopy);
            else
                menu.AddDisabledItem(mCopyContent, false);

            if (mTextEditor.CanCut)
                menu.AddItem(mCutContent, false, OnCut);
            else
                menu.AddDisabledItem(mCutContent, false);

            if (mTextEditor.CanPaste)
                menu.AddItem(mPasteContent, false, OnPaste);
            else
                menu.AddDisabledItem(mPasteContent, false);

            if (mTextEditor.CanDelete)
                menu.AddItem(mDeleteContent, false, OnDelete);
            else
                menu.AddDisabledItem(mDeleteContent, false);

            menu.AddSeparator(string.Empty);

            menu.AddItem(mSelectAllContent, false, OnSelectAll);

            menu.AddSeparator(string.Empty);

            menu.AddItem(mGoToLineContent, false, OnGoToLine);

            menu.AddSeparator(string.Empty);

            menu.AddItem(mFindContent, false, OnFind);
        }

        void OnUndo()
        {
            mTextEditor.Undo();
        }

        void OnRedo()
        {
            mTextEditor.Redo();
        }

        void OnCopy()
        {
            mTextEditor.Copy();
        }

        void OnCut()
        {
            mTextEditor.Cut();
        }

        void OnPaste()
        {
            mTextEditor.Paste();
        }

        void OnDelete()
        {
            mTextEditor.Delete();
        }

        void OnSelectAll()
        {
            mTextEditor.SelectAll();
        }

        void OnGoToLine()
        {
            GoToLineDialog.Show(
                mTextEditor.TextArea.Caret.Line,
                mTextEditor.LineCount,
                GoToLine);
        }

        void OnFind()
        {
            mTextEditor.SearchPanel.IsReplaceMode = false;
            mTextEditor.SearchPanel.Open();
        }

        void GoToLine(int targetLine)
        {
            mTextEditor.CaretOffset =
                mTextEditor.Document.GetLineByNumber(targetLine).Offset;
            mTextEditor.ScrollToLine(targetLine);
        }

        void CreateGUI()
        {
            mUndoContent = new GUIContent(string.Format("{0} {1}",
                PlasticLocalization.GetString(PlasticLocalization.Name.EditMenuUndo),
                GetPlasticShortcut.ForUndo()));
            mRedoContent = new GUIContent(string.Format("{0} {1}",
                PlasticLocalization.GetString(PlasticLocalization.Name.EditMenuRedo),
                GetPlasticShortcut.ForRedo()));
            mCopyContent = new GUIContent(string.Format("{0} {1}",
                PlasticLocalization.GetString(PlasticLocalization.Name.EditMenuCopy),
                GetPlasticShortcut.ForCopy()));
            mCutContent = new GUIContent(string.Format("{0} {1}",
                PlasticLocalization.GetString(PlasticLocalization.Name.EditMenuCut),
                GetPlasticShortcut.ForCut()));
            mPasteContent = new GUIContent(string.Format("{0} {1}",
                PlasticLocalization.GetString(PlasticLocalization.Name.EditMenuPaste),
                GetPlasticShortcut.ForPaste()));
            mDeleteContent = new GUIContent(
                PlasticLocalization.GetString(PlasticLocalization.Name.EditMenuDelete));
            mSelectAllContent = new GUIContent(string.Format("{0} {1}",
                PlasticLocalization.GetString(PlasticLocalization.Name.EditMenuSelectAll),
                GetPlasticShortcut.ForSelectAll()));
            mGoToLineContent = new GUIContent(string.Format("{0} {1}",
                PlasticLocalization.GetString(PlasticLocalization.Name.EditMenuGoToLine),
                GetPlasticShortcut.ForGoToLine()));
            mFindContent = new GUIContent(string.Format("{0} {1}",
                PlasticLocalization.GetString(PlasticLocalization.Name.EditMenuFind),
                GetPlasticShortcut.ForFind()));
        }

        GUIContent mUndoContent;
        GUIContent mRedoContent;
        GUIContent mCopyContent;
        GUIContent mCutContent;
        GUIContent mPasteContent;
        GUIContent mDeleteContent;
        GUIContent mSelectAllContent;
        GUIContent mGoToLineContent;
        GUIContent mFindContent;

        readonly Unity.CodeEditor.TextEditor mTextEditor;
    }
}
