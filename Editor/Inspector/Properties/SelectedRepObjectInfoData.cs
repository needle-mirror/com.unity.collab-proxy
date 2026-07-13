using System;
using Codice.CM.Common;
using Codice.CM.Common.Mount;
using UnityEditor;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Inspector.Properties
{
    internal class SelectedRepObjectInfoData : ScriptableObject
    {
        internal RepObjectInfo ObjectInfo { get; private set; }
        internal RepositorySpec RepSpec { get; private set; }
        internal MountPointWithPath MountPoint { get; private set; }

        internal static SelectedRepObjectInfoData Create(
            RepObjectInfo objectInfo,
            RepositorySpec repSpec,
            MountPointWithPath mountPoint = null)
        {
            SelectedRepObjectInfoData result = CreateInstance<SelectedRepObjectInfoData>();
            result.ObjectInfo = objectInfo;
            result.RepSpec = repSpec;
            result.MountPoint = mountPoint;

            return result;
        }

        internal static void SetActiveObject(
            RepObjectInfo objectInfo,
            RepositorySpec repSpec,
            MountPointWithPath mountPoint = null)
        {
            // Reassigning Selection.activeObject to a new instance makes Unity rebuild
            // the Inspector and recompute the diff, losing the current file selection.
            // Skip it when the same object is already displayed (VCS-1008599).
            if (IsSameObjectSelected(objectInfo, repSpec))
                return;

            Selection.activeObject = Create(objectInfo, repSpec, mountPoint);
        }

        static bool IsSameObjectSelected(
            RepObjectInfo objectInfo,
            RepositorySpec repSpec)
        {
            SelectedRepObjectInfoData current =
                Selection.activeObject as SelectedRepObjectInfoData;

            if (current == null || current.ObjectInfo == null || objectInfo == null)
                return false;

            return current.ObjectInfo.Equals(objectInfo)
                && current.RepSpec.Equals(repSpec);
        }
    }
}
