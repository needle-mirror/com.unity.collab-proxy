using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class ContributorsHeaderPanel : VisualElement
    {
        internal ContributorsHeaderPanel()
        {
            BuildComponents();
        }

        internal void Dispose()
        {
        }

        internal void SetNames(string leftSpec, string rightSpec)
        {
            mLeftFileName.text = leftSpec;
            mLeftFileName.tooltip = leftSpec;

            mRightFileName.text = rightSpec;
            mRightFileName.tooltip = rightSpec;

            OnRightTextBoxClean();
        }

        internal void OnLeftTextBoxDirty()
        {
            mLeftFileName.style.color = UnityStyles.Colors.RedText;
            EditedTextBlockMarker.AddMark(mLeftFileName);
        }

        internal void OnLeftTextBoxClean()
        {
            mLeftFileName.style.color = StyleKeyword.Null;
            EditedTextBlockMarker.RemoveMark(mLeftFileName);
        }

        internal void OnRightTextBoxDirty()
        {
            mRightFileName.style.color = UnityStyles.Colors.RedText;
            EditedTextBlockMarker.AddMark(mRightFileName);
        }

        internal void OnRightTextBoxClean()
        {
            mRightFileName.style.color = StyleKeyword.Null;
            EditedTextBlockMarker.RemoveMark(mRightFileName);
        }

        internal void SetSeparatorsVisible(bool visible)
        {
            float width = visible ? 1 : 0;
            mLeftToolbar.style.borderBottomWidth = width;
            mRightToolbar.style.borderBottomWidth = width;
        }

        void BuildComponents()
        {
            style.flexDirection = FlexDirection.Row;
            style.flexShrink = 0;

            mLeftToolbar = BuildLeftHeaderPanel();
            mLeftToolbar.style.flexGrow = 1;
            mLeftToolbar.style.flexBasis = 0;

            mRightToolbar = BuildRightHeaderPanel();
            mRightToolbar.style.flexGrow = 1;
            mRightToolbar.style.flexBasis = 0;

            Add(mLeftToolbar);
            Add(mRightToolbar);
        }

        UnityEditor.UIElements.Toolbar BuildLeftHeaderPanel()
        {
            UnityEditor.UIElements.Toolbar toolbar = ControlBuilder.Toolbar.Create();

            mLeftFileName = ControlBuilder.Label.CreateSelectableLabel();
            mLeftFileName.style.unityTextAlign = TextAnchor.MiddleCenter;
            mLeftFileName.style.overflow = Overflow.Hidden;
            mLeftFileName.style.textOverflow = TextOverflow.Ellipsis;
            mLeftFileName.style.flexGrow = 1;
            mLeftFileName.style.flexShrink = 1;
            mLeftFileName.text = "Left file";

            toolbar.Add(mLeftFileName);

            return toolbar;
        }

        UnityEditor.UIElements.Toolbar BuildRightHeaderPanel()
        {
            UnityEditor.UIElements.Toolbar toolbar = ControlBuilder.Toolbar.Create();

            mRightFileName = ControlBuilder.Label.CreateSelectableLabel();
            mRightFileName.style.unityTextAlign = TextAnchor.MiddleCenter;
            mRightFileName.style.overflow = Overflow.Hidden;
            mRightFileName.style.textOverflow = TextOverflow.Ellipsis;
            mRightFileName.style.flexGrow = 1;
            mRightFileName.style.flexShrink = 1;
            mRightFileName.text = "Right file";

            toolbar.Add(mRightFileName);

            return toolbar;
        }

        UnityEditor.UIElements.Toolbar mLeftToolbar;
        UnityEditor.UIElements.Toolbar mRightToolbar;
        Label mLeftFileName;
        Label mRightFileName;
    }
}
