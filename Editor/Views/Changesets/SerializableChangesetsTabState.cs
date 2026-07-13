using System;

namespace Unity.PlasticSCM.Editor.Views.Changesets
{
    [Serializable]
    internal class SerializableChangesetsTabState
    {
        internal bool ShowHiddenChangesets;

        internal bool IsInitialized { get; private set; }

        internal SerializableChangesetsTabState(bool showHiddenChangesets)
        {
            ShowHiddenChangesets = showHiddenChangesets;

            IsInitialized = true;
        }
    }
}
