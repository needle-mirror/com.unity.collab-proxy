using System.Collections.Generic;

using Codice.CM.Client.Differences;
using Codice.CM.Client.Differences.Graphic;

using XDiffGui;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class DiffCalculatorSync
    {
        public bool IsDiffCalculationInProgress()
        {
            lock (mSyncLock)
                return mCalculator != null;
        }

        public bool TrySetCurrentDiffCalculation(DiffCalculator calculator)
        {
            lock (mSyncLock)
            {
                if (mCalculator != null)
                    return false;

                mCalculator = calculator;
                return true;
            }
        }

        public void ReplaceCurrentDiffCalculation(DiffCalculator calculator)
        {
            lock (mSyncLock)
            {
                if (mCalculator != null)
                    mCalculator.Cancel();

                mCalculator = calculator;
            }
        }

        public void CancelCurrentDiffCalculation()
        {
            lock (mSyncLock)
            {
                if (mCalculator == null)
                    return;

                mCalculator.Cancel();
                mCalculator = null;
            }
        }

        public bool IsCurrentDiffCalculation(DiffCalculator calculator)
        {
            if (calculator.IsCancelled())
                return false;

            lock (mSyncLock)
            {
                return mCalculator == calculator;
            }
        }

        public void CleanCurrentDiffCalculation()
        {
            lock (mSyncLock)
                mCalculator = null;
        }

        object mSyncLock = new object();
        DiffCalculator mCalculator;
    }

    internal class DiffCalculator
    {
        internal DiffCalculator(
            DiffViewerData diffInfo,
            MovedDetectionOptionsInfo movedDetectionOptions,
            ComparisonMethodTypes comparisonMethod,
            IDiffContent rightEditableContent,
            IDiffContentLoader contentLoader)
        {
            mDiffInfo = diffInfo;
            mMovedDetectionOptions = movedDetectionOptions;
            mComparisonMethod = comparisonMethod;
            mRightEditableContent = rightEditableContent;
            mContentLoader = contentLoader;
        }

        internal void CalculateDiff()
        {
            SetResult(null);

            if (IsCancelled())
                return;

            Result diffResult = new Result();
            CalculateDiff(diffResult);

            if (IsCancelled())
                return;

            SetResult(diffResult);
        }

        internal bool IsCancelled()
        {
            lock (mSyncLock)
            {
                return mbIsCancelled;
            }
        }

        internal Result GetResult()
        {
            lock (mSyncLock)
            {
                return mResult ?? new Result();
            }
        }

        internal void Cancel()
        {
            lock (mSyncLock)
            {
                mbIsCancelled = true;
            }
        }

        void SetResult(Result result)
        {
            lock (mSyncLock)
            {
                mResult = result;
            }
        }

        void CalculateDiff(Result diffResult)
        {
            diffResult.LeftContent = DiffContent.Build(
                mContentLoader.LoadLeftContent(mDiffInfo.Left.Encoding));

            if (IsCancelled())
                return;

            IDiffContent rightContent = mRightEditableContent;

            if (rightContent == null)
            {
                diffResult.RightContent = DiffContent.Build(
                    mContentLoader.LoadRightContent(mDiffInfo.Right.Encoding));

                rightContent = diffResult.RightContent;
            }

            if (IsCancelled())
                return;

            CalculateTextBasedDiff(
                diffResult.LeftContent, rightContent, diffResult);
        }

        void CalculateTextBasedDiff(
            IDiffContent leftContent, IDiffContent rightContent, Result diffResult)
        {
            DiffComparableContent leftComparable = new DiffComparableContent(
                leftContent, mComparisonMethod);

            DiffComparableContent rightComparable = new DiffComparableContent(
                rightContent, mComparisonMethod);

            DiffCollection differences = TextDiffInfoCalculator.CalculateDifferences(
               leftComparable, rightComparable, mComparisonMethod);

            if (IsCancelled())
                return;

            List<DifferencesBlock> differencesBlock =
                TextDiffInfoCalculator.GenerateDifferencesBlock(differences);

            if (IsCancelled())
                return;

            ColorConfiguration colorConfig = ColorConfiguration.Value;

            diffResult.DiffDrawingInfo = TextDiffInfoCalculator.GenerateDrawingInfo(
                differencesBlock, leftComparable, rightComparable,
                colorConfig.BaseColor, colorConfig.SourceColor);

            diffResult.DiffDrawingInfo.IsSemanticDiff = false;

            if (IsCancelled())
                return;

            diffResult.DiffDrawingInfo.MoveInfo =
                MoveInfoCalculator.GenerateMoveRegions(
                    differences, differencesBlock,
                    leftComparable, rightComparable,
                    mMovedDetectionOptions, mComparisonMethod,
                    colorConfig.BaseColor, colorConfig.SourceColor);

            if (IsCancelled())
                return;

            diffResult.DiffDrawingInfo.MoveInfo.XDiffIconsData = new XDiffIconsData(
                mDiffInfo.Left.SymbolicName, mDiffInfo.Right.SymbolicName,
                mDiffInfo.Extension, mComparisonMethod);
        }

        object mSyncLock = new object();
        bool mbIsCancelled;
        Result mResult;

        readonly DiffViewerData mDiffInfo;
        readonly MovedDetectionOptionsInfo mMovedDetectionOptions;
        readonly ComparisonMethodTypes mComparisonMethod;
        readonly IDiffContent mRightEditableContent;
        readonly IDiffContentLoader mContentLoader;

        internal class Result
        {
            internal DiffDrawingInfo DiffDrawingInfo;
            internal DiffContent LeftContent;
            internal DiffContent RightContent;
        }
    }
}
