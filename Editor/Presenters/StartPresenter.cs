using System;
using JetBrains.Annotations;
using Unity.Cloud.Collaborate.Models;
using Unity.Cloud.Collaborate.Models.Enums;
using Unity.Cloud.Collaborate.Views;
using UnityEngine;

namespace Unity.Cloud.Collaborate.Presenters
{
    internal class StartPresenter : IStartPresenter
    {
        [NotNull]
        readonly IStartView m_View;
        [NotNull]
        readonly IStartModel m_Model;

        public StartPresenter([NotNull] IStartView view, [NotNull] IStartModel model)
        {
            m_View = view;
            m_Model = model;
        }

        /// <inheritdoc />
        public void Start()
        {
            OnProjectStatusChanged(m_Model.ProjectStatus);
            m_Model.ProjectStatusChanged += OnProjectStatusChanged;
        }

        /// <inheritdoc />
        public void Stop()
        {
            m_Model.ProjectStatusChanged -= OnProjectStatusChanged;
        }

        void OnProjectStatusChanged(ProjectStatus status)
        {
            switch (status) {
                case ProjectStatus.Bound:
                    m_View.Text = "Welcome to Collab. Please click the button below to start.";
                    m_View.ButtonText = "Start Collab";
                    m_View.SetButtonVisible(true);
                    break;
                case ProjectStatus.Unbound:
                    m_View.Text = "Welcome to Collab. Before starting, please click the button below to set a new or existing Unity Project ID for this project. Return to this window once it is set.";
                    m_View.ButtonText = "Set Project ID";
                    m_View.SetButtonVisible(true);
                    break;
                case ProjectStatus.Loading:
                    m_View.Text = "Loading, please wait...";
                    m_View.ButtonText = "";
                    m_View.SetButtonVisible(false);
                    break;
                case ProjectStatus.Ready:
                    m_View.Text = "";
                    m_View.ButtonText = "";
                    m_View.SetButtonVisible(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, "Unexpected project status.");
            }
        }

        /// <inheritdoc />
        public void RequestStart()
        {
            var status = m_Model.ProjectStatus;
            switch (status) {
                // If project is bound, start collab; otherwise, open the services window.
                case ProjectStatus.Bound:
                    // Turn on collab Service. This is where we do a Genesis request apparently.
                    m_Model.RequestTurnOnService();
                    break;
                case ProjectStatus.Unbound:
                    m_Model.ShowServicePage();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, "Unexpected project status.");
            }
        }
    }
}
