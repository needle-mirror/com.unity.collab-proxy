using System;

#if LIFECYCLE_APIS_AVAILABLE
using Unity.Scripting.LifecycleManagement;
#endif

using Codice.Client.Common.Threading;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor
{
    // Partial class required by the source generator to use [OnCodeLoaded] and [OnCodeUnloading] attributes
    internal static partial class AppDomainHandler
    {
        internal static void RegisterExceptionHandlers()
        {
#if !LIFECYCLE_APIS_AVAILABLE
            RegisterExceptionHandlersInternal();
#endif
        }

        internal static void UnRegisterExceptionHandlers()
        {
#if !LIFECYCLE_APIS_AVAILABLE
            UnRegisterExceptionHandlersInternal();
#endif
        }

#if LIFECYCLE_APIS_AVAILABLE
        [OnCodeLoaded]
#endif
        static void RegisterExceptionHandlersInternal()
        {
// Keeping UAC0006 as this event is properly managed in the lifecycle.
// We will continue to utilize AppDomain.CurrentDomain.UnhandledException,
// as the API itself is not the core issue.
// The key requirement is to guarantee that we consistently unsubscribe from
// this event before the ALC unloads to prevent memory leaks.
#pragma warning disable UAC0006
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
#pragma warning restore UAC0006
        }

#if LIFECYCLE_APIS_AVAILABLE
        [OnCodeUnloading]
#endif
        static void UnRegisterExceptionHandlersInternal()
        {
#pragma warning disable UAC0006
            AppDomain.CurrentDomain.UnhandledException -= HandleUnhandledException;
#pragma warning restore UAC0006
        }

        static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception ex = (Exception)args.ExceptionObject;

            if (CheckUnityException.IsExitGUIException(ex) || !StackTraceChecker.IsPlasticStackTrace(ex.StackTrace))
                throw ex;

            GUIActionRunner.RunGUIAction(delegate {
                ExceptionsHandler.HandleException("HandleUnhandledException", ex);
            });
        }
    }
}
