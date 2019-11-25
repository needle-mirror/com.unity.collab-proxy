using System;

namespace Unity.Cloud.Collaborate.Utilities
{
    internal static class GlobalEvents
    {
        /// <summary>
        /// Register back navigation to be made available to the user to navigate backwards in the UI.
        /// </summary>
        public static event Action<string, string, Action> RegisteredBackNavigation = delegate { };

        /// <summary>
        /// Unregister back navigation if the given id matches the currently displayed back navigation.
        /// </summary>
        public static event Func<string, bool> UnregisteredBackNavigation = delegate { return true; };

        /// <summary>
        /// Event called when the window is being closed.
        /// </summary>
        public static event Action WindowClosed = delegate { };

        /// <summary>
        /// Register back navigation to be made available to the user to navigate backwards in the UI.
        /// </summary>
        /// <param name="id">Id for the back event.</param>
        /// <param name="text">Text for the back label.</param>
        /// <param name="backEvent">Action to perform to go back.</param>
        public static void RegisterBackNavigation(string id, string text, Action backEvent)
        {
            RegisteredBackNavigation(id, text, backEvent);
        }

        /// <summary>
        /// Unregister back navigation if the given id matches the currently displayed back navigation.
        /// </summary>
        /// <param name="id">Id for the back event.</param>
        /// <returns>True if id matched.</returns>
        public static bool UnregisterBackNavigation(string id)
        {
            return UnregisteredBackNavigation(id);
        }

        public static void WindowClose()
        {
            WindowClosed();
        }
    }
}
