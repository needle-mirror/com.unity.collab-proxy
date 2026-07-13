using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.UnityObject
{
    // Classifies objects returned by InternalEditorUtility.LoadSerializedFileAndForget
    // to detect cases where the loader silently dropped serialized data.
    //
    // Each rule below names a specific failure mode in Unity's loader:
    //
    //  - Type was substituted with UnityEditor.FallbackEditorWindow because
    //    the MonoScript could not be resolved (the loader uses the
    //    kAutoreplaceEditorWindow flag — see EditorWindowController.cpp in
    //    the Unity reference repo).
    //  - MonoBehaviour with no resolvable script reference. Fields beyond
    //    base MonoBehaviour are gone.
    //  - AnimatorOverrideController with a null runtimeAnimatorController.
    //    AwakeFromLoad → BuildAsset wipes m_Clips whenever the controller
    //    is null (see Modules/Animation/AnimatorOverrideController.cpp),
    //    so any clip-override data in the YAML is hidden from the diff.
    //
    // New failure modes go here as new rules. We deliberately do not
    // surface a generic "something looks off" signal — every kind names a
    // concrete problem so the UI can explain it accurately.
    internal static class DataLossDetection
    {
        internal static Dictionary<UnityEngine.Object, DataLossKind> DetectPerObject(
            UnityEngine.Object[] loadedObjects)
        {
            Dictionary<UnityEngine.Object, DataLossKind> map =
                new Dictionary<UnityEngine.Object, DataLossKind>();

            if (loadedObjects == null)
                return map;

            foreach (UnityEngine.Object obj in loadedObjects)
            {
                if (obj == null)
                    continue;

                DataLossKind kind = Classify(obj);
                if (kind != DataLossKind.None)
                    map[obj] = kind;
            }

            return map;
        }

        internal static DataLossKind LookupOrNone(
            Dictionary<UnityEngine.Object, DataLossKind> map,
            UnityEngine.Object obj)
        {
            if (obj == null || map == null)
                return DataLossKind.None;

            return map.TryGetValue(obj, out DataLossKind kind)
                ? kind
                : DataLossKind.None;
        }

        // Higher enum value wins (most specific reason surfaced to the user).
        internal static DataLossKind PickMoreSpecific(DataLossKind a, DataLossKind b)
        {
            return (int)a >= (int)b ? a : b;
        }

        static DataLossKind Classify(UnityEngine.Object obj)
        {
            // UnityEditor.FallbackEditorWindow is internal; match by full
            // type name to avoid a hard reference. Catches the case where
            // the loader substituted the type because the script couldn't
            // be resolved.
            if (obj.GetType().FullName == FALLBACK_EDITOR_WINDOW_TYPE_NAME)
                return DataLossKind.FallbackEditorWindow;

            if (obj is MonoBehaviour mb)
            {
                MonoScript script = MonoScript.FromMonoBehaviour(mb);
                if (script == null || script.GetClass() == null)
                    return DataLossKind.UnresolvedMonoScript;
            }

            if (obj is AnimatorOverrideController aoc
                && aoc.runtimeAnimatorController == null)
            {
                // BuildAsset wipes m_Clips whenever the controller is
                // null, regardless of WHY it's null (unresolvable external
                // reference, deleted asset, or genuinely unassigned). The
                // warning is accurate either way: any clip-override data
                // in the YAML will be hidden from the diff. An AOC that
                // legitimately has no controller AND no clips is
                // byte-identical across src/dst, so the diff layer never
                // calls into this detector for that case.
                return DataLossKind.UnresolvedAnimatorController;
            }

            return DataLossKind.None;
        }

        const string FALLBACK_EDITOR_WINDOW_TYPE_NAME =
            "UnityEditor.FallbackEditorWindow";
    }
}
