using Codice.CM.Client.Differences;
using MergetoolGui;
using UnityEditor;
using UnityEngine;
using XDiffGui.Options;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class ComparisonMethodMenu
    {
        internal ComparisonMethodMenu(
            IComparisonMethodListener comparisonMethodListener)
        {
            mComparisonMethodListener = comparisonMethodListener;
        }

        internal void SetComparisonMethod(ComparisonMethodTypes comparisonMethod)
        {
            mCurrentComparisonMethod = comparisonMethod;
        }

        internal void UpdateIsCheckedValueForMenuItems(
            ComparisonMethodTypes comparisonMethod)
        {
            mCurrentComparisonMethod = comparisonMethod;
        }

        internal void BuildMenuItems(GenericMenu menu, string submenuPath)
        {
            menu.AddItem(
                new GUIContent(submenuPath + MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.IgnoreEol)),
                mCurrentComparisonMethod == ComparisonMethodTypes.IgnoreEol,
                () => ComparisonMethodMenuItem_Click(ComparisonMethodTypes.IgnoreEol));

            menu.AddItem(
                new GUIContent(submenuPath + MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.IgnoreWhitespace)),
                mCurrentComparisonMethod == ComparisonMethodTypes.IgnoreWhiteSpaces,
                () => ComparisonMethodMenuItem_Click(ComparisonMethodTypes.IgnoreWhiteSpaces));

            menu.AddItem(
                new GUIContent(submenuPath + MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.IgnoreEolWhitespace)),
                mCurrentComparisonMethod == ComparisonMethodTypes.IgnoreEolWhiteSpaces,
                () => ComparisonMethodMenuItem_Click(ComparisonMethodTypes.IgnoreEolWhiteSpaces));

            menu.AddSeparator(submenuPath);

            menu.AddItem(
                new GUIContent(submenuPath + MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.RecognizeAll)),
                mCurrentComparisonMethod == ComparisonMethodTypes.NotIgnore,
                () => ComparisonMethodMenuItem_Click(ComparisonMethodTypes.NotIgnore));
        }

        void ComparisonMethodMenuItem_Click(ComparisonMethodTypes comparisonMethod)
        {
            mCurrentComparisonMethod = comparisonMethod;
            mComparisonMethodListener.OnComparisonMethodChanged(comparisonMethod);
        }

        ComparisonMethodTypes mCurrentComparisonMethod;

        readonly IComparisonMethodListener mComparisonMethodListener;
    }
}
