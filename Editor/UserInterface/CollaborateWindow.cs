using System;
using Unity.Cloud.Collaborate.Assets;
using Unity.Cloud.Collaborate.Components;
using Unity.Cloud.Collaborate.Components.Menus;
using Unity.Cloud.Collaborate.Models;
using Unity.Cloud.Collaborate.Models.Api;
using Unity.Cloud.Collaborate.Models.Enums;
using Unity.Cloud.Collaborate.Models.Providers;
using Unity.Cloud.Collaborate.Views;
using Unity.Cloud.Collaborate.Presenters;
using Unity.Cloud.Collaborate.Settings;
using Unity.Cloud.Collaborate.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Cloud.Collaborate.UserInterface
{
    internal class CollaborateWindow : EditorWindow
    {
        public const string UssClassName = "main-window";
        public const string ContainerUssClassName = UssClassName + "__container";

        public const string PackagePath = "Packages/com.unity.collab-proxy";
        public const string UserInterfacePath = PackagePath + "/Editor/UserInterface";
        public const string ResourcePath = PackagePath + "/Editor/Assets";
        public const string LayoutPath = ResourcePath + "/Layouts";
        public const string StylePath = ResourcePath + "/Styles";
        public const string IconPath = ResourcePath + "/Icons";
        public const string TestWindowPath = UserInterfacePath + "/TestWindows";

        const string k_LayoutPath = LayoutPath + "/main-window.uxml";
        public const string MainStylePath = StylePath + "/styles.uss";

        MainPageView m_MainView;
        ErrorPageView m_ErrorPageView;
        StartPageView m_StartView;
        VisualElement m_ViewContainer;

        PageComponent m_ActivePage;

        ISourceControlProvider m_Provider;

        [MenuItem("Window/Collaborate")]
        internal static void Init()
        {
            var openLocation = CollabSettingsManager.Get(CollabSettings.settingDefaultOpenLocation, fallback: CollabSettings.OpenLocation.Docked);

            CollaborateWindow window;
            if (openLocation == CollabSettings.OpenLocation.Docked)
            {
                // Dock next to inspector, if available
                var inspectorType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
                window = GetWindow<CollaborateWindow>(inspectorType);
            }
            else
            {
                window = GetWindow<CollaborateWindow>();
            }

            // Set up window
            window.titleContent = new GUIContent("Collaborate");
            window.minSize = new Vector2(256, 400);

            // Display window
            window.Show();
            window.Focus();
        }

        void OnDisable()
        {
            m_Provider.UpdatedProjectStatus -= OnUpdatedProjectStatus;
            GlobalEvents.WindowClose();
            WindowCache.Instance.Serialize();
        }

        void OnEnable()
        {
            var root = rootVisualElement;
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(MainStylePath));

            root.AddToClassList(EditorGUIUtility.isProSkin
                ? UiConstants.ussDark
                : UiConstants.ussLight);

            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_LayoutPath).CloneTree(root);

            m_Provider = new Collab();
            m_Provider.UpdatedProjectStatus += OnUpdatedProjectStatus;

            m_ViewContainer = root.Q<VisualElement>(className: ContainerUssClassName);

            // Get the views and configure them.
            m_MainView = new MainPageView();
            m_MainView.Presenter = new MainPresenter(m_MainView, new MainModel(m_Provider));
            m_StartView = new StartPageView();
            m_StartView.Presenter = new StartPresenter(m_StartView, new StartModel(m_Provider));
            m_ErrorPageView = new ErrorPageView();

            // Add floating dialogue so it can be displayed anywhere in the window.
            root.Add(FloatingDialogue.Instance);

            OnUpdatedProjectStatus(m_Provider.GetProjectStatus());
        }

        void OnUpdatedProjectStatus(ProjectStatus status)
        {
            UpdateDisplayMode(status == ProjectStatus.Ready ? Display.Main : Display.Add);
        }

        void UpdateDisplayMode(Display newDisplay)
        {
            m_ActivePage?.RemoveFromHierarchy();
            m_ActivePage?.SetActive(false);
            m_ViewContainer.Clear();

            // Get new page to display
            switch (newDisplay)
            {
                case Display.Add:
                    m_ActivePage = m_StartView;
                    break;
                case Display.Error:
                    m_ActivePage = m_ErrorPageView;
                    break;
                case Display.Main:
                    m_ActivePage = m_MainView;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            m_ActivePage.SetActive(true);
            m_ViewContainer.Add(m_ActivePage);
        }

        enum Display
        {
            Add,
            Error,
            Main
        }
    }
}
