#if AIA_PRESENT
using System;
using System.Threading.Tasks;
using Unity.AI.Assistant.FunctionCalling;

namespace Unity.PlasticSCM.Editor.Assistant.UI.Interactions
{
    class ConfirmationInteraction : IInteractionSource<bool>, IApprovalInteraction
    {
        public string Action { get; }
        public string Detail { get; }
        public string AllowLabel { get; }
        public string DenyLabel { get; }
        public bool ShowScope => false;

        public event Action<bool> OnCompleted;
        public TaskCompletionSource<bool> TaskCompletionSource { get; } = new();

        public ConfirmationInteraction(
            string action,
            string detail,
            string allowLabel = null,
            string denyLabel = null)
        {
            Action = action;
            Detail = detail;
            AllowLabel = allowLabel;
            DenyLabel = denyLabel;
        }

        public void Respond(PermissionUserAnswer answer)
        {
            var approved = answer == PermissionUserAnswer.AllowOnce
                           || answer == PermissionUserAnswer.AllowAlways;
            TaskCompletionSource.TrySetResult(approved);
            OnCompleted?.Invoke(approved);
        }

        public void CancelInteraction()
        {
            TaskCompletionSource.TrySetCanceled();
        }
    }
}
#endif
