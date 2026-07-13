using System.Diagnostics.CodeAnalysis;
using Codice.CM.Client.Differences.Graphic;
using UnityEditor;

namespace Unity.PlasticSCM.Editor.Diff
{
    [InitializeOnLoad]
    [ExcludeFromCodeCoverage]
    internal static class DiffMergeColorThemeSetter
    {
        static DiffMergeColorThemeSetter()
        {
            bool isDarkTheme = EditorGUIUtility.isProSkin;

            SetDiffMergeColors(ColorConfiguration.Value, isDarkTheme);
        }

        static void SetDiffMergeColors(ColorConfiguration configuration, bool isDarkTheme)
        {
            configuration.DefaultBaseColor = configuration.BaseColor = (isDarkTheme) ?
                 DiffMergeDefaultColors.DEFAULT_BASE_COLOR_DARK :
                 DiffMergeDefaultColors.DEFAULT_BASE_COLOR;

            configuration.DefaultSourceColor = configuration.SourceColor = (isDarkTheme) ?
                 DiffMergeDefaultColors.DEFAULT_SOURCE_COLOR_DARK :
                 DiffMergeDefaultColors.DEFAULT_SOURCE_COLOR;

            configuration.DefatultDestinationColor = configuration.DestinationColor = (isDarkTheme) ?
                 DiffMergeDefaultColors.DEFAULT_DESTINATION_COLOR_DARK :
                 DiffMergeDefaultColors.DEFAULT_DESTINATION_COLOR;

            configuration.CurrentConflictBorderColor = (isDarkTheme) ?
                 DiffMergeDefaultColors.CURRENT_CONFLICT_BORDER_COLOR_DARK :
                 DiffMergeDefaultColors.CURRENT_CONFLICT_BORDER_COLOR;

            configuration.DiffLinesColor = (isDarkTheme) ?
                DiffMergeDefaultColors.DIFFLINES_COLOR_DARK :
                DiffMergeDefaultColors.DIFFLINES_COLOR;

            configuration.CollapsedDiffColor = (isDarkTheme) ?
                DiffMergeDefaultColors.COLLAPSED_DIFF_COLOR_DARK :
                DiffMergeDefaultColors.COLLAPSED_DIFF_COLOR;

            configuration.MergeManualSolvedConflictColor = (isDarkTheme) ?
                DiffMergeDefaultColors.MERGE_MANUAL_SOLVED_CONFLICT_COLOR_DARK :
                DiffMergeDefaultColors.MERGE_MANUAL_SOLVED_CONFLICT_COLOR;

            configuration.CombinedDiffAlpha = (isDarkTheme) ?
                DiffMergeDefaultColors.COMBINED_DIFF_ALPHA_DARK :
                DiffMergeDefaultColors.COMBINED_DIFF_ALPHA;

            configuration.DiffAlpha = (isDarkTheme) ?
                DiffMergeDefaultColors.DIFF_ALPHA_DARK :
                DiffMergeDefaultColors.DIFF_ALPHA;

            configuration.InsideDiffAlpha = (isDarkTheme) ?
                DiffMergeDefaultColors.INSIDE_DIFF_ALPHA_DARK :
                DiffMergeDefaultColors.INSIDE_DIFF_ALPHA;

            configuration.DeletedDiffAlpha = (isDarkTheme) ?
                DiffMergeDefaultColors.DELETED_DIFF_ALPHA_DARK :
                DiffMergeDefaultColors.DELETED_DIFF_ALPHA;

            configuration.MovedDiffAlpha = (isDarkTheme) ?
                DiffMergeDefaultColors.MOVED_DIFF_ALPHA_DARK :
                DiffMergeDefaultColors.MOVED_DIFF_ALPHA;

            configuration.UnselectedDiffAlpha = (isDarkTheme) ?
                DiffMergeDefaultColors.UNSELECTED_DIFF_ALPHA_DARK :
                DiffMergeDefaultColors.UNSELECTED_DIFF_ALPHA;
        }
    }
}
