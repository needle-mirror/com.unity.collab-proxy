using UnityEditor;
using UnityEditor.Collaboration;

namespace CollabProxy.UI
{
    [InitializeOnLoad]
    internal static class Bootstrap
    {
        private const float kCollabToolbarButtonWidth = 78.0f;
        
        static Bootstrap()
        {
            Collab.ShowHistoryWindow = CollabHistoryWindow.ShowHistoryWindow;
            Collab.ShowToolbarAtPosition = CollabToolbarWindow.ShowCenteredAtPosition;
            Collab.IsToolbarVisible = CollabToolbarWindow.IsVisible;
            Collab.CloseToolbar = CollabToolbarWindow.CloseToolbar;
            Toolbar.AddSubToolbar(new CollabToolbarButton
            {
                Width = kCollabToolbarButtonWidth
            });
        }
    }
}