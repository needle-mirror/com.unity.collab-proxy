using UnityEditor.Collaboration;
using UnityEngine;

namespace CollabProxy.Client
{
    internal class JsProxyClient
    {
        readonly CollabVersionControl m_CollabVersionControl;

        public JsProxyClient(CollabVersionControl collabVersionControl)
        {
            m_CollabVersionControl = collabVersionControl;
        }

        public bool IsJobRunning()
        {
            return m_CollabVersionControl.IsJobRunning;
        }
    }
}