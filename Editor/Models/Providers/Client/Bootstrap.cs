#define COLLAB_V2

using UnityEditor;
using UnityEngine;

#if COLLAB_V2
namespace Unity.Cloud.Collaborate.Models.Providers.Client
{

    /// <summary>
    /// The static constructor of this class is called immediately on load, we use this to register our version
    /// control provider with Unity
    /// </summary>
    [InitializeOnLoad]
    internal class Bootstrap
    {
        public static readonly CollabVersionControl VersionControl;
        static Bootstrap()
        {
            VersionControl = new CollabVersionControl();

            UnityEditor.Collaboration.Collab.SetVersionControl(VersionControl);
        }
    }
}
#endif
