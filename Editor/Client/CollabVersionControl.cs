using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CollabProxy.Models;
using UnityEditor;
using UnityEditor.Collaboration;
using UnityEditor.Connect;

namespace CollabProxy.Client
{
    /// <summary>
    /// This class implements IVersionControl and registers itself as a provider of version control operations with
    /// Collab. This allows it to be used as the backend for Collab's core and user interface, through the exposed
    /// methods.
    /// </summary>IsJobRunning
    internal class CollabVersionControl : IVersionControl
    {
        const string k_CloneUrl = "{0}/api/projects/{1}/git";
        const string k_NoTip = "none";
        const int k_CollabGenericError = 1;
        const string k_GetWorkingDirectoryCallbackName = "GetWorkingDirectoryChangesCallback";

        protected IGitProxy Git { get; set; }
        CollabProxyClient m_CollabProxyClient;
        private CollabProxyClient CollabProxyClient
        {
            get { return m_CollabProxyClient; }
        }

        string m_LastKnownHead = k_NoTip;
        bool m_IsProjectBound;
        Mutex m_SetChangesToPublishMutex = new Mutex();

        public virtual bool IsJobRunning
        {
            get { return Git.IsRunningAsyncOperations(); }
        }

        /// <summary>
        /// Create a new instance of CollabVersionControl, consisting of a CollabProxyClient and GitProxy
        /// </summary>
        public CollabVersionControl()
        {
            m_CollabProxyClient = new CollabProxyClient();
            Git = new GitProxy(m_CollabProxyClient)
            {
                OnUpdateHeadListener = OnUpdateHead
            };
        }

        /// <summary>
        /// Create a new instance of CollabVersionControl, consisting of a CollabProxyClient and GitProxy
        /// </summary>
        public CollabVersionControl(CollabProxyClient collabProxyClient, IGitProxy gitProxy)
        {
            if (gitProxy == null)
            {
                throw new ArgumentNullException("gitProxy is required");
            }

            m_CollabProxyClient = collabProxyClient;
            Git = gitProxy;
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
            }
            catch (CollabProxyException initializationException)
            {
                LogExceptionDetails(initializationException);
                return false;
            }

            // Fire off initial set changes to publish
            OnFileSystemChanged();
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
            m_LastKnownHead = k_NoTip;
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
        /// Callback for when the Git repository is successfully updated
        /// </summary>
        void OnUpdateHead()
        {
            // ensure we update the view with the new state (if it exists)
            Collab.instance.SendCollabInfoNotification();
            OnFileSystemChanged();
        }

        void GetWorkingDirectoryChangesCallback(IList<ChangeWrapper> changes)
        {
            ChangeItem[] changeItems;
            if (changes != null)
            {
                IEnumerable<ChangeItem> collabChanges = changes.Where(change => !string.IsNullOrEmpty(change.Path))
                                                               .Select(ChangeWrapperToCollabChangeItem);
                changeItems = collabChanges.ToArray();
            }
            else
            {
                changeItems = new ChangeItem[0];
            }

            Collab.instance.SetChangesToPublish(changeItems);
        }

        /// <summary>
        /// Listens to the TCP connection for file system changes, then sets the new changelist in Collab
        /// </summary>
        protected virtual void OnFileSystemChanged()
        {
            m_SetChangesToPublishMutex.WaitOne();
            try
            {
                Git.GetWorkingDirectoryChangesAsync(k_GetWorkingDirectoryCallbackName);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                m_SetChangesToPublishMutex.ReleaseMutex();
            }
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
            if (tip != m_LastKnownHead)
            {
                try
                {
                    m_LastKnownHead = tip;
                    Git.SetCurrentHead(tip, CloudProjectSettings.accessToken);
                }
                catch (CollabProxyException e)
                {
                    LogExceptionDetails(e);
                }
            }
        }

        protected virtual void RegisterListeners()
        {
            Collab.instance.StateChanged += OnCollabStateChanged;
            UnityConnect.instance.ProjectStateChanged += OnProjectStateChanged;
            CollabProxyClient.RegisterListener(AsyncMessageType.FileSystemChanged.ToString(), OnFileSystemChanged);
            CollabProxyClient.RegisterListener<ChangeWrapper[]>(k_GetWorkingDirectoryCallbackName, GetWorkingDirectoryChangesCallback);
        }

        protected virtual void DeregisterListeners()
        {
            Collab.instance.StateChanged -= OnCollabStateChanged;
            UnityConnect.instance.ProjectStateChanged -= OnProjectStateChanged;
            CollabProxyClient.DeregisterListeners();
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
                Path = Path.DirectorySeparatorChar.Equals('\\') ? changeWrapper.Path.Replace('\\', '/') : changeWrapper.Path,
                State = state,
                Hash = changeWrapper.Hash
            };
        }

        /// <summary>
        /// Given a CollabProxyException, logs the most relevant message for the user into Unity's console,
        /// and outputs the full stacktrace into the logfile, as well as setting Collab's Error message
        /// </summary>
        /// <param name="exception">The collab proxy exception that was received</param>
        void LogExceptionDetails(CollabProxyException exception)
        {
            Collab.instance.SetError(k_CollabGenericError);
            UnityEngine.Debug.LogError(exception.InnermostExceptionMessage);
            Console.WriteLine(exception.InnermostExceptionStackTrace);
        }
    }
}
