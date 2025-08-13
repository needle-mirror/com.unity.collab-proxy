using Codice.CM.Common;
using PlasticGui.WorkspaceWindow.Diff;
using Unity.PlasticSCM.Editor.Tool;

namespace Unity.PlasticSCM.Editor.Views
{
    static class LaunchDiffOperations
    {
        internal static void DiffChangeset(
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            LaunchTool.IProcessExecutor processExecutor,
            RepositorySpec repSpec,
            long changesetId,
            bool isGluonMode)
        {
            if (changesetId == -1)
                return;

            string changesetFullSpec = GetChangesetFullSpec(
                repSpec, changesetId);

            LaunchTool.OpenChangesetDiffs(
                showDownloadPlasticExeWindow,
                processExecutor,
                repSpec,
                changesetFullSpec,
                isGluonMode);
        }

        internal static void DiffChangeset(
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            LaunchTool.IProcessExecutor processExecutor,
            RepositorySpec repSpec,
            ChangesetInfo changesetInfo,
            bool isGluonMode)
        {
            if (changesetInfo == null)
                return;

            string changesetFullSpec = GetChangesetFullSpec(
                repSpec, changesetInfo.ChangesetId);

            LaunchTool.OpenChangesetDiffs(
                showDownloadPlasticExeWindow,
                processExecutor,
                repSpec,
                changesetFullSpec,
                isGluonMode);
        }

        internal static void DiffSelectedChangesets(
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            LaunchTool.IProcessExecutor processExecutor,
            RepositorySpec repSpec,
            ChangesetInfo cset1,
            ChangesetInfo cset2,
            bool isGluonMode)
        {
            ChangesetInfo srcChangesetInfo;
            ChangesetInfo dstChangesetInfo;

            GetSrcAndDstChangesets(
                cset1,
                cset2,
                out srcChangesetInfo,
                out dstChangesetInfo);

            string srcChangesetFullSpec = GetChangesetFullSpec(
                repSpec, srcChangesetInfo.ChangesetId);

            string dstChangesetFullSpec = GetChangesetFullSpec(
                repSpec, dstChangesetInfo.ChangesetId);

            LaunchTool.OpenSelectedChangesetsDiffs(
                showDownloadPlasticExeWindow,
                processExecutor,
                repSpec,
                srcChangesetFullSpec,
                dstChangesetFullSpec,
                isGluonMode);
        }

        internal static void DiffBranch(
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            LaunchTool.IProcessExecutor processExecutor,
            RepositorySpec repSpec,
            BranchInfo branchInfo,
            bool isGluonMode)
        {
            if (branchInfo == null)
                return;

            string branchFullSpec = GetBranchFullSpec(
                repSpec, branchInfo);

            LaunchTool.OpenBranchDiffs(
                showDownloadPlasticExeWindow,
                processExecutor,
                repSpec,
                branchFullSpec,
                isGluonMode);
        }

        internal static void DiffBranch(
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            LaunchTool.IProcessExecutor processExecutor,
            RepositorySpec repSpec,
            ChangesetExtendedInfo changesetExtendedInfo,
            bool isGluonMode)
        {
            if (changesetExtendedInfo == null)
                return;

            string branchFullSpec = GetBranchFullSpec(
                repSpec, changesetExtendedInfo);

            LaunchTool.OpenBranchDiffs(
                showDownloadPlasticExeWindow,
                processExecutor,
                repSpec,
                branchFullSpec,
                isGluonMode);
        }

        internal static void DiffSelectedLabels(
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            LaunchTool.IProcessExecutor processExecutor,
            RepositorySpec repSpec,
            MarkerExtendedInfo label1,
            MarkerExtendedInfo label2,
            bool isGluonMode)
        {
            if (DiffWindowParametersBuilder.NeedsToRevertLabelDiffOrder(label1, label2))
            {
                MarkerExtendedInfo tmpLabel = label1;
                label1 = label2;
                label2 = tmpLabel;
            }

            string srcChangesetFullSpec = GetChangesetFullSpec(
                repSpec, label1.Changeset);

            string dstChangesetFullSpec = GetChangesetFullSpec(
                repSpec, label2.Changeset);

            LaunchTool.OpenSelectedChangesetsDiffs(
                showDownloadPlasticExeWindow,
                processExecutor,
                repSpec,
                srcChangesetFullSpec,
                dstChangesetFullSpec,
                isGluonMode);
        }

        static void GetSrcAndDstChangesets(
            ChangesetInfo cset1,
            ChangesetInfo cset2,
            out ChangesetInfo srcChangesetInfo,
            out ChangesetInfo dstChangesetInfo)
        {
            if (cset1.LocalTimeStamp < cset2.LocalTimeStamp)
            {
                srcChangesetInfo = cset1;
                dstChangesetInfo = cset2;
                return;
            }

            srcChangesetInfo = cset2;
            dstChangesetInfo = cset1;
        }

        static string GetChangesetFullSpec(
            RepositorySpec repSpec,
            long changesetId)
        {
            return string.Format("cs:{0}@{1}",
                changesetId, repSpec.ToString());
        }

        static string GetBranchFullSpec(
            RepositorySpec repSpec,
            BranchInfo branchInfo)
        {
            return string.Format("br:{0}@{1}",
                branchInfo.BranchName,
                repSpec.ToString());
        }

        static string GetBranchFullSpec(
            RepositorySpec repSpec,
            ChangesetExtendedInfo changesetExtendedInfo)
        {
            return string.Format("br:{0}@{1}",
                changesetExtendedInfo.BranchName,
                repSpec.ToString());
        }
    }
}
