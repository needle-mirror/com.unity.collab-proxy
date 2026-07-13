using System.Collections.Generic;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

using Codice.Client.BaseCommands;
using Codice.Client.Common;
using PlasticGui;
using Unity.PlasticSCM.Editor.AssetUtils.Processor;

namespace Unity.PlasticSCM.Editor.AssetUtils
{
    internal interface ISaveAssets
    {
        void UnderWorkspaceWithConfirmation(
            string wkPath,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            bool canContinueWithDirtyScenes,
            out bool isCancelled);

        void ForChangesWithConfirmation(
            string wkPath,
            List<ChangeInfo> changes,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            bool canContinueWithDirtyScenes,
            out bool isCancelled);

        void ForPathsWithConfirmation(
            string wkPath,
            List<string> paths,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            bool canContinueWithDirtyScenes,
            out bool isCancelled);

        void ForChangesWithoutConfirmation(
            string wkPath,
            List<ChangeInfo> changes,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            bool canContinueWithDirtyScenes,
            out bool isCancelled);

        void ForPathsWithoutConfirmation(
            string wkPath,
            List<string> paths,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            bool canContinueWithDirtyScenes,
            out bool isCancelled);
    }

    internal class SaveAssets : ISaveAssets
    {
        void ISaveAssets.UnderWorkspaceWithConfirmation(
            string wkPath,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            bool canContinueWithDirtyScenes,
            out bool isCancelled)
        {
            ForPaths(
                wkPath,
                null,
                workspaceOperationsMonitor,
                askForUserConfirmation: true,
                canContinueWithDirtyScenes,
                out isCancelled);
        }

        void ISaveAssets.ForChangesWithConfirmation(
            string wkPath,
            List<ChangeInfo> changes,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            bool canContinueWithDirtyScenes,
            out bool isCancelled)
        {
            ForPaths(
                wkPath,
                GetPaths(changes),
                workspaceOperationsMonitor,
                askForUserConfirmation: true,
                canContinueWithDirtyScenes,
                out isCancelled);
        }

        void ISaveAssets.ForPathsWithConfirmation(
            string wkPath,
            List<string> paths,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            bool canContinueWithDirtyScenes,
            out bool isCancelled)
        {
            ForPaths(
                wkPath,
                paths,
                workspaceOperationsMonitor,
                askForUserConfirmation: true,
                canContinueWithDirtyScenes,
                out isCancelled);
        }

        void ISaveAssets.ForChangesWithoutConfirmation(
            string wkPath,
            List<ChangeInfo> changes,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            bool canContinueWithDirtyScenes,
            out bool isCancelled)
        {
            ForPaths(
                wkPath,
                GetPaths(changes),
                workspaceOperationsMonitor,
                askForUserConfirmation: false,
                canContinueWithDirtyScenes,
                out isCancelled);
        }

        void ISaveAssets.ForPathsWithoutConfirmation(
            string wkPath,
            List<string> paths,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            bool canContinueWithDirtyScenes,
            out bool isCancelled)
        {
            ForPaths(
                wkPath,
                paths,
                workspaceOperationsMonitor,
                askForUserConfirmation: false,
                canContinueWithDirtyScenes,
                out isCancelled);
        }

        static void ForPaths(
            string wkPath,
            List<string> paths,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            bool askForUserConfirmation,
            bool canContinueWithDirtyScenes,
            out bool isCancelled)
        {
            workspaceOperationsMonitor.Disable();
            try
            {
                SaveDirtyScenes(
                    wkPath,
                    paths,
                    askForUserConfirmation,
                    canContinueWithDirtyScenes,
                    out isCancelled);

                if (isCancelled)
                    return;

                AssetDatabase.SaveAssets();
            }
            finally
            {
                workspaceOperationsMonitor.Enable();
            }
        }

        static void SaveDirtyScenes(
            string wkPath,
            List<string> paths,
            bool askForUserConfirmation,
            bool canContinueWithDirtyScenes,
            out bool isCancelled)
        {
            List<Scene> scenesToSave = GetScenesToSave(wkPath, paths);

            if (scenesToSave.Count == 0)
            {
                isCancelled = false;
                return;
            }

            if (EditorApplication.isPlaying)
            {
                SaveDirtyScenesInPlayMode(canContinueWithDirtyScenes, out isCancelled);
                return;
            }

            SaveDirtyScenesInEditMode(scenesToSave, askForUserConfirmation, out isCancelled);
        }

        static void SaveDirtyScenesInPlayMode(
            bool canContinueWithDirtyScenes,
            out bool isCancelled)
        {
            // Saving scenes during PlayMode is currently forbidden by the Editor, so we either inform the users
            // about the blocker or let the operations continue without saving scenes (if requested)

            if (canContinueWithDirtyScenes)
            {
                isCancelled = !EditorUtility.DisplayDialog(
                    PlasticLocalization.Name.DirtyScenesInPlayModeTitle.GetString(),
                    PlasticLocalization.Name.DirtyScenesInPlayModeMessage.GetString(),
                    PlasticLocalization.Name.ContinueWithoutSavingButton.GetString(),
                    PlasticLocalization.Name.CancelButton.GetString());

                return;
            }

            EditorUtility.DisplayDialog(
                PlasticLocalization.Name.DirtyScenesInPlayModeTitle.GetString(),
                PlasticLocalization.Name.DirtyScenesInPlayModeBlockingMessage.GetString(),
                PlasticLocalization.Name.OkButton.GetString());

            isCancelled = true;
        }

        static void SaveDirtyScenesInEditMode(
            List<Scene> scenesToSave,
            bool askForUserConfirmation,
            out bool isCancelled)
        {
            isCancelled = false;

            if (askForUserConfirmation)
            {
                isCancelled = !EditorSceneManager.SaveModifiedScenesIfUserWantsTo(scenesToSave.ToArray());

                if (!isCancelled)
                    DiscardChangesInActiveSceneIfDirty(scenesToSave);

                return;
            }

            EditorSceneManager.SaveScenes(scenesToSave.ToArray());
        }

        static void DiscardChangesInActiveSceneIfDirty(List<Scene> scenesToSave)
        {
            string activeScenePath = EditorSceneManager.GetActiveScene().path;
            Scene? activeScene = GetSceneByPath(scenesToSave, activeScenePath);

            if (activeScene == null)
                return;

            if (!activeScene.Value.isDirty)
                return;

            EditorSceneManager.OpenScene(activeScenePath);
        }

        static Scene? GetSceneByPath(List<Scene> scenes, string scenePath)
        {
            foreach (Scene scene in scenes)
            {
                if (scene.path == scenePath)
                    return scene;
            }

            return null;
        }

        static List<Scene> GetScenesToSave(string wkPath, List<string> paths)
        {
            List<Scene> dirtyScenes = GetDirtyScenesUnderWorkspace(wkPath);

            if (paths == null)
                return dirtyScenes;

            List<Scene> scenesToSave = new List<Scene>();

            foreach (Scene dirtyScene in dirtyScenes)
            {
                if (Contains(paths, dirtyScene))
                    scenesToSave.Add(dirtyScene);
            }

            return scenesToSave;
        }

        static List<Scene> GetDirtyScenesUnderWorkspace(string wkPath)
        {
            List<Scene> dirtyScenes = new List<Scene>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (!scene.isDirty)
                    continue;

                if (string.IsNullOrEmpty(scene.path))
                    continue;

                string fullPath = AssetsPath.GetFullPath.ForPath(scene.path);

                if (!PathHelper.IsContainedOn(fullPath, wkPath))
                    continue;

                dirtyScenes.Add(scene);
            }

            return dirtyScenes;
        }

        static bool Contains(
            List<string> paths,
            Scene scene)
        {
            foreach (string path in paths)
            {
                if (PathHelper.IsSamePath(
                        path,
                        AssetsPath.GetFullPath.ForPath(scene.path)))
                    return true;
            }

            return false;
        }

        static List<string> GetPaths(
            List<ChangeInfo> changeInfos)
        {
            List<string> result = new List<string>();
            foreach (ChangeInfo change in changeInfos)
                result.Add(change.GetFullPath());
            return result;
        }
    }
}
