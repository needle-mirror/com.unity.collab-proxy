using System;
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

        public bool IsGettingChanges()
        {
            return CollabVersionControl.IsGettingChanges;
        }

        public void StartGetChanges()
        {
            m_CollabVersionControl.StartGetChanges();
        }

        public void StartUpdateCachedChanges()
        {
            m_CollabVersionControl.StartUpdateCachedChanges();
        }

        public void StartUpdateFileStatus(string filePath)
        {
            m_CollabVersionControl.StartUpdateFileStatus(filePath);
        }
    }
}
