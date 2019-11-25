using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Cloud.Collaborate.Assets;
using Unity.Cloud.Collaborate.Models;
using Unity.Cloud.Collaborate.Views;
using Unity.Cloud.Collaborate.Models.Structures;
using Unity.Cloud.Collaborate.Utilities;
using UnityEngine;

namespace Unity.Cloud.Collaborate.Presenters
{
    internal class HistoryPresenter : IHistoryPresenter
    {
        internal const int pageSize = 10;
        internal const string historyEntrySelectedId = "history-entry-selected";

        int m_MaxPages;

        readonly IHistoryView m_View;
        readonly IHistoryModel m_HistoryModel;

        public HistoryPresenter(IHistoryView view, IHistoryModel historyModel)
        {
            m_View = view;
            m_HistoryModel = historyModel;

            m_HistoryModel.HistoryListUpdated += OnHistoryListUpdated;
            m_HistoryModel.SelectedRevisionReceived += OnSelectedRevisionReceived;
            m_HistoryModel.EntryCountUpdated += OnEntryCountUpdated;
            m_HistoryModel.HistoryListReceived += OnHistoryListReceived;
            m_HistoryModel.BusyStatusUpdated += OnBusyStatusUpdated;
        }

        /// <inheritdoc />
        public void Start()
        {
            m_View.SetBusyStatus(m_HistoryModel.Busy);

            // Request initial data
            if (string.IsNullOrEmpty(m_HistoryModel.SavedRevisionId))
            {
                m_HistoryModel.RequestPageOfRevisions(pageSize);
            }
            else
            {
                m_HistoryModel.RequestSingleRevision(m_HistoryModel.SavedRevisionId);
            }
            m_HistoryModel.RequestEntryNumber();
        }

        /// <inheritdoc />
        public void Stop()
        {
            GlobalEvents.UnregisterBackNavigation(historyEntrySelectedId);
        }

        /// <summary>
        /// Event handler to receive updated busy status.
        /// </summary>
        /// <param name="busy">New busy status.</param>
        void OnBusyStatusUpdated(bool busy)
        {
            m_View.SetBusyStatus(busy);
        }

        /// <summary>
        /// Event handler to receive requested history list.
        /// </summary>
        /// <param name="list">Received history list.</param>
        void OnHistoryListReceived(IReadOnlyList<IHistoryEntry> list)
        {
            if (list == null)
            {
                // Return back to first page of entries
                m_HistoryModel.PageNumber = 0;
                m_HistoryModel.RequestPageOfRevisions(pageSize);
                Debug.LogError("Request page does not exist.");
                return;
            }

            GlobalEvents.UnregisterBackNavigation(historyEntrySelectedId);
            m_View.SetHistoryList(list);
        }

        /// <summary>
        /// Event handler to receive updated history entry count.
        /// </summary>
        /// <param name="count">New entry count.</param>
        void OnEntryCountUpdated(int? count)
        {
            if (count == null)
            {
                Debug.LogError("Unable to fetch number of revisions");
                return;
            }

            m_MaxPages = (count.Value - 1) / pageSize;
            m_View.SetPage(m_HistoryModel.PageNumber, m_MaxPages);
        }

        /// <summary>
        /// Event handler to receive requested single revision.
        /// </summary>
        /// <param name="entry">Received single revision.</param>
        void OnSelectedRevisionReceived(IHistoryEntry entry)
        {
            if (entry == null)
            {
                // Return back to all revisions list
                m_HistoryModel.RequestPageOfRevisions(pageSize);
                Debug.LogError("Unable to find requested revision");
                return;
            }

            GlobalEvents.RegisterBackNavigation(historyEntrySelectedId, StringAssets.allHistory, OnBackEvent);
            m_View.SetSelection(entry);
        }

        /// <summary>
        /// Event handler for when the model has received a message of an updated history list.
        /// </summary>
        void OnHistoryListUpdated()
        {
            // Request updated number of entries.
            m_HistoryModel.RequestEntryNumber();
            // Request either single revision or list of revisions depending on current state.
            if (m_HistoryModel.IsRevisionSelected)
            {
                Assert.AreNotEqual(string.Empty, m_HistoryModel.SelectedRevisionId, "There should be a revision id at this point.");
                m_HistoryModel.RequestSingleRevision(m_HistoryModel.SelectedRevisionId);
            }
            else
            {
                m_HistoryModel.RequestPageOfRevisions(pageSize);
            }
        }

        /// <summary>
        /// Event handler for when the back button is pressed.
        /// </summary>
        void OnBackEvent()
        {
            // Return back to all revisions list
            m_HistoryModel.RequestPageOfRevisions(pageSize);
        }

        /// <inheritdoc />
        public void PrevPage()
        {
            m_HistoryModel.PageNumber = Math.Max(m_HistoryModel.PageNumber - 1, 0);
            m_HistoryModel.RequestPageOfRevisions(pageSize);
            m_View.SetPage(m_HistoryModel.PageNumber, m_MaxPages);
        }

        /// <inheritdoc />
        public void NextPage()
        {
            m_HistoryModel.PageNumber = Math.Min(m_HistoryModel.PageNumber + 1, m_MaxPages);
            m_HistoryModel.RequestPageOfRevisions(pageSize);
            m_View.SetPage(m_HistoryModel.PageNumber, m_MaxPages);
        }

        /// <inheritdoc />
        public string SelectedRevisionId
        {
            set
            {
                if (m_HistoryModel.SelectedRevisionId == value) return;
                m_HistoryModel.RequestSingleRevision(value);
            }
        }

        /// <inheritdoc />
        public void RequestGoto(string revisionId, HistoryEntryStatus status)
        {
            switch (status) {
                case HistoryEntryStatus.Ahead:
                    m_HistoryModel.RequestUpdateTo(revisionId);
                    break;
                case HistoryEntryStatus.Current:
                    m_HistoryModel.RequestRestoreTo(revisionId);
                    break;
                case HistoryEntryStatus.Behind:
                    m_HistoryModel.RequestGoBackTo(revisionId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        /// <inheritdoc />
        public bool SupportsRevert => m_HistoryModel.SupportsRevert;

        /// <inheritdoc />
        public void RequestRevert(string revisionId, IReadOnlyList<string> files)
        {
            m_HistoryModel.RequestRevert(revisionId, files);
        }
    }
}
