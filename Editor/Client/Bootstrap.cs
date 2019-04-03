using UnityEditor;
using UnityEditor.Collaboration;
using UnityEditor.Web;

namespace CollabProxy.Client
{   
    /// <summary>
    /// The static constructor of this class is called immediately on load, we use this to register our version
    /// control provider with Unity
    /// </summary>
    [InitializeOnLoad]
    internal class Bootstrap
    {
        static Bootstrap()
        {
            CollabVersionControl collabVersionControl = new CollabVersionControl();
            Collab.SetVersionControl(collabVersionControl);
            JSProxyMgr.GetInstance().AddGlobalObject("unity/collab/proxy", new JsProxyClient(collabVersionControl));
        }
    }
}