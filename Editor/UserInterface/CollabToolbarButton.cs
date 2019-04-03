using System;
using System.Collections.Generic;
using UnityEditor.Collaboration;
using UnityEditor.Connect;
using UnityEditor.Web;
using UnityEngine;

namespace UnityEditor
{
    internal class CollabToolbarButton : SubToolbar, IDisposable
    {
        // Must match s_CollabIcon array
        enum CollabToolbarState
        {
            NeedToEnableCollab,
            UpToDate,
            Conflict,
            OperationError,
            ServerHasChanges,
            FilesToPush,
            InProgress,
            Disabled,
            Offline
        }

        string[] m_iconNames;
        string[] m_tooltipContents;
        CollabToolbarState m_CollabToolbarState = CollabToolbarState.UpToDate;
        Dictionary<CollabToolbarState, GUIContent> m_CollabIcons;
        const float kCollabButtonWidth = 78.0f;
        ButtonWithAnimatedIconRotation m_CollabButton;
        string m_DynamicTooltip;
        static bool m_ShowCollabTooltip = false;

        GUIContent currentCollabContent
        {
            get
            {
                GUIContent content = new GUIContent(GetIconForCurrentState());
                if (!m_ShowCollabTooltip)
                {
                    content.tooltip = null;
                }
                else if (m_DynamicTooltip != "")
                {
                    content.tooltip = m_DynamicTooltip;
                }

                if (Collab.instance.AreTestsRunning())
                {
                    content.text = "CTF";
                }

                return content;
            }
        }

        GUIContent GetIconForCurrentState()
        {
            if (m_CollabIcons == null)
            {
                m_CollabIcons = new Dictionary<CollabToolbarState, GUIContent>();
            }

            if (!m_CollabIcons.ContainsKey(m_CollabToolbarState))
            {
                m_CollabIcons.Add(m_CollabToolbarState, EditorGUIUtility.TrTextContentWithIcon("Collab", GetStateTooltip(), GetStateIconName()));
            }

            return m_CollabIcons[m_CollabToolbarState];
        }

        string GetStateTooltip()
        {
            return m_tooltipContents[(int) m_CollabToolbarState];
        }

        string GetStateIconName()
        {
            return m_iconNames[(int) m_CollabToolbarState];
        }

        public CollabToolbarButton()
        {
            m_iconNames = new[]
            {
                "CollabNew",
                "Collab",
                "CollabConflict",
                "CollabError",
                "CollabPull",
                "CollabPush",
                "CollabProgress",
                "CollabNew",
                "CollabNew"
            };

            m_tooltipContents = new[]
            {
                " You need to enable collab.",
                " You are up to date.",
                " Please fix your conflicts prior to publishing.",
                " Last operation failed. Please retry later.",
                " Please update, there are server changes.",
                " You have files to publish.",
                " Operation in progress.",
                " Collab is disabled.",
                " Please check your network connection."
            };

            Collab.instance.StateChanged += OnCollabStateChanged;
            UnityConnect.instance.StateChanged += OnUnityConnectStateChanged;
            UnityConnect.instance.UserStateChanged += OnUnityConnectUserStateChanged;
        }

        void OnUnityConnectUserStateChanged(UserInfo state)
        {
            UpdateCollabToolbarState();
        }

        void OnUnityConnectStateChanged(ConnectInfo state)
        {
            UpdateCollabToolbarState();
        }

        public override void OnGUI(Rect rect)
        {
            DoCollabDropDown(rect);
        }

        Rect GUIToScreenRect(Rect guiRect)
        {
            Vector2 screenPoint = GUIUtility.GUIToScreenPoint(new Vector2(guiRect.x, guiRect.y));
            guiRect.x = screenPoint.x;
            guiRect.y = screenPoint.y;
            return guiRect;
        }

        void ShowPopup(Rect rect)
        {
            // window should be centered on the button
            ReserveRight(kCollabButtonWidth / 2, ref rect);
            ReserveBottom(5, ref rect);
            // calculate screen rect before saving assets since it might open the AssetSaveDialog window
            var screenRect = GUIToScreenRect(rect);
            // save all the assets
            AssetDatabase.SaveAssets();
            if (Collab.ShowToolbarAtPosition != null && Collab.ShowToolbarAtPosition(screenRect))
            {
                GUIUtility.ExitGUI();
            }
        }

        void DoCollabDropDown(Rect rect)
        {
            UpdateCollabToolbarState();
            GUIStyle collabButtonStyle = "OffsetDropDown";
            bool showPopup = Toolbar.requestShowCollabToolbar;
            Toolbar.requestShowCollabToolbar = false;
            if(showPopup)
            {
                Toolbar.isLastShowRequestPartial = true;
            }

            bool enable = !EditorApplication.isPlaying;

            using (new EditorGUI.DisabledScope(!enable))
            {
                bool animate = m_CollabToolbarState == CollabToolbarState.InProgress;

                EditorGUIUtility.SetIconSize(new Vector2(12, 12));
                if (GetCollabButton().OnGUI(rect, currentCollabContent, animate, collabButtonStyle))
                {
                    showPopup = true;
                    Toolbar.isLastShowRequestPartial = false;
                }
                EditorGUIUtility.SetIconSize(Vector2.zero);
            }

            if (m_CollabToolbarState == CollabToolbarState.Disabled)
                return;

            if (showPopup)
            {
                ShowPopup(rect);
            }
        }

        public void OnCollabStateChanged(CollabInfo info)
        {
            UpdateCollabToolbarState();
        }

        public void UpdateCollabToolbarState()
        {
            var currentCollabState = CollabToolbarState.UpToDate;
            bool networkAvailable = UnityConnect.instance.connectInfo.online && UnityConnect.instance.connectInfo.loggedIn;
            m_DynamicTooltip = "";

            if (UnityConnect.instance.isDisableCollabWindow)
            {
                currentCollabState = CollabToolbarState.Disabled;
            }
            else if (networkAvailable)
            {
                Collab collab = Collab.instance;
                CollabInfo currentInfo = collab.collabInfo;
                UnityErrorInfo errInfo;
                bool error = false;
                if (collab.GetError((UnityConnect.UnityErrorFilter.ByContext | UnityConnect.UnityErrorFilter.ByChild), out errInfo))
                {
                    error = (errInfo.priority <= (int)UnityConnect.UnityErrorPriority.Error);
                    m_DynamicTooltip = errInfo.shortMsg;
                }

                if (!currentInfo.ready)
                {
                    currentCollabState = CollabToolbarState.InProgress;
                }
                else if (error)
                {
                    currentCollabState = CollabToolbarState.OperationError;
                }
                else if (currentInfo.inProgress)
                {
                    currentCollabState = CollabToolbarState.InProgress;
                }
                else
                {
                    bool collabEnable = Collab.instance.IsCollabEnabledForCurrentProject();

                    if (UnityConnect.instance.projectInfo.projectBound == false || !collabEnable)
                    {
                        currentCollabState = CollabToolbarState.NeedToEnableCollab;
                    }
                    else if (currentInfo.update)
                    {
                        currentCollabState = CollabToolbarState.ServerHasChanges;
                    }
                    else if (currentInfo.conflict)
                    {
                        currentCollabState = CollabToolbarState.Conflict;
                    }
                    else if (currentInfo.publish)
                    {
                        currentCollabState = CollabToolbarState.FilesToPush;
                    }
                }
            }
            else
            {
                currentCollabState = CollabToolbarState.Offline;
            }

            if (Collab.IsToolbarVisible != null)
            {
                if (currentCollabState != m_CollabToolbarState ||
                    Collab.IsToolbarVisible() == m_ShowCollabTooltip)
                {
                    m_CollabToolbarState = currentCollabState;
                    m_ShowCollabTooltip = !Collab.IsToolbarVisible();
                    Toolbar.RepaintToolbar();
                }
            }
        }

        void ReserveRight(float width, ref Rect pos)
        {
            pos.x += width;
        }

        void ReserveBottom(float height, ref Rect pos)
        {
            pos.y += height;
        }

        ButtonWithAnimatedIconRotation GetCollabButton()
        {
            if (m_CollabButton == null)
            {
                const int repaintsPerSecond = 20;
                const float animSpeed = 500f;
                const bool mouseDownButton = true;
                m_CollabButton = new ButtonWithAnimatedIconRotation(() => (float)EditorApplication.timeSinceStartup * animSpeed, Toolbar.RepaintToolbar, repaintsPerSecond, mouseDownButton);
            }

            return m_CollabButton;
        }

        public void Dispose()
        {
            Collab.instance.StateChanged -= OnCollabStateChanged;
            UnityConnect.instance.StateChanged -= OnUnityConnectStateChanged;
            UnityConnect.instance.UserStateChanged -= OnUnityConnectUserStateChanged;

            if (m_CollabButton != null)
                m_CollabButton.Clear();
        }
    }
} // namespace
