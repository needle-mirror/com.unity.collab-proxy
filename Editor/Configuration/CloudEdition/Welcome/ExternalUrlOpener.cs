using UnityEngine;

namespace Unity.PlasticSCM.Editor.Configuration.CloudEdition.Welcome
{
    internal interface IExternalUrlOpener
    {
        void Open(string url);
    }

    internal class DefaultExternalUrlOpener : IExternalUrlOpener
    {
        void IExternalUrlOpener.Open(string url)
        {
            Application.OpenURL(url);
        }
    }
}
