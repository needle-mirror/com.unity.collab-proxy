using System;
using Codice.CM.Common;

namespace Unity.PlasticSCM.Editor.Views.Branches
{
    [Serializable]
    internal class SerializableBranchesTabState
    {
        internal BranchInfo BranchToSelect;
        internal bool ShowHiddenBranches;

        internal bool IsInitialized { get; private set; }

        internal SerializableBranchesTabState(
            BranchInfo branchToSelect,
            bool showHiddenBranches)
        {
            ShowHiddenBranches = showHiddenBranches;

            IsInitialized = true;
        }
    }
}
