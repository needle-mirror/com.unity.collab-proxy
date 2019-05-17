using System;
using System.Collections.Generic;
using System.Linq;
using CollabProxy.Models;
using UnityEditor;
using UnityEditor.Collaboration;
using UnityEditor.Connect;
using UnityEngine;

namespace CollabProxy.Client
{
    /// <summary>
    /// This class implements IVersionControl and registers itself as a provider of version control operations with
    /// Collab. This allows it to be used as the backend for Collab's core and user interface, through the exposed
    /// methods.
    /// </summary>
    internal class CollabVersionControl : IVersionControl
    {
        const string k_CloneUrl = "{0}/api/projects/{1}/git";
        const string k_NoTip = "none";
        const int k_CollabGenericError = 1;

        IGitProxy Git { get; set; }
        CollabProxyClient m_CollabProxyClient;

        CollabProxyClient CollabProxyClient
        {
            get { return m_CollabProxyClient; }
        }

        bool m_IsProjectBound;
        bool m_GitRepoExists;

        public static bool IsGettingChanges;
        public static bool IsHeadUpdating;
        public static event Action GetChangesFinished;
        public static event Action GetChangesStarted;
        public static event Action<bool> UpdateCachedChangesFinished;

        public virtual bool IsJobRunning
        {
            get { return IsHeadUpdating; }
        }

        /// <summary>
        /// Create a new instance of CollabVersionControl, consisting of a CollabProxyClient and GitProxy
        /// </summary>
        public CollabVersionControl()
        {
            m_CollabProxyClient = new CollabProxyClient();
            Git = new GitProxy(m_CollabProxyClient);
            RegisterServerCallbacks();
        }

        /// <summary>
        /// Create a new instance of CollabVersionControl, consisting of a CollabProxyClient and GitProxy
        /// </summary>
        protected CollabVersionControl(CollabProxyClient collabProxyClient, IGitProxy gitProxy)
        {
            if (gitProxy == null)
            {
                throw new ArgumentNullException("gitProxy", "gitProxy is required");
            }

            m_CollabProxyClient = collabProxyClient;
            Git = gitProxy;
            RegisterServerCallbacks();
        }

        /// <summary>
        /// De-register listeners to the proxy server on destruction.
        /// </summary>
        ~CollabVersionControl()
        {
            CollabProxyClient.DeregisterListeners();
        }

        /// <summary>
        /// Callbacks from the various async calls to the proxy server.
        /// </summary>
        protected virtual void RegisterServerCallbacks()
        {
            CollabProxyClient.RegisterListener<bool>(AsyncMessageType.CurrentHeadUpdated.ToString(), OnUpdateHead, OnUpdateHeadException);
            CollabProxyClient.RegisterListener<List<ChangeWrapper>>(AsyncMessageType.GetChanges.ToString(), OnGetChanges, OnGetChangesException);
            CollabProxyClient.RegisterListener<List<ChangeWrapper>>(AsyncMessageType.UpdateCachedChanges.ToString(), OnUpdateCachedChanges, OnUpdateCachedChangesException);
            CollabProxyClient.RegisterListener<List<ChangeWrapper>>(AsyncMessageType.UpdateFileStatus.ToString(), OnUpdateFileStatus, OnUpdateFileStatusException);
        }

        /// <summary>
        /// Whether or not the package supports async API to fetch file changes.
        /// </summary>
        public bool SupportsAsyncChanges()
        {
            return true;
        }

        /// <summary>
        /// Called by native code if an implementation of IVersionControl is registered, after performing downloads,
        /// but before performing merges and conflict resolution
        /// </summary>
        /// <returns>false if legacy code should be used, true for using MergeDownloadedFiles</returns>
        public bool SupportsDownloads()
        {
            return false;
        }

        /// <summary>
        /// Called by native code if an implementation of IVersionControl is registered, either at registration (if
        /// Collab is already enabled), or whenever Collab is enabled by the user
        /// </summary>
        public bool OnEnableVersionControl()
        {
            bool repositoryFound = Git.RepositoryExists();
            if (!repositoryFound && String.IsNullOrEmpty(GetProjectId()))
            {
                // Cannot create a new repository with no project Id -- wait for project binding
                UnityConnect.instance.ProjectStateChanged += EnableOnProjectBound;
                return true;
            }

            try
            {
                Git.InitializeRepository();
                if (!repositoryFound)
                {
                    string remoteUrl = string.Format(k_CloneUrl, GetBaseUrl(), GetProjectId());
                    Git.SetRemoteOrigin(remoteUrl);
                    IgnoreFileManager.Instance.CreateOrMigrateIgnoreFile();
                }

                UpdateHeadRevision();
                RegisterListeners();
                m_GitRepoExists = true;

                if (!repositoryFound)
                {
                    StartGetChanges();
                }
            }
            catch (CollabProxyException initializationException)
            {
                LogExceptionDetails(initializationException);
                return false;
            }

            return true;
        }

        /// <summary>
        /// This is the fallback for enabling version control when a project has not been bound in time
        /// </summary>
        /// <param name="projectInfo">Project info from the notification</param>
        void EnableOnProjectBound(ProjectInfo projectInfo)
        {
            if (String.IsNullOrEmpty(projectInfo.projectGUID)) return;
            UnityConnect.instance.ProjectStateChanged -= EnableOnProjectBound;
            OnEnableVersionControl();
        }

        /// <summary>
        /// Called by native code if an implementation of IVersionControl is registered, either at registration (if
        /// Collab is already disabled), or whenever Collab is disabled by the user
        /// </summary>
        public void OnDisableVersionControl()
        {
            DeregisterListeners();
        }

        /// <summary>
        /// Listens for collab state changes, sets the current HEAD in Git if tip changes
        /// </summary>
        /// <param name="info">CollabInfo</param>
        void OnCollabStateChanged(CollabInfo info)
        {
            UpdateHeadRevision(info.tip);
        }

        /// <summary>
        /// Listens for project state changes, removes repository if a project was unlinked
        /// </summary>
        /// <param name="info">ProjectInfo</param>
        void OnProjectStateChanged(ProjectInfo info)
        {
            if (info.projectBound)
            {
                m_IsProjectBound = true;
            }
            else
            {
                if (m_IsProjectBound)
                {
                    try
                    {
                        m_IsProjectBound = false;
                        Git.RemoveRepository();
                    }
                    catch (CollabProxyException e)
                    {
                        LogExceptionDetails(e);
                    }
                }
            }
        }

        /// <summary>
        /// Callback for when the head of the Git repository is successfully updated
        /// </summary>
        void OnUpdateHead(bool changeMade)
        {
            IsHeadUpdating = false;
            // ensure we update the view with the new state
            Collab.instance.SendCollabInfoNotification();
            if (changeMade)
            {
                StartGetChanges();
            }
        }

        static void OnUpdateHeadException(Exception e)
        {
            IsHeadUpdating = false;
            // ensure we update the view with the new state
            Collab.instance.SendCollabInfoNotification();
            LogExceptionDetails(e);
        }

        static void OnGetChanges(List<ChangeWrapper> changes)
        {
            SetChangesToPublish(changes);
            // Notify new changes and call event on main thread.
            IsGettingChanges = false;
            if (GetChangesFinished != null)
                EditorApplication.delayCall += () => GetChangesFinished();
        }

        static void OnGetChangesException(Exception e)
        {
            // Notify new changes and call event on main thread.
            IsGettingChanges = false;
            if (GetChangesFinished != null)
                EditorApplication.delayCall += () => GetChangesFinished();
            LogExceptionDetails(e);
        }

        static void OnUpdateCachedChanges(List<ChangeWrapper> changes)
        {
            var numberOfChanges = SetChangesToPublish(changes);
            // Notify new changes and call event on main thread.
            IsGettingChanges = false;
            if (UpdateCachedChangesFinished != null)
                EditorApplication.delayCall += () => UpdateCachedChangesFinished(numberOfChanges > 0);
        }

        static void OnUpdateCachedChangesException(Exception e)
        {
            // Notify new changes and call event on main thread.
            IsGettingChanges = false;
            if (UpdateCachedChangesFinished != null)
                EditorApplication.delayCall += () => UpdateCachedChangesFinished(false);
            LogExceptionDetails(e);
        }

        static void OnUpdateFileStatus(ICollection<ChangeWrapper> changes)
        {
            SetChangesToPublish(changes);
        }

        static void OnUpdateFileStatusException(Exception e)
        {
            LogExceptionDetails(e);
        }

        static int SetChangesToPublish(ICollection<ChangeWrapper> changes)
        {
            var length = 0;
            ChangeItem[] changeItems;
            if (changes != null)
            {
                // Convert change wrapper to change items
                changeItems = changes.Where(change => !string.IsNullOrEmpty(change.Path))
                    .Select(ChangeWrapperToCollabChangeItem)
                    .ToArray();
                length = changeItems.Length;
            }
            else
            {
                changeItems = new ChangeItem[0];
            }

            Collab.instance.SetChangesToPublish(changeItems);
            return length;
        }

        protected virtual string GetProjectId()
        {
            return CloudProjectSettings.projectId;
        }

        protected virtual string GetAccessToken()
        {
            return CloudProjectSettings.accessToken;
        }

        protected virtual string GetCollabTip()
        {
            return Collab.instance.collabInfo.tip;
        }

        ChangeItem[] IVersionControl.GetChanges()
        {
            // shouldn't call this !
            throw new NotImplementedException();
        }

        public void StartUpdateFileStatus(string path)
        {
            Git.UpdateFileStatusAsync(path);
        }

        public void MergeDownloadedFiles(bool isFullDownload)
        {
            throw new NotImplementedException();
        }

        Collab.CollabStates IVersionControl.GetAssetState(string assetGuid, string assetPath)
        {
            return Collab.instance.GetAssetState(assetGuid);
        }

        protected virtual string GetBaseUrl()
        {
            string environment = UnityConnect.instance.GetEnvironment();
            switch (environment)
            {
                    case "local":
                        return "https://dev-collab.cloud.unity3d.com";
                    case "dev":
                        return "https://andy-collab.cloud.unity3d.com";
                    case "staging":
                        return "https://staging-collab.cloud.unity3d.com";
                    default:
                        return "https://collab.cloud.unity3d.com";
            }
        }

        protected virtual void UpdateHeadRevision()
        {
            UpdateHeadRevision(Collab.instance.collabInfo.tip);
        }

        void UpdateHeadRevision(string tip)
        {
            if (tip != k_NoTip)
            {
                IsHeadUpdating = true;
                Git.SetCurrentHeadAsync(tip, CloudProjectSettings.accessToken);
            }
        }

        internal void StartGetChanges()
        {
            // Don't get changes on a non-existent repo
            if (!m_GitRepoExists) return;

            IsGettingChanges = true;
            if (GetChangesStarted != null)
                EditorApplication.delayCall += () => GetChangesStarted();
            Git.GetWorkingDirectoryChangesAsync();
        }

        internal void StartUpdateCachedChanges()
        {
            IsGettingChanges = true;
            if (GetChangesStarted != null)
                EditorApplication.delayCall += () => GetChangesStarted();
            Git.UpdateCachedChangesAsync();
        }

        protected virtual void RegisterListeners()
        {
            Collab.instance.StateChanged += OnCollabStateChanged;
            UnityConnect.instance.ProjectStateChanged += OnProjectStateChanged;
        }

        protected virtual void DeregisterListeners()
        {
            Collab.instance.StateChanged -= OnCollabStateChanged;
            UnityConnect.instance.ProjectStateChanged -= OnProjectStateChanged;
        }

        /// <summary>
        /// Helper method to convert a given ChangeWrapper object into the format Collab expects
        /// </summary>
        /// <param name="changeWrapper">ChangeWrapper containing Change info</param>
        /// <returns></returns>
        internal static ChangeItem ChangeWrapperToCollabChangeItem(ChangeWrapper changeWrapper)
        {
            Collab.CollabStates state;
            switch (changeWrapper.Status)
            {
                case ChangeType.Modified:
                    state = Collab.CollabStates.kCollabCheckedOutLocal;
                    break;
                case ChangeType.Added:
                    state = Collab.CollabStates.kCollabAddedLocal;
                    break;
                case ChangeType.Deleted:
                    state = Collab.CollabStates.kCollabDeletedLocal;
                    break;
                case ChangeType.Moved:
                case ChangeType.Renamed:
                    state = Collab.CollabStates.kCollabMovedLocal;
                    break;
                default:
                    state = Collab.CollabStates.kCollabNone;
                    break;
            }

            if (changeWrapper.IsFolder)
            {
                state |= Collab.CollabStates.kCollabFolderMetaFile;
                state |= Collab.CollabStates.kCollabMetaFile;
            }

            return new ChangeItem
            {
                Path = changeWrapper.Path,
                State = state,
                Hash = changeWrapper.Hash
            };
        }

        /// <summary>
        /// Given a Exception, logs the most relevant message for the user into Unity's console,
        /// and outputs the full stacktrace into the logfile, as well as setting Collab's Error message
        /// if it's a CollabProxyException.
        /// </summary>
        /// <param name="exception">The collab proxy exception that was received</param>
        static void LogExceptionDetails(Exception exception)
        {
            if (exception.GetType() == typeof(CollabProxyException))
            {
                CollabProxyException ce = (CollabProxyException) exception;
                EditorApplication.delayCall += () => Collab.instance.SetError(k_CollabGenericError);
                Debug.LogError("CollabProxyException: " + ce.InnermostExceptionMessage);
                Console.WriteLine(ce.InnermostExceptionStackTrace);
            }
            else
            {
                Debug.LogException(exception);
            }
        }
    }
}
