using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEngine.UIElements;
using XDiffGui;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class TextEditorSaveKeyBinding
    {
        internal static TextEditorSaveKeyBinding InitForEditor(
            Unity.CodeEditor.TextEditor textEditor,
            ISaveChangesListener listener)
        {
            return new TextEditorSaveKeyBinding(textEditor, listener);
        }

        internal void Dispose()
        {
            mTextEditor.TextArea.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        TextEditorSaveKeyBinding(
            Unity.CodeEditor.TextEditor textEditor,
            ISaveChangesListener listener)
        {
            mTextEditor = textEditor;
            mListener = listener;
            mSaveShortcut = GetPlasticShortcut.ForSave();

            mTextEditor.TextArea.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        void OnKeyDown(KeyDownEvent e)
        {
            if (!KeyboardEvents.MatchesShortcut(e, mSaveShortcut))
                return;

            if (mTextEditor.IsReadOnly)
                return;

            e.StopPropagation();

            mListener.OnSaveChanges();
        }

        readonly Unity.CodeEditor.TextEditor mTextEditor;
        readonly ISaveChangesListener mListener;
        readonly string mSaveShortcut;
    }
}
