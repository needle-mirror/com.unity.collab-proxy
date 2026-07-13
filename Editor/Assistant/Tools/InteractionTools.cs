#if AIA_PRESENT
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.AI.Assistant.FunctionCalling;
using Unity.PlasticSCM.Editor.Assistant.UI.Interactions;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    static class InteractionTools
    {
        internal static async Task<string> SelectFromList(
            ToolExecutionContext context,
            string title,
            string message,
            string label,
            List<string> choices)
        {
            var interaction = new DropDownInteraction(
                title,
                message,
                label,
                "Select",
                choices);

            try
            {
                return await context.Interactions.WaitForUser(interaction);
            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException("User cancelled the selection.");
            }
        }

        internal static async Task<string> SelectOption(
            ToolExecutionContext context,
            string title,
            string message,
            List<OptionChoice> options)
        {
            var interaction = new RadioGroupInteraction(
                title,
                message,
                options);

            try
            {
                return await context.Interactions.WaitForUser(interaction);
            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException("User cancelled the selection.");
            }
        }

        internal static async Task<bool> AskForConfirmation(
            ToolExecutionContext context,
            string action,
            string detail)
        {
            var interaction = new ConfirmationInteraction(
                action,
                detail,
                "Confirm",
                "Cancel");

            var approved = await context.Interactions.WaitForUser(interaction);

            if (!approved)
                throw new OperationCanceledException("User cancelled the action.");

            return true;
        }
    }
}
#endif
