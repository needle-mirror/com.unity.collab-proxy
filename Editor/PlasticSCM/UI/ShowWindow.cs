﻿using System;

using UnityEditor;

namespace Unity.PlasticSCM.Editor.UI
{
    internal static class ShowWindow
    {
        internal static PlasticWindow Plastic()
        {
            return ShowPlasticWindow(false);
        }

        internal static PlasticWindow PlasticDisablingCollab()
        {
            return ShowPlasticWindow(true);
        }

        static PlasticWindow ShowPlasticWindow(bool disableCollabWhenLoaded)
        {
            PlasticWindow window = EditorWindow.GetWindow<PlasticWindow>(
                UnityConstants.PLASTIC_WINDOW_TITLE,
                true,
                mConsoleWindowType,
                mProjectBrowserType);

            if (disableCollabWhenLoaded)
                window.DisableCollabIfEnabledWhenLoaded();

            window.UpdateWindowIcon(PlasticPlugin.GetNotificationStatus());

            return window;
        }

        static Type mConsoleWindowType = typeof(EditorWindow).
            Assembly.GetType("UnityEditor.ConsoleWindow");
        static Type mProjectBrowserType = typeof(EditorWindow).
            Assembly.GetType("UnityEditor.ProjectBrowser");
    }
}