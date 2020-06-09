using System.Linq;
using Unity.Cloud.Collaborate.Models.Providers.Client;
using UnityEditor;
using UnityEditor.Collaboration;
using UnityEditor.Connect;
using UnityEngine;

namespace Unity.Cloud.Collaborate.UserInterface
{
    internal class ToolbarButtonV2 : ToolbarButton
    {
        bool m_IsGettingChanges;

        public ToolbarButtonV2()
        {
            CollabVersionControl.GetChangesStarted += OnGetChangesStart;
            CollabVersionControl.GetChangesFinished += OnGetChangesFinish;
            CollabVersionControl.UpdateCachedChangesFinished += OnUpdateCachedChangesFinish;
        }

        void OnGetChangesStart()
        {
            m_IsGettingChanges = true;
            EditorApplication.delayCall += Update;
        }

        void OnGetChangesFinish()
        {
            m_IsGettingChanges = false;
            Update();
        }

        void OnUpdateCachedChangesFinish(bool ignore)
        {
            m_IsGettingChanges = false;
            Update();
        }

        protected override ToolbarButtonState GetCurrentState()
        {
            var currentState = ToolbarButtonState.UpToDate;
            var networkAvailable = UnityConnect.instance.connectInfo.online && UnityConnect.instance.connectInfo.loggedIn;
            m_ErrorMessage = string.Empty;

            if (UnityConnect.instance.isDisableCollabWindow)
            {
                currentState = ToolbarButtonState.Disabled;
            }
            else if (networkAvailable)
            {
                var collab = Collab.instance;
                var currentInfo = collab.collabInfo;

                if (!currentInfo.ready)
                {
                    currentState = ToolbarButtonState.InProgress;
                }
                else if (collab.GetError(UnityConnect.UnityErrorFilter.ByContext | UnityConnect.UnityErrorFilter.ByChild, out var errInfo) &&
                    errInfo.priority <= (int)UnityConnect.UnityErrorPriority.Error)
                {
                    currentState = ToolbarButtonState.OperationError;
                    m_ErrorMessage = errInfo.shortMsg;
                }
                else if (currentInfo.inProgress)
                {
                    currentState = ToolbarButtonState.InProgress;
                }
                else if (m_IsGettingChanges)
                {
                    currentState = ToolbarButtonState.InProgress;
                }
                else
                {
                    var collabEnabled = Collab.instance.IsCollabEnabledForCurrentProject();

                    if (UnityConnect.instance.projectInfo.projectBound == false || !collabEnabled)
                    {
                        currentState = ToolbarButtonState.NeedToEnableCollab;
                    }
                    else if (currentInfo.update)
                    {
                        currentState = ToolbarButtonState.ServerHasChanges;
                    }
                    else if (currentInfo.conflict)
                    {
                        currentState = ToolbarButtonState.Conflict;
                    }
                    // Check if there are any un-ignored files available to publish.
                    else if (collab.GetChangesToPublish_V2().changes.Any(e => e.State != Collab.CollabStates.kCollabIgnored))
                    {
                        currentState = ToolbarButtonState.FilesToPush;
                    }
                }
            }
            else
            {
                currentState = ToolbarButtonState.Offline;
            }

            return currentState;
        }
    }
}
