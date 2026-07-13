using PlasticGui;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff
{
    // The diff was produced from an object whose serialized data was
    // dropped by Unity's loader (LoadSerializedFileAndForget). The diff is
    // incomplete — fields the loader couldn't materialize are missing from
    // it — and the user should be steered to text diff.
    //
    // Each kind corresponds to a specific failure mode in Unity's loader
    // that we have explicitly identified. We do not surface a generic
    // "something might be wrong" signal — every entry here names a concrete
    // problem so the UI can give the user an accurate explanation. Higher
    // values are more specific — see DataLossDetection.PickMoreSpecific for
    // the precedence used when multiple signals fire on the same object pair.
    internal enum DataLossKind
    {
        None = 0,

        // AnimatorOverrideController whose runtimeAnimatorController is
        // null. AwakeFromLoad → BuildAsset wipes m_Clips whenever the
        // controller is null (see Modules/Animation/AnimatorOverrideController.cpp
        // in the Unity source), so any clip overrides serialized in the
        // YAML are silently dropped and never reach the diff.
        UnresolvedAnimatorController = 1,

        // MonoBehaviour with no resolvable MonoScript reference (missing
        // script). Serialized fields beyond the base MonoBehaviour are lost.
        UnresolvedMonoScript = 2,

        // The loader replaced the object with UnityEditor.FallbackEditorWindow
        // because its script could not be loaded into the editor AppDomain
        // (typically: package not installed or AppDomain reload timing). All
        // fields except the base EditorWindow members are stripped.
        FallbackEditorWindow = 3
    }

    // Localized copy for every data-loss surface — per-row tooltip,
    // file-level banner, and the "switch to text diff" button. Kept
    // alongside DataLossKind so adding a new kind forces the maintainer
    // to also choose the user-facing message.
    internal static class DataLossDescriptions
    {
        internal static string GetRowTooltip(DataLossKind kind)
        {
            switch (kind)
            {
                case DataLossKind.UnresolvedAnimatorController:
                    return PlasticLocalization.Name
                        .DiffDataLossUnresolvedAnimatorController.GetString();
                case DataLossKind.UnresolvedMonoScript:
                    return PlasticLocalization.Name
                        .DiffDataLossUnresolvedMonoScript.GetString();
                case DataLossKind.FallbackEditorWindow:
                    return PlasticLocalization.Name
                        .DiffDataLossFallbackEditorWindow.GetString();
                default:
                    return string.Empty;
            }
        }

        internal static string GetBannerMessage(int dataLossCount)
        {
            if (dataLossCount <= 0)
                return string.Empty;

            if (dataLossCount == 1)
            {
                return PlasticLocalization.Name
                    .DiffDataLossBannerSingular.GetString();
            }

            return PlasticLocalization.Name
                .DiffDataLossBannerPlural.GetString(dataLossCount);
        }

        internal static string GetViewAsTextDiffButtonText()
        {
            return PlasticLocalization.Name
                .DiffDataLossViewAsTextButton.GetString();
        }
    }
}
