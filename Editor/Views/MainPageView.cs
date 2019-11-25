using System;
using JetBrains.Annotations;
using NUnit.Framework;
using Unity.Cloud.Collaborate.Assets;
using Unity.Cloud.Collaborate.Components;
using Unity.Cloud.Collaborate.Presenters;
using Unity.Cloud.Collaborate.UserInterface;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Cloud.Collaborate.Views
{
    internal class MainPageView : PageComponent, IMainView
    {
        IMainPresenter m_Presenter;

        public const string UssClassName = "main-page-view";
        public const string AlertBoxUssClassName = UssClassName + "__alert-box";
        public const string TabViewUssClassName = UssClassName + "__tab-view";
        public const string ContainerUssClassName = UssClassName + "__container";

        static readonly string k_LayoutPath = $"{CollaborateWindow.LayoutPath}/{nameof(MainPageView)}.uxml";
        static readonly string k_StylePath = $"{CollaborateWindow.StylePath}/{nameof(MainPageView)}.uss";

        readonly AlertBox m_AlertBox;
        readonly TabView m_TabView;
        readonly HistoryTabPageView m_HistoryView;
        readonly ChangesTabPageView m_ChangesView;
        readonly VisualElement m_Container;
        ProgressView m_ProgressView;

        public MainPageView()
        {
            AddToClassList(UssClassName);
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_LayoutPath).CloneTree(this);
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StylePath));

            m_AlertBox = this.Q<AlertBox>(className: AlertBoxUssClassName);
            m_TabView = this.Q<TabView>(className: TabViewUssClassName);
            m_Container = this.Q<VisualElement>(className: ContainerUssClassName);

            m_ChangesView = new ChangesTabPageView();
            m_HistoryView = new HistoryTabPageView();
            m_TabView.AddTab(StringAssets.changes, m_ChangesView);
            m_TabView.AddTab(StringAssets.history, m_HistoryView);
            m_TabView.Init();
        }

        /// <inheritdoc />
        public IMainPresenter Presenter
        {
            set
            {
                m_Presenter = value;
                m_Presenter.AssignHistoryPresenter(m_HistoryView);
                m_Presenter.AssignHistoryPresenter(m_ChangesView);
                // If page active before presenter has been added, call start once we have it.
                if (Active)
                {
                    m_Presenter.Start();
                }
            }
        }

        /// <inheritdoc />
        protected override void SetActive()
        {
            m_Presenter?.Start();
        }

        /// <inheritdoc />
        protected override void SetInactive()
        {
            m_Presenter?.Stop();
        }

        /// <inheritdoc />
        public void AddAlert(string id, AlertBox.AlertLevel level, string message, (string text, Action action)? button = null)
        {
            m_AlertBox.QueueAlert(id, level, message, button);
        }

        /// <inheritdoc />
        public void RemoveAlert(string id)
        {
            m_AlertBox.DequeueAlert(id);
        }

        public void AddOperationProgress()
        {
            if (m_ProgressView == null)
            {
                m_ProgressView = new ProgressView();
                m_ProgressView.SetCancelCallback(m_Presenter.RequestCancelJob);
                m_Container.Add(m_ProgressView);
            }
            m_ProgressView.RemoveFromClassList(UiConstants.ussHidden);
            m_TabView.AddToClassList(UiConstants.ussHidden);
        }

        public void RemoveOperationProgress()
        {
            m_ProgressView?.AddToClassList(UiConstants.ussHidden);

            m_TabView.RemoveFromClassList(UiConstants.ussHidden);
        }

        public void SetOperationProgress(string title, string details, int percentage, int completed, int total, bool isPercentage, bool canCancel)
        {
            Assert.IsNotNull(m_ProgressView);
            if (m_ProgressView == null) return;
            var progress = isPercentage ? $"{percentage}%" : $"({completed} of {total})";
            m_ProgressView.SetText($"{title}\n\n{details}", progress);
            m_ProgressView.SetPercentComplete(percentage);
            m_ProgressView.SetCancelButtonActive(canCancel);
        }

        [UsedImplicitly]
        public new class UxmlFactory : UxmlFactory<MainPageView> { }
    }
}
