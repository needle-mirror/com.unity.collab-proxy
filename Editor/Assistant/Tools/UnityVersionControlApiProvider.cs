#if AIA_PRESENT
using Unity.PlasticSCM.Editor.Api;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    internal static class UnityVersionControlApiProvider
    {
        static UnityVersionControlApiProvider()
        {
            Instance = new UnityVersionControlApi();
        }

        internal static IUnityVersionControlApi Instance
        {
            get;
            private set;
        }

        internal static void SetForTesting(IUnityVersionControlApi api)
        {
            Instance = api;
        }
    }
}
#endif
