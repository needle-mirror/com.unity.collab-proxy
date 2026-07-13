using System;

using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

using Codice.Client.Common.EventTracking;
using Codice.CM.Common;
using PlasticGui;
using Unity.PlasticSCM.Editor.Toolbar;
using Unity.PlasticSCM.Editor.UI;

using SettingsWindow = Unity.PlasticSCM.Editor.UnityInternals.UnityEditor.SettingsWindow;

namespace Unity.PlasticSCM.Editor.Settings
{
    /// <summary>
    /// Project Settings UI for Unity Version Control.
    /// <para>
    /// Unity associates this object with the
    /// &quot;Unity Version Control&quot; entry in <b>Version Control</b> mode (see
    /// <see cref="VersionControlObject"/> and <see cref="VersionControlAttribute"/>).
    /// When the user picks that mode, Unity shows this provider&apos;s inspector for editing
    /// version-control-related project settings.
    /// </para>
    /// <para>
    /// <see cref="OnActivate"/> is called when this provider becomes the active Version Control
    /// mode, after the user selects &quot;Unity Version Control&quot; in the mode dropdown.
    /// </para>
    /// <para>
    /// <see cref="OnDeactivate"/> is called when this provider stops being the active Version Control mode.
    /// </para>
    /// </summary>
    [VersionControl("Unity Version Control")]
    internal class UVCSProjectSettingsProvider : VersionControlObject, ISettingsInspectorExtension
    {
        void OnEnable()
        {
            PlasticApp.InitializeIfNeeded();

            mUVCSPlugin = UVCSPlugin.Instance;
            mUpdateToolbarButtonVisibility = UVCSToolbar.Controller;

            ReloadSettings();
        }

        public override void OnActivate()
        {
            EditorWindow currentWindow = EditorWindow.focusedWindow;

            SwitchUVCSPlugin.OnIfNeeded(mUVCSPlugin);

            if (SettingsWindow.IsProjectSettingsWindow(currentWindow))
            {
                currentWindow.Focus();
            }
        }

        public override void OnDeactivate()
        {
            EditorWindow currentWindow = EditorWindow.focusedWindow;

            SwitchUVCSPlugin.Off(mUVCSPlugin);

            if (SettingsWindow.IsProjectSettingsWindow(currentWindow))
            {
                currentWindow.Focus();
            }
        }

        internal static UVCSProjectSettingsProvider GetIfActive()
        {
            return VersionControlManager.activeVersionControlObject as UVCSProjectSettingsProvider;
        }

        internal void ReloadSettings()
        {
            if (mUVCSPlugin == null)
                return;

            mIsUVCSPluginEnabled = UVCSPluginIsEnabledPreference.IsEnabled();
            mShowToolbarButton = UVCSToolbarButtonIsShownPreference.IsEnabled();

            mWkInfo = FindWorkspace.InfoForApplicationPath(
                ApplicationDataPath.Get(), PlasticGui.Plastic.API);

            if (mWkInfo == null)
                return;

            OpenAllFoldouts();
            ActivateFoldouts();

            EditorUtility.SetDirty(this);
        }

        internal void OpenAllFoldouts()
        {
            mIsPendingChangesFoldoutOpen = true;
            mIsDiffAndMergeFoldoutOpen = true;
            mIsShelveAndSwitchFoldoutOpen = true;
            mIsOtherFoldoutOpen = true;
        }

        internal void OpenDiffAndMergeFoldout()
        {
            mIsDiffAndMergeFoldoutOpen = true;
            mIsPendingChangesFoldoutOpen = false;
            mIsShelveAndSwitchFoldoutOpen = false;
            mIsOtherFoldoutOpen = false;
        }

        internal void OpenShelveAndSwitchFoldout()
        {
            mIsShelveAndSwitchFoldoutOpen = true;
            mIsPendingChangesFoldoutOpen = false;
            mIsDiffAndMergeFoldoutOpen = false;
            mIsOtherFoldoutOpen = false;
        }

        internal void OpenOtherFoldout()
        {
            mIsOtherFoldoutOpen = true;
            mIsPendingChangesFoldoutOpen = false;
            mIsDiffAndMergeFoldoutOpen = false;
            mIsShelveAndSwitchFoldoutOpen = false;
        }

        internal void ShowToolbarButtonCheckboxRaiseCheckedForTesting()
        {
            mShowToolbarButton = true;
        }

        internal void ShowToolbarButtonCheckboxRaiseUncheckedForTesting()
        {
            mShowToolbarButton = false;
        }

        void ISettingsInspectorExtension.OnInspectorGUI()
        {
            GUILayout.Space(10);

            DrawSettingsSection(DoIsEnabledSetting);

            if (!mIsUVCSPluginEnabled)
                return;

#if !UNITY_6000_3_OR_NEWER
            DrawSettingsSection(DoShowToolbarButtonSetting);
#endif

            if (mWkInfo == null)
                return;

            mIsPendingChangesFoldoutOpen = DrawFoldout(
                mIsPendingChangesFoldoutOpen,
                PlasticLocalization.Name.PendingChangesOptionsSectionTitle.GetString(),
                mPendingChangesOptionsFoldout.OnGUI);

            mIsDiffAndMergeFoldoutOpen = DrawFoldout(
                mIsDiffAndMergeFoldoutOpen,
                PlasticLocalization.Name.DiffAndMergeOptionsSectionTitle.GetString(),
                mDiffAndMergeOptionsFoldout.OnGUI);

            mIsShelveAndSwitchFoldoutOpen = DrawFoldout(
                mIsShelveAndSwitchFoldoutOpen,
                PlasticLocalization.Name.ShelveAndSwitchOptionsSectionTitle.GetString(),
                mShelveAndSwitchOptionsFoldout.OnGUI);

            mIsOtherFoldoutOpen = DrawFoldout(
                mIsOtherFoldoutOpen,
                PlasticLocalization.Name.OtherOptionsSectionTitle.GetString(),
                mOtherOptionsFoldout.OnGUI);
        }

        void ActivateFoldouts()
        {
            mPendingChangesOptionsFoldout.OnActivate(mWkInfo);
            mDiffAndMergeOptionsFoldout.OnActivate();
            mShelveAndSwitchOptionsFoldout.OnActivate();
            mOtherOptionsFoldout.OnActivate();
        }

        void DoIsEnabledSetting()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string message = PlasticLocalization.GetString(
                    mIsUVCSPluginEnabled ?
                        PlasticLocalization.Name.UnityVCSIsActive :
                        PlasticLocalization.Name.UnityVCSIsPaused);

                GUILayout.Label(
                    message,
                    EditorStyles.boldLabel,
                    GUILayout.Height(20));

                EditorGUILayout.Space(8);

                DoIsEnabledButton();

                GUILayout.FlexibleSpace();
            }
        }

        void DoIsEnabledButton()
        {
            if (!GUILayout.Button(PlasticLocalization.GetString(
                        mIsUVCSPluginEnabled ?
                            PlasticLocalization.Name.PauseButton :
                            PlasticLocalization.Name.ResumeButton),
                    UnityStyles.ProjectSettings.ToggleOn))
            {
                return;
            }

            EditorWindow currentWindow = EditorWindow.focusedWindow;

            if (mIsUVCSPluginEnabled)
            {
                mIsUVCSPluginEnabled = false;

                TrackFeatureEvent(
                    mWkInfo,
                    TrackFeatureUseEvent.Features.UnityPackage.DisableManually);

                SwitchUVCSPlugin.Off(mUVCSPlugin);
            }
            else
            {
                mIsUVCSPluginEnabled = true;

                TrackFeatureEvent(
                    mWkInfo,
                    TrackFeatureUseEvent.Features.UnityPackage.EnableManually);

                SwitchUVCSPlugin.On(mUVCSPlugin);
            }

            currentWindow.Focus();
        }

        void DoShowToolbarButtonSetting()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                bool wasChecked = mShowToolbarButton;

                bool isChecked = EditorGUILayout.Toggle(Styles.ShowToolbarButton, wasChecked);

                if (!wasChecked && isChecked)
                {
                    mShowToolbarButton = true;
                    mUpdateToolbarButtonVisibility.Show();
                    return;
                }

                if (wasChecked && !isChecked)
                {
                    mShowToolbarButton = false;
                    mUpdateToolbarButtonVisibility.Hide();
                    return;
                }
            }
        }

        static void DrawSettingsSection(Action drawSettings)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;

            try
            {
                EditorGUIUtility.labelWidth = UnityConstants.SETTINGS_GUI_WIDTH_MAIN_SECTION;

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.Space(10);

                        drawSettings();

                        GUILayout.Space(10);
                    }
                }
            }
            finally
            {
                EditorGUIUtility.labelWidth = originalLabelWidth;
            }
        }

        static bool DrawFoldout(
            bool isFoldoutOpen,
            string title,
            Action drawContent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            bool result =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    isFoldoutOpen,
                    title,
                    UnityStyles.ProjectSettings.FoldoutHeader);

            if (result)
                drawContent();

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();

            return result;
        }

        static void TrackFeatureEvent(WorkspaceInfo wkInfo, string eventName)
        {
            if (wkInfo == null)
                return;

            TrackFeatureUseEvent.For(
                PlasticGui.Plastic.API.GetRepositorySpec(wkInfo),
                eventName);
        }

        class Styles
        {
            internal static GUIContent ShowToolbarButton =
                new GUIContent(
                    PlasticLocalization.Name.ShowUnityVersionControlToolbarButton.GetString());
        }

        bool mIsPendingChangesFoldoutOpen;
        bool mIsDiffAndMergeFoldoutOpen;
        bool mIsShelveAndSwitchFoldoutOpen;
        bool mIsOtherFoldoutOpen;
        bool mShowToolbarButton;
        bool mIsUVCSPluginEnabled;

        WorkspaceInfo mWkInfo;

        PendingChangesOptionsFoldout mPendingChangesOptionsFoldout = new PendingChangesOptionsFoldout();
        DiffAndMergeOptionsFoldout mDiffAndMergeOptionsFoldout = new DiffAndMergeOptionsFoldout();
        ShelveAndSwitchOptionsFoldout mShelveAndSwitchOptionsFoldout = new ShelveAndSwitchOptionsFoldout();
        OtherOptionsFoldout mOtherOptionsFoldout = new OtherOptionsFoldout();

        IUpdateToolbarButtonVisibility mUpdateToolbarButtonVisibility;
        UVCSPlugin mUVCSPlugin;
    }
}
