using System.Linq;
using UnityEditor;
using UnityEditor.Collaboration;
using UnityEditor.Web;
using UnityEngine;

namespace CollabProxy.Client
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
            Collab.SetVersionControl(VersionControl);
            JSProxyMgr.GetInstance().AddGlobalObject("unity/collab/proxy", new JsProxyClient(VersionControl));
        }
    }
}
