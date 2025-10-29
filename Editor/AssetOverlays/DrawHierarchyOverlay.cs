using System;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using Codice.Client.Common.Threading;
using Codice.LogWrapper;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.AssetUtils;

using Object = UnityEngine.Object;

namespace Unity.PlasticSCM.Editor.AssetsOverlays
{
    internal static class DrawHierarchyOverlay
    {
        internal interface IGetAssetPathFromInstance
        {
            bool TryGetAssetPath(int instanceID, out string assetPath);
        }

        internal static IGetAssetPathFromInstance GetSubSceneAssetPath;

        internal static void Enable(
            string wkPath,
            IAssetStatusCache assetStatusCache)
        {
            if (mIsEnabled)
                return;

            mLog.Debug("Enable");

            mWkPath = wkPath;
            mAssetStatusCache = assetStatusCache;

            mIsEnabled = true;

#if UNITY_6000_4_OR_NEWER
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnHierarchyGUIByEntityId;
#else
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
#endif

            RepaintEditor.HierarchyWindow();
        }

        internal static void Disable()
        {
            mLog.Debug("Disable");

            mIsEnabled = false;

#if UNITY_6000_4_OR_NEWER
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI -= OnHierarchyGUIByEntityId;
#else
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
#endif

            RepaintEditor.HierarchyWindow();

            mWkPath = null;
            mAssetStatusCache = null;
        }

#if UNITY_6000_4_OR_NEWER
        static void OnHierarchyGUIByEntityId(EntityId entityId, Rect selectionRect)
        {
            OnHierarchyGUI(entityId, selectionRect);
        }
#endif

        static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            try
            {
                string assetPath = GetAssetPathFromInstanceID(instanceID);

                if (assetPath == null)
                    return;

                string assetFullPath = AssetsPath.GetFullPathUnderWorkspace.ForAsset(mWkPath, assetPath);

                if (assetFullPath == null)
                    return;

                DrawOverlayForAsset(assetFullPath, selectionRect, mAssetStatusCache);
            }
            catch (Exception ex)
            {
                ExceptionsHandler.LogException(typeof(DrawHierarchyOverlay).Name, ex);
            }
        }

        static string GetAssetPathFromInstanceID(int instanceID)
        {
            string sceneAssetPath;
            if (TryGetAssetPathForScene(instanceID, out sceneAssetPath))
            {
                return sceneAssetPath;
            }

            string subSceneAssetPath;
            if (TryGetAssetPathForSubScene(instanceID, out subSceneAssetPath))
            {
                return subSceneAssetPath;
            }

            string prefabAssetPath;
            if (TryGetAssetPathForPrefab(instanceID, out prefabAssetPath))
            {
                return prefabAssetPath;
            }

            return null;
        }

        static bool TryGetAssetPathForScene(int instanceID, out string assetPath)
        {
            assetPath = null;

            if (FindUnityObject.ForInstanceID(instanceID) != null)
                return false;

            assetPath = FindScenePathForHandle(instanceID);

            return assetPath != null;
        }

        static string FindScenePathForHandle(int sceneHandle)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

#if UNITY_6000_4_OR_NEWER
                if (scene.handle == SceneHandle.FromRawData((ulong)sceneHandle) && scene.path != null)
#else
                if (scene.handle == sceneHandle && scene.path != null)
#endif
                {
                    return scene.path;
                }
            }

            return null;
        }

        static bool TryGetAssetPathForSubScene(int instanceID, out string assetPath)
        {
            assetPath = null;

            string subSceneAssetPath;
            if (GetSubSceneAssetPath != null &&
                GetSubSceneAssetPath.TryGetAssetPath(instanceID, out subSceneAssetPath))
            {
                assetPath = subSceneAssetPath;
            }

            return assetPath != null;
        }

        static bool TryGetAssetPathForPrefab(int instanceID, out string assetPath)
        {
            assetPath = null;

            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage == null)
                return false;

            Object hierarchyObject = FindUnityObject.ForInstanceID(instanceID);

            if (hierarchyObject == null)
                return false;

            if (prefabStage.prefabContentsRoot == hierarchyObject)
                assetPath = prefabStage.assetPath;

            return assetPath != null;
        }

        static void DrawOverlayForAsset(
            string assetFullPath,
            Rect selectionRect,
            IAssetStatusCache assetStatusCache)
        {
            AssetStatus assetStatus = assetStatusCache.GetStatus(assetFullPath);

            string tooltipText = AssetOverlay.GetTooltipText(
                assetStatus,
                assetStatusCache.GetLockStatusData(assetFullPath));

            DrawAssetOverlayIcon.ForStatus(
                selectionRect,
                assetStatus,
                tooltipText);
        }

        static bool mIsEnabled;
        static IAssetStatusCache mAssetStatusCache;
        static string mWkPath;

        static readonly ILog mLog = PlasticApp.GetLogger("DrawHierarchyOverlay");
    }
}
