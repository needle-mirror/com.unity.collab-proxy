using System;
using JetBrains.Annotations;
using Unity.Cloud.Collaborate.Components;
using Unity.Cloud.Collaborate.Presenters;

namespace Unity.Cloud.Collaborate.Views
{
    internal interface IMainView : IView<IMainPresenter>
    {
        /// <summary>
        /// Add or update an alert with the provided id.
        /// </summary>
        /// <param name="id">Id of the alert to add or update.</param>
        /// <param name="level">Level of severity.</param>
        /// <param name="message">Message for the alert.</param>
        /// <param name="button">Optional button with text and a callback.</param>
        void AddAlert([NotNull] string id, AlertBox.AlertLevel level, [NotNull] string message, (string text, Action action)? button = null);

        /// <summary>
        /// Remove alert with the provided id.
        /// </summary>
        /// <param name="id">Id of the alert to remove.</param>
        void RemoveAlert([NotNull] string id);

        void AddOperationProgress();
        void RemoveOperationProgress();
        void SetOperationProgress(string title, string details, int percentage, int completed, int total, bool isPercentage, bool canCancel);
    }
}
