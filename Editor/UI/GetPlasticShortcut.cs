using Codice.Utils;
using PlasticGui;

namespace Unity.PlasticSCM.Editor.UI
{
    internal static class GetPlasticShortcut
    {
        internal static string ForOpen()
        {
            return PlasticLocalization.GetString(
                PlasticLocalization.Name.UnityOpenShortcut);
        }

        internal static string ForDelete()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityDeleteShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityDeleteShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForDiff()
        {
            return PlasticLocalization.GetString(
                PlasticLocalization.Name.UnityDiffShortcut);
        }

        internal static string ForAssetDiff()
        {
            return PlasticLocalization.GetString(
                PlasticLocalization.Name.UnityAssetDiffShortcut);
        }

        internal static string ForHistory()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityHistoryShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityHistoryShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForMerge()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityMergeShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityMergeShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForHideUnhide()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityHideUnhideShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityHideUnhideShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForLabel()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityLabelShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityLabelShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForSwitch()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnitySwitchShortcutForWindows);
            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnitySwitchShortcutForMacOS);
            return string.Empty;
        }

        internal static string ForRename()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityRenameShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityRenameShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForShowInExplorer()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityShowInExplorerShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityShowInExplorerShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForEnter()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityEnterShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityEnterShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForUndo()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityUndoShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityUndoShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForRedo()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityRedoShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityRedoShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForCopy()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityCopyShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityCopyShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForCut()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityCutShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityCutShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForPaste()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityPasteShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityPasteShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForSelectAll()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnitySelectAllShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnitySelectAllShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForGoToLine()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityGoToLineShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityGoToLineShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForFind()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityFindShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnityFindShortcutForMacOS);

            return string.Empty;
        }

        internal static string ForSave()
        {
            if (PlatformIdentifier.IsWindows())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnitySaveShortcutForWindows);

            if (PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.UnitySaveShortcutForMacOS);

            return string.Empty;
        }

        internal static string DisplayString(string shortcut)
        {
            bool isMac = PlatformIdentifier.IsMac();

            string result = shortcut;

            result = result.Replace("%", isMac ? "⌘" : "Ctrl+");
            result = result.Replace("#", isMac ? "⇧" : "Shift+");
            result = result.Replace("&", isMac ? "⌥" : "Alt+");
            result = result.Replace("_", ""); // no modifier prefix

            if (result.Length >= 1)
            {
                char last = result[result.Length - 1];
                if (char.IsLetter(last))
                    result = result.Substring(0, result.Length - 1) + char.ToUpper(last);
            }

            return result;
        }
    }
}
