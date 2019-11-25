using System;
using Unity.Cloud.Collaborate.Models.Api;
using Unity.Cloud.Collaborate.Models.Enums;
using UnityEngine;

namespace Unity.Cloud.Collaborate.Models
{
    internal class StartModel : IStartModel
    {
        ISourceControlProvider m_Provider;

        /// <inheritdoc />
        public event Action<ProjectStatus> ProjectStatusChanged;

        public StartModel(ISourceControlProvider provider)
        {
            m_Provider = provider;
            m_Provider.UpdatedProjectStatus += OnUpdatedProjectStatus;
        }

        /// <inheritdoc />
        public void OnStop()
        {
            m_Provider.UpdatedProjectStatus -= OnUpdatedProjectStatus;
        }

        /// <inheritdoc />
        public ProjectStatus ProjectStatus => m_Provider.GetProjectStatus();

        /// <inheritdoc />
        public void RequestTurnOnService()
        {
            m_Provider.RequestTurnOnService();
        }

        /// <inheritdoc />
        public void ShowServicePage()
        {
            m_Provider.ShowServicePage();
        }

        void OnUpdatedProjectStatus(ProjectStatus status)
        {
            ProjectStatusChanged?.Invoke(status);
        }
    }
}
