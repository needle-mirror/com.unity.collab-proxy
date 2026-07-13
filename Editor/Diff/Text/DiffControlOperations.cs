using System;
using System.IO;
using System.Text;
using Codice.Client.Common.Threading;
using Codice.CM.Client.Differences;
using Codice.CM.Client.Differences.Graphic;
using Codice.LogWrapper;
using MergetoolGui;
using XDiffGui;
using XDiffGui.Options;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal interface IRunAfterSetDifferencesInfo
    {
        // This needs to run *always after* IDiffControl.SetDifferencesInfo
        void Run();
    }

    internal static class DiffMessage
    {
        public static string Get(
            string initialMessage, bool bHasDifferences)
        {
            if (bHasDifferences)
                return initialMessage;

            string filesIdenticalMessage = MergetoolLocalization.GetString(
                MergetoolLocalization.Name.FilesIdentical);

            if (string.IsNullOrEmpty(initialMessage))
                return filesIdenticalMessage;

            return string.Format("{0} {1}", filesIdenticalMessage, initialMessage);
        }
    }

    internal static class DiffControlOperations
    {
        public interface IDiffControl
        {
            void ShowDiffPanel();
            void ShowWaitingAnimation();
            void HideWaitingAnimation();

            void ClearNavigationPanel();

            void SetOptionsContextMenuButtonInfo(
                ComparisonMethodTypes comparisonMethod,
                Language syntaxHighlightLanguage,
                DiffViewerData diffInfo);

            void UpdateTextContent(
                DiffContent leftContent, DiffContent rightContent);
            void SetSyntaxLanguage(Language language);
            Language GetSyntaxLanguage();
            void SetEditable(bool editable);
            void OnRightTextBoxClean();

            void SetDiffDrawingInfo(DiffDrawingInfo diffDrawingInfo);
            void SetDifferencesInfo(
                DiffDrawingInfo diffDrawingInfo,
                DiffViewerData diffData);
            void UpdateDifferencesInfoSilently(DiffDrawingInfo diffDrawingInfo);

            void HandleException(Exception e);

            void EnableBigFileMessagePanel();
            void DisableBigFileMessagePanel();
        }

        internal static void AsyncCalculateDiff(
            IDiffControl diffControl,
            DiffViewerData diffViewerData,
            MovedDetectionOptionsInfo movedDetectionOptions,
            ComparisonMethodTypes comparisonMethod,
            IDiffContent rightContentForEdition,
            IDiffContentLoader contentLoader,
            DiffCalculatorSync syncCalculator)
        {
            diffControl.ShowDiffPanel();

            diffControl.OnRightTextBoxClean();

            diffControl.ClearNavigationPanel();

            DiffCalculator calculator = BuildDiffCalculator(
                diffViewerData,
                movedDetectionOptions,
                comparisonMethod,
                rightContentForEdition,
                contentLoader);

            syncCalculator.ReplaceCurrentDiffCalculation(calculator);

            diffControl.ShowWaitingAnimation();

            Language language = SyntaxLanguageFactory.GetSyntaxLanguage(
                FileSizeCalculator.GetMaxFileSize(
                    diffViewerData.Left.File, diffViewerData.Right.File),
                Path.GetExtension(diffViewerData.Left.File));

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(TIMER_INTERVAL);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    mLog.Info(
                        "AsyncCalculateDiff thread about to CalculateDiff");

                    calculator.CalculateDiff();
                },
                /*afterOperationDelegate*/ delegate
                {
                    diffControl.HideWaitingAnimation();

                    DiffCalculator.Result result = calculator.GetResult();
                    diffControl.SetDiffDrawingInfo(result.DiffDrawingInfo);

                    diffControl.SetOptionsContextMenuButtonInfo(
                        comparisonMethod, language, diffViewerData);

                    if (language != diffControl.GetSyntaxLanguage())
                        diffControl.SetSyntaxLanguage(Language.PlainText);

                    diffControl.UpdateTextContent(
                        result.LeftContent, result.RightContent);

                    bool isEditable = diffViewerData.IsEditable;
                    diffControl.SetEditable(isEditable);

                    diffControl.SetSyntaxLanguage(language);

                    if (waiter.Exception != null)
                    {
                        syncCalculator.CleanCurrentDiffCalculation();

                        diffControl.HandleException(waiter.Exception);
                        return;
                    }

                    diffControl.SetDifferencesInfo(
                        result.DiffDrawingInfo,
                        diffViewerData);

                    syncCalculator.CleanCurrentDiffCalculation();

                    /*if (runAfterSetDifferencesInfo != null)
                        runAfterSetDifferencesInfo.Run();*/

                    /*TrackDiffEvent.FileDiff(
                        diffViewerData, isSemanticDiff && useSemanticDiff, result);*/
                });
        }

        internal static void ComparisonMethodChanged(
            ComparisonMethodTypes comparisonMethod,
            IDiffControl diffControl,
            DiffViewerData diffViewerData,
            MovedDetectionOptionsInfo movedDetectionOptions,
            IDiffContent rightContentForEdition,
            IDiffContentLoader contentLoader,
            DiffCalculatorSync syncCalculator)
        {
            DiffCalculator calculator = BuildDiffCalculator(
                diffViewerData,
                movedDetectionOptions,
                comparisonMethod,
                rightContentForEdition,
                contentLoader);

            if (!syncCalculator.TrySetCurrentDiffCalculation(calculator))
                return;

            diffControl.ShowWaitingAnimation();

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(TIMER_INTERVAL);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    mLog.Info("OnComparisonMethodChanged thread about to " +
                        "recalculate diff with a different comparison method");

                    calculator.CalculateDiff();
                },
                /*afterOperationDelegate*/ delegate
                {
                    if (!syncCalculator.IsCurrentDiffCalculation(calculator))
                        return;

                    diffControl.HideWaitingAnimation();

                    if (waiter.Exception != null)
                    {
                        syncCalculator.CleanCurrentDiffCalculation();

                        diffControl.HandleException(waiter.Exception);
                        return;
                    }

                    DiffCalculator.Result result = calculator.GetResult();
                    diffControl.SetDiffDrawingInfo(result.DiffDrawingInfo);

                    diffControl.SetDifferencesInfo(
                        result.DiffDrawingInfo,
                        diffViewerData);

                    syncCalculator.CleanCurrentDiffCalculation();
                });
        }

        internal static void CalculateDifferencesButtonClick(
            IBigFileDownloader bigFileDownloader,
            IDiffControl diffControl,
            DiffViewerData diffViewerData,
            MovedDetectionOptionsInfo movedDetectionOptions,
            ComparisonMethodTypes comparisonMethod,
            IDiffContent rightContentForEdition,
            IDiffContentLoader contentLoader,
            DiffCalculatorSync syncCalculator,
            Encoding defaultEncoding,
            Action<DiffViewerData> onPurgedDetected = null)
        {
            if (bigFileDownloader == null)
            {
                AsyncCalculateDiff(
                    diffControl,
                    diffViewerData,
                    movedDetectionOptions,
                    comparisonMethod,
                    rightContentForEdition,
                    contentLoader,
                    syncCalculator);
                return;
            }

            diffControl.DisableBigFileMessagePanel();
            diffControl.ShowWaitingAnimation();

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(TIMER_INTERVAL);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    mLog.Info(
                        "OnCalculateDifferencesButtonClick thread about to DownloadFiles");

                    bigFileDownloader.DownloadFiles(diffViewerData, defaultEncoding);
                },
                /*afterOperationDelegate*/ delegate
                {
                    diffControl.HideWaitingAnimation();
                    diffControl.EnableBigFileMessagePanel();

                    if (waiter.Exception != null)
                    {
                        syncCalculator.CleanCurrentDiffCalculation();

                        diffControl.HandleException(waiter.Exception);
                        return;
                    }

                    if (onPurgedDetected != null &&
                        (diffViewerData.Left?.IsPurged == true ||
                         diffViewerData.Right?.IsPurged == true))
                    {
                        onPurgedDetected(diffViewerData);
                        return;
                    }

                    AsyncCalculateDiff(
                        diffControl,
                        diffViewerData,
                        movedDetectionOptions,
                        comparisonMethod,
                        rightContentForEdition,
                        contentLoader,
                        syncCalculator);
                });
        }

        internal static bool EncodingChanged(
            TextBoxContributor contributor,
            Encoding encoding,
            IDiffControl diffControl,
            DiffViewerData diffViewerData,
            MovedDetectionOptionsInfo movedDetectionOptions,
            ComparisonMethodTypes comparisonMethod,
            IDiffContent rightContentForEdition,
            IDiffContentLoader contentLoader,
            DiffCalculatorSync syncCalculator)
        {
            diffControl.OnRightTextBoxClean();

            DiffCalculator calculator = BuildDiffCalculator(
                diffViewerData,
                movedDetectionOptions,
                comparisonMethod,
                rightContentForEdition,
                contentLoader);

            if (!syncCalculator.TrySetCurrentDiffCalculation(calculator))
                return false;

            bool bLeftFile = contributor == TextBoxContributor.Left;

            if (bLeftFile)
                diffViewerData.Left.Encoding = encoding;
            else
                diffViewerData.Right.Encoding = encoding;

            diffControl.ShowWaitingAnimation();

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(TIMER_INTERVAL);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    mLog.Info("OnEncodingChanged thread about to recalculate " +
                        "diff with a different encoding");

                    calculator.CalculateDiff();
                },
                /*afterOperationDelegate*/ delegate
                {
                    if (!syncCalculator.IsCurrentDiffCalculation(calculator))
                        return;

                    diffControl.HideWaitingAnimation();

                    if (waiter.Exception != null)
                    {
                        syncCalculator.CleanCurrentDiffCalculation();

                        diffControl.HandleException(waiter.Exception);
                        return;
                    }

                    DiffCalculator.Result result = calculator.GetResult();
                    diffControl.SetDiffDrawingInfo(result.DiffDrawingInfo);

                    diffControl.UpdateTextContent(
                        result.LeftContent, result.RightContent);

                    diffControl.SetDifferencesInfo(
                        result.DiffDrawingInfo,
                        diffViewerData);

                    syncCalculator.CleanCurrentDiffCalculation();
                });

            return true;
        }

        internal static void OnRightTextViewContentChanged(
            IDiffControl diffControl,
            DiffViewerData diffViewerData,
            MovedDetectionOptionsInfo movedDetectionOptions,
            ComparisonMethodTypes comparisonMethod,
            bool isRightTextBoxDirty,
            IDiffContent rightContentForEdition,
            IDiffContentLoader contentLoader,
            DiffCalculatorSync syncCalculator)
        {
            DiffCalculator calculator = BuildDiffCalculator(
                diffViewerData,
                movedDetectionOptions,
                comparisonMethod,
                rightContentForEdition,
                contentLoader);

            syncCalculator.ReplaceCurrentDiffCalculation(calculator);

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(TIMER_INTERVAL);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    calculator.CalculateDiff();
                },
                /*afterOperationDelegate*/ delegate
                {
                    if (!syncCalculator.IsCurrentDiffCalculation(calculator))
                        return;

                    if (waiter.Exception != null)
                    {
                        syncCalculator.CleanCurrentDiffCalculation();

                        diffControl.HandleException(waiter.Exception);
                        return;
                    }

                    DiffCalculator.Result result = calculator.GetResult();
                    diffControl.SetDiffDrawingInfo(result.DiffDrawingInfo);

                    diffControl.UpdateDifferencesInfoSilently(
                        result.DiffDrawingInfo);

                    syncCalculator.CleanCurrentDiffCalculation();
                });
        }

        static DiffCalculator BuildDiffCalculator(
            DiffViewerData diffViewerData,
            MovedDetectionOptionsInfo movedDetectionOptions,
            ComparisonMethodTypes comparisonMethod,
            IDiffContent rightContentForEdition,
            IDiffContentLoader contentLoader)
        {
            return new DiffCalculator(
                diffViewerData,
                movedDetectionOptions,
                comparisonMethod,
                rightContentForEdition,
                contentLoader);
        }

        const int TIMER_INTERVAL = 10;

        static readonly ILog mLog = LogManager.GetLogger("DiffControl");
    }
}
