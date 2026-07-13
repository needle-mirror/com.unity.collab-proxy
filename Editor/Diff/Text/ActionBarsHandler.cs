using Unity.CodeEditor;

using Codice.CM.Client.Differences.Graphic;
using UnityEngine.UIElements;
using XDiffGui;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class ActionBarsHandler
    {
        internal LeftActionBar LeftActionBar { get { return mLeftActionBar; } }
        internal RightActionBar RightActionBar { get { return mRightActionBar; } }

        internal ActionBarsHandler(IActionBarClickListener actionBarClickListener)
        {
            mActionBarClickListener = actionBarClickListener;
        }

        internal ActionBar BuildLeftActionBar(TextEditor textEditor)
        {
            mLeftActionBar = new LeftActionBar(textEditor, mActionBarClickListener);
            return mLeftActionBar;
        }

        internal ActionBar BuildRightActionBar(TextEditor textEditor)
        {
            mRightActionBar = new RightActionBar(textEditor, mActionBarClickListener);
            return mRightActionBar;
        }

        internal void SetDrawingInfo(DiffRegions diffRegions)
        {
            DiffActions actions = DiffActionCalculator.CalculateDiffActions(diffRegions);
            mLeftActionBar.DiffActions = actions.LeftDiffs;
            mRightActionBar.DiffActions = actions.RightDiffs;
        }

        internal void SetActionBarsVisibility(bool visible)
        {
            if (visible)
            {
                ShowActionBars();
                return;
            }

            HideActionBars();
        }

        internal void ShowActionBars()
        {
            mLeftActionBar.style.display = DisplayStyle.Flex;
            mRightActionBar.style.display = DisplayStyle.Flex;
        }

        internal void HideActionBars()
        {
            mLeftActionBar.style.display = DisplayStyle.None;
            mRightActionBar.style.display = DisplayStyle.None;
        }

        internal void Dispose()
        {
            if (mLeftActionBar != null)
                mLeftActionBar.Dispose();

            if (mRightActionBar != null)
                mRightActionBar.Dispose();
        }

        LeftActionBar mLeftActionBar;
        RightActionBar mRightActionBar;
        IActionBarClickListener mActionBarClickListener;
    }
}
