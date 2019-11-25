using UnityEditor;
using UnityEngine;

using Unity.Cloud.Collaborate.UserInterface;

namespace CollabProxy.UI
{
    [InitializeOnLoad]
    public class Bootstrap
    {
        static Bootstrap()
        {
            Toolbar.AddSubToolbar(new ToolbarButton { Width = 32f });
        }
    }
}
