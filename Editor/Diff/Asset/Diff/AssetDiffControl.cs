using System;
using System.Collections.Generic;

using Codice.Client.Common.Threading;
using Codice.CM.Client.Differences;
using Codice.CM.Client.Differences.Graphic;
using MergetoolGui;
using PlasticGui;

using Unity.PlasticSCM.Editor.Diff.Asset.Diff.Meta;
using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;
using Unity.PlasticSCM.Editor.Diff.Asset.Diff.Property;
using Unity.PlasticSCM.Editor.Diff.Asset.Diff.UnityObject;
using Unity.PlasticSCM.Editor.Diff.Asset.Diff.Filtering;
using Unity.PlasticSCM.Editor.Diff.Asset.Diff.Search;
using Unity.PlasticSCM.Editor.Diff.Purged;
using Unity.PlasticSCM.Editor.Diff.Text;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using XDiffGui;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff
{
    internal class AssetDiffControl : VisualElement, IBigFilePanelListener
    {
        internal AssetDiffControl(
            IBigFileDownloader bigFileDownloader,
            IBigFileChecker bigFileChecker,
            Action onViewAsTextDiff)
        {
            mBigFileDownloader = bigFileDownloader;
            mBigFileChecker = bigFileChecker;
            mOnViewAsTextDiff = onViewAsTextDiff;

            CreateGUI();

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                AssetReloadNotifier.AssetsChanged += OnAssetsChanged;
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                AssetReloadNotifier.AssetsChanged -= OnAssetsChanged;
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            });
        }

        internal void ShowData(DiffViewerData diffData)
        {
            if (diffData == null)
                return;

            mDiffViewerData = diffData;

            DisplayCurrentData(isReload: false);
        }

        void DisplayCurrentData(bool isReload)
        {
            DiffViewerData diffData = mDiffViewerData;

            string leftContributor;
            string rightContributor;

            SymbolicNameParser.GetContributorSpecs(diffData.Left.SymbolicName,
                diffData.Right.SymbolicName, out leftContributor, out rightContributor);

            mContributorsHeaderPanel.SetNames(leftContributor, rightContributor);

            mMessagePanel.Hide();

            ReplaceMainView(mContentPanel);

            if (BigFileDiffCalculator.IsBigFileDiff(diffData, mBigFileDownloader, mBigFileChecker))
            {
                ShowBigFileMessage();
                return;
            }

            if (diffData.Left?.IsPurged == true || diffData.Right?.IsPurged == true)
            {
                ShowPurgedRevision(diffData);
                return;
            }

            ShowAssetDiff(diffData.Left.File, diffData.Right.File, isReload);
        }

        void OnAssetsChanged(HashSet<string> changedFullPaths)
        {
            if (mDiffViewerData == null)
                return;

            if (!AssetReloadNotifier.IsWatchedPath(
                    mDiffViewerData.Left?.File, changedFullPaths)
                && !AssetReloadNotifier.IsWatchedPath(
                    mDiffViewerData.Right?.File, changedFullPaths))
                return;

            DisplayCurrentData(isReload: true);
        }

        // Entering or exiting play mode causes Unity to drop the objects we
        // loaded into mObjectDiffs via LoadSerializedFileAndForget. Without
        // a fresh load the next re-bind would render every header as a
        // fallback ("Unknown" / generic file icon), since the underlying
        // native objects are gone even though the C# refs remain.
        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (mDiffViewerData == null)
                return;

            if (state != PlayModeStateChange.EnteredEditMode
                && state != PlayModeStateChange.EnteredPlayMode)
                return;

            DisplayCurrentData(isReload: true);
        }

        void ShowAssetDiff(string leftFile, string rightFile, bool isReload)
        {
            ReplaceView(mListView);

            mVisibleRows.Clear();

            ExpansionSnapshot previousCache = isReload
                ? SnapshotExpansionState()
                : null;

            bool isMetaDiff = MetaPath.IsMetaPath(leftFile);

            mObjectDiffs = isMetaDiff ?
                    MetaFileDiffs.BuildDiffData(leftFile, rightFile) :
                    UnityObjectDiffs.BuildDiffData(leftFile, rightFile);

            UpdateDataLossBanner(mObjectDiffs);

            ApplyExpansionState(previousCache);

            UpdateAvailableFilters(isMetaDiff);

            RebuildVisibleRows();
        }

        void UpdateAvailableFilters(bool isMetaDiff)
        {
            if (isMetaDiff)
            {
                mDiffToolbarPanel.UpdateAvailableFilters(null, null, show: false);
                return;
            }

            mDiffToolbarPanel.UpdateAvailableFilters(
                ObjectDiffOptions.ExtractGameObjects(mObjectDiffs, GetGameObjectIcon),
                ObjectDiffOptions.ExtractTypes(mObjectDiffs, GetIconForObject),
                show: true);
        }

        static UnityEngine.Texture GetGameObjectIcon(ObjectDiff diff)
        {
            GameObject go = diff.GetGameObject();
            if (go != null)
                return EditorGUIUtility.ObjectContent(go, typeof(GameObject)).image;

            return GetIconForObject(diff);
        }

        static UnityEngine.Texture GetIconForObject(ObjectDiff diff)
        {
            UnityEngine.Object obj = diff.SrcObject ?? diff.DstObject;
            if (obj == null)
                return null;

            return EditorGUIUtility.ObjectContent(obj, obj.GetType()).image;
        }

        ExpansionSnapshot SnapshotExpansionState()
        {
            ExpansionSnapshot snapshot = new ExpansionSnapshot();

            foreach (ObjectDiff obj in mObjectDiffs)
            {
                AddIfDeviatesFromDefault(snapshot.Objects, BuildObjectKey(obj), obj);

                if (obj.ComponentDiffs == null)
                    continue;

                for (int i = 0; i < obj.ComponentDiffs.Count; i++)
                {
                    ObjectDiff comp = obj.ComponentDiffs[i];
                    AddIfDeviatesFromDefault(
                        snapshot.Objects, BuildComponentKey(obj, comp, i), comp);
                }
            }

            foreach (string fullKey in mCollapsedGroups)
                snapshot.CollapsedGroups.Add(fullKey);

            return snapshot;
        }

        void AddIfDeviatesFromDefault(
            Dictionary<string, bool> snapshot, string key, ObjectDiff obj)
        {
            bool isExpanded = mExpandedObjects.Contains(obj.GetObjectId());
            if (isExpanded == DefaultExpand(obj))
                return;

            snapshot[key] = isExpanded;
        }

        void ApplyExpansionState(ExpansionSnapshot previousCache)
        {
            mExpandedObjects.Clear();
            mCollapsedGroups.Clear();

            foreach (ObjectDiff obj in mObjectDiffs)
            {
                if (ShouldExpand(previousCache?.Objects, BuildObjectKey(obj), DefaultExpand(obj)))
                    mExpandedObjects.Add(obj.GetObjectId());

                if (obj.ComponentDiffs == null)
                    continue;

                for (int i = 0; i < obj.ComponentDiffs.Count; i++)
                {
                    ObjectDiff comp = obj.ComponentDiffs[i];
                    if (ShouldExpand(previousCache?.Objects, BuildComponentKey(obj, comp, i), DefaultExpand(comp)))
                        mExpandedObjects.Add(comp.GetObjectId());
                }
            }

            if (previousCache == null)
                return;

            foreach (string fullKey in previousCache.CollapsedGroups)
                mCollapsedGroups.Add(fullKey);
        }

        static bool ShouldExpand(
            Dictionary<string, bool> previousCache, string key, bool defaultExpand)
        {
            if (previousCache != null && previousCache.TryGetValue(key, out bool wasExpanded))
                return wasExpanded;

            return defaultExpand;
        }

        static bool DefaultExpand(ObjectDiff obj)
        {
            bool hasComponents = obj.ComponentDiffs != null && obj.ComponentDiffs.Count > 0;
            if (hasComponents)
                return true;

            if (obj.DiffType != DiffType.Modified)
                return false;

            return obj.PropertyDiffTree != null && obj.PropertyDiffTree.Children.Count > 0;
        }

        static string BuildObjectKey(ObjectDiff obj)
        {
            return "o:" + obj.GetSrcDisplayName();
        }

        static string BuildComponentKey(
            ObjectDiff parent, ObjectDiff comp, int index)
        {
            return "c:" + parent.GetSrcDisplayName() + "/" + index + ":" + comp.GetSrcDisplayName();
        }

        static string BuildGroupFullKey(ObjectDiff owner, string groupKey)
        {
            return owner.GetObjectId() + ":" + groupKey;
        }

        void IBigFilePanelListener.OnCalculateDifferencesButtonClick()
        {
            mBigFileMessagePanel.Disable();

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    mBigFileDownloader.DownloadFiles(mDiffViewerData, null);
                },
                /*afterOperationDelegate*/ delegate
                {
                    mBigFileMessagePanel.Enable();

                    if (waiter.Exception != null)
                    {
                        MergetoolExceptionsHandler.DisplayException(
                            waiter.Exception);
                        return;
                    }

                    DisplayCurrentData(isReload: false);
                });
        }

        void ShowBigFileMessage()
        {
            if (mBigFileMessagePanel == null)
                mBigFileMessagePanel = new BigFileMessagePanel(this, false);

            mBigFileMessagePanel.UpdateDisplayData(
                BigFileDisplayData.Build(
                    mDiffViewerData, mBigFileDownloader));

            mContributorsHeaderPanel.SetSeparatorsVisible(true);

            ReplaceView(mBigFileMessagePanel);
        }

        void ShowPurgedRevision(DiffViewerData data)
        {
            if (mPurgedRevisionControl == null)
                mPurgedRevisionControl = new PurgedRevisionControl();

            mPurgedRevisionControl.ShowData(data);
            ReplaceMainView(mPurgedRevisionControl);
        }

        void ReplaceMainView(VisualElement panel)
        {
            if (hierarchy.childCount == 1 && hierarchy[0] == panel)
                return;

            Clear();
            Add(panel);
        }

        void ReplaceView(VisualElement view)
        {
            if (mCurrentView == view)
                return;

            if (mCurrentView != null)
                mContainerPanel.Remove(mCurrentView);

            mContainerPanel.Add(view);
            mCurrentView = view;
        }

        internal void Dispose()
        {
            mDiffToolbarPanel.Dispose();

            if (mBigFileMessagePanel != null)
                mBigFileMessagePanel.Dispose();

            if (mPurgedRevisionControl != null)
                mPurgedRevisionControl.Dispose();
        }

        void RebuildVisibleRows()
        {
            mVisibleRows.Clear();
            mKnownGroups.Clear();
            mSearchIndex = PropertyDiffSearchIndex.Build(mObjectDiffs, mSearchFilter);

            CollectAllGroupKeys();

            int modified = 0, added = 0, removed = 0;

            foreach (ObjectDiff objDiff in mObjectDiffs)
            {
                CountByDiffType(objDiff.DiffType, ref modified, ref added, ref removed);

                if (objDiff.ComponentDiffs == null)
                    continue;

                foreach (ObjectDiff comp in objDiff.ComponentDiffs)
                    CountByDiffType(comp.DiffType, ref modified, ref added, ref removed);
            }

            foreach (ObjectDiff objDiff in mObjectDiffs)
            {
                if (!PassesGameObjectFilter(objDiff))
                    continue;

                if (mSearchIndex.IsActive)
                    AddObjectRowsWithSearch(objDiff);
                else
                    AddObjectRows(objDiff);
            }

            mDiffToolbarPanel.UpdateFilterButtons(modified, added, removed, mDiffFilter);

            mContributorsHeaderPanel.SetSeparatorsVisible(mVisibleRows.Count == 0);

            if (mVisibleRows.Count == 0)
            {
                ShowEmptyState();
                return;
            }

            ReplaceView(mListView);

            mListView.RefreshItems();
        }

        void ShowEmptyState()
        {
            if (mEmptyStatePanel == null)
                mEmptyStatePanel = new DiffMessagePanel();

            mEmptyStatePanel.ShowMessage(GetEmptyStateMessage());

            ReplaceView(mEmptyStatePanel);
        }

        string GetEmptyStateMessage()
        {
            bool hasSearch = !string.IsNullOrEmpty(mSearchFilter);
            bool hasFilter = mDiffFilter != DiffFilter.All
                || mSelectedGameObjects != null
                || mSelectedTypes != null;

            if (hasSearch || hasFilter)
                return PlasticLocalization.Name.NoDiffsMatchingFilters.GetString();

            return PlasticLocalization.Name.NoSerializedAssetDifferencesFound.GetString();
        }

        void AddObjectRows(ObjectDiff objDiff)
        {
            bool selfPasses = PassesDiffFilter(objDiff.DiffType)
                && PassesTypeFilter(objDiff);
            bool anyChildPasses = HasVisibleChildComponent(objDiff);

            if (!selfPasses && !anyChildPasses)
                return;

            mVisibleRows.Add(new DiffRow(
                DiffRowKind.ObjectHeader, objDiff, null));

            if (!mExpandedObjects.Contains(objDiff.GetObjectId()))
                return;

            if (selfPasses)
                AddPropertyRows(objDiff, 0);

            AddComponentRows(objDiff);
        }

        bool HasVisibleChildComponent(ObjectDiff parent)
        {
            if (parent.ComponentDiffs == null)
                return false;

            foreach (ObjectDiff comp in parent.ComponentDiffs)
            {
                if (!PassesDiffFilter(comp.DiffType))
                    continue;

                if (!PassesTypeFilter(comp))
                    continue;

                return true;
            }

            return false;
        }

        void AddComponentRows(ObjectDiff parentDiff)
        {
            if (parentDiff.ComponentDiffs == null)
                return;

            foreach (ObjectDiff compDiff in parentDiff.ComponentDiffs)
            {
                if (!PassesDiffFilter(compDiff.DiffType))
                    continue;

                if (!PassesTypeFilter(compDiff))
                    continue;

                mVisibleRows.Add(new DiffRow(
                    DiffRowKind.ComponentHeader, compDiff, null, 1));

                if (!mExpandedObjects.Contains(compDiff.GetObjectId()))
                    continue;

                AddPropertyRows(compDiff, 1);
            }
        }

        void AddPropertyRows(ObjectDiff objDiff, int indentLevel)
        {
            PropertyDiffNode root = objDiff.PropertyDiffTree;
            if (root == null)
                return;

            mWalker.Walk(
                root.Children,
                indentLevel,
                IsNodeVisible,
                node => BuildGroupFullKey(objDiff, node.Path),
                IsGroupExpanded,
                (node, indent) => AddPropertyRow(
                    objDiff, ToFlatDiff(node), indent),
                (node, indent, fullKey) => mVisibleRows.Add(new DiffRow(
                    DiffRowKind.ContainerHeader, objDiff, fullKey,
                    node.DisplayName, node.DiffType, indent)));
        }

        void CollectAllGroupKeys()
        {
            foreach (ObjectDiff objDiff in mObjectDiffs)
            {
                CollectGroupKeysForOwner(objDiff);

                if (objDiff.ComponentDiffs == null)
                    continue;

                foreach (ObjectDiff comp in objDiff.ComponentDiffs)
                    CollectGroupKeysForOwner(comp);
            }
        }

        void CollectGroupKeysForOwner(ObjectDiff owner)
        {
            if (owner.PropertyDiffTree == null)
                return;

            mWalker.CollectGroupKeys(
                owner.PropertyDiffTree.Children,
                node => BuildGroupFullKey(owner, node.Path),
                key => mKnownGroups.Add(key));
        }

        bool IsNodeVisible(PropertyDiffNode node)
        {
            if (!HasFilterMatchInSubtree(node))
                return false;

            return mSearchIndex.IsNodeVisible(node);
        }

        bool HasFilterMatchInSubtree(PropertyDiffNode node)
        {
            switch (mDiffFilter)
            {
                case DiffFilter.All:
                    return node.DescendantDiffTypes != DiffTypeFlags.None;
                case DiffFilter.Modified:
                    return node.HasDescendantOfType(DiffType.Modified);
                case DiffFilter.Added:
                    return node.HasDescendantOfType(DiffType.Added);
                case DiffFilter.Removed:
                    return node.HasDescendantOfType(DiffType.Removed);
                default:
                    return true;
            }
        }

        bool IsGroupExpanded(string fullKey)
        {
            if (mSearchIndex.IsActive)
                return true;
            return !mCollapsedGroups.Contains(fullKey);
        }

        static PropertyDiff ToFlatDiff(PropertyDiffNode node)
        {
            return new PropertyDiff
            {
                Path = node.Path,
                DisplayName = node.DisplayName,
                TypeTag = node.TypeTag,
                DiffType = node.DiffType,
                SrcValue = node.SrcValue,
                DstValue = node.DstValue,
                SrcTag = node.SrcTag,
                DstTag = node.DstTag
            };
        }

        void AddPropertyRow(
            ObjectDiff owner, PropertyDiff propDiff, int indentLevel)
        {
            mVisibleRows.Add(new DiffRow(
                DiffRowKind.PropertyDiff, owner, propDiff, indentLevel));

            if (PropertyRow.IsTallProperty(propDiff.SrcTag)
                || PropertyRow.IsTallProperty(propDiff.DstTag))
            {
                mVisibleRows.Add(new DiffRow(
                    DiffRowKind.PropertyDiff, owner, propDiff,
                    indentLevel, isSpacer: true));
            }
        }

        void AddObjectRowsWithSearch(ObjectDiff objDiff)
        {
            bool selfPasses = PassesDiffFilter(objDiff.DiffType)
                && PassesTypeFilter(objDiff);

            bool objectNameMatches =
                SearchMatcher.Contains(objDiff.GetSrcDisplayName(), mSearchFilter) ||
                SearchMatcher.Contains(objDiff.GetDstDisplayName(), mSearchFilter);

            bool hasOwnPropertyMatch = selfPasses && SubtreeHasSearchMatch(objDiff);
            bool hasComponentMatch = HasComponentSearchMatch(objDiff);

            bool selfWouldShow = selfPasses
                && (hasOwnPropertyMatch || objectNameMatches);

            bool nameMatchSurfacesChildren =
                objectNameMatches && HasVisibleChildComponent(objDiff);

            if (!selfWouldShow && !hasComponentMatch && !nameMatchSurfacesChildren)
                return;

            mVisibleRows.Add(new DiffRow(
                DiffRowKind.ObjectHeader, objDiff, null));

            if (selfPasses && (hasOwnPropertyMatch || objectNameMatches))
                AddPropertyRows(objDiff, 0);

            AddComponentRowsWithSearch(objDiff, objectNameMatches);
        }

        void AddComponentRowsWithSearch(
            ObjectDiff parentDiff, bool parentNameMatches)
        {
            if (parentDiff.ComponentDiffs == null)
                return;

            foreach (ObjectDiff compDiff in parentDiff.ComponentDiffs)
            {
                if (!PassesDiffFilter(compDiff.DiffType))
                    continue;

                if (!PassesTypeFilter(compDiff))
                    continue;

                bool compNameMatches =
                    SearchMatcher.Contains(
                        compDiff.GetSrcDisplayName(), mSearchFilter) ||
                    SearchMatcher.Contains(
                        compDiff.GetDstDisplayName(), mSearchFilter);

                bool compHasPropertyMatch = SubtreeHasSearchMatch(compDiff);

                if (!parentNameMatches && !compNameMatches && !compHasPropertyMatch)
                    continue;

                mVisibleRows.Add(new DiffRow(
                    DiffRowKind.ComponentHeader, compDiff, null, 1));

                if (compHasPropertyMatch
                    || mExpandedObjects.Contains(compDiff.GetObjectId()))
                {
                    AddPropertyRows(compDiff, 1);
                }
            }
        }

        bool SubtreeHasSearchMatch(ObjectDiff objDiff)
        {
            if (objDiff.PropertyDiffTree == null)
                return false;

            foreach (PropertyDiffNode child in objDiff.PropertyDiffTree.Children)
            {
                if (mSearchIndex.IsNodeVisible(child) && HasFilterMatchInSubtree(child))
                    return true;
            }
            return false;
        }

        bool HasComponentSearchMatch(ObjectDiff objDiff)
        {
            if (objDiff.ComponentDiffs == null)
                return false;

            foreach (ObjectDiff compDiff in objDiff.ComponentDiffs)
            {
                if (!PassesDiffFilter(compDiff.DiffType))
                    continue;

                if (!PassesTypeFilter(compDiff))
                    continue;

                if (SearchMatcher.Contains(
                        compDiff.GetSrcDisplayName(), mSearchFilter) ||
                    SearchMatcher.Contains(
                        compDiff.GetDstDisplayName(), mSearchFilter))
                    return true;

                if (SubtreeHasSearchMatch(compDiff))
                    return true;
            }
            return false;
        }

        static void CountByDiffType(
            DiffType diffType, ref int modified, ref int added, ref int removed)
        {
            switch (diffType)
            {
                case DiffType.Modified: modified++; break;
                case DiffType.Added: added++; break;
                case DiffType.Removed: removed++; break;
            }
        }

        bool PassesDiffFilter(DiffType diffType)
        {
            switch (mDiffFilter)
            {
                case DiffFilter.All: return diffType != DiffType.Unchanged;
                case DiffFilter.Modified: return diffType == DiffType.Modified;
                case DiffFilter.Added: return diffType == DiffType.Added;
                case DiffFilter.Removed: return diffType == DiffType.Removed;
                default: return true;
            }
        }

        bool PassesGameObjectFilter(ObjectDiff diff)
        {
            return ObjectDiffFilters.PassesGameObjectFilter(diff, mSelectedGameObjects);
        }

        bool PassesTypeFilter(ObjectDiff diff)
        {
            return ObjectDiffFilters.PassesTypeFilter(diff, mSelectedTypes);
        }

        void OnBindRow(VisualElement element, int index)
        {
            if (index < 0 || index >= mVisibleRows.Count)
                return;

            DiffRow row = mVisibleRows[index];
            DiffRowElement rowElement = (DiffRowElement)element;

            if (row.IsSpacer)
            {
                rowElement.BindSpacer(row.PropertyDiff, row.IndentLevel);
                return;
            }

            switch (row.Kind)
            {
                case DiffRowKind.ObjectHeader:
                case DiffRowKind.ComponentHeader:
                    bool isExpanded = mExpandedObjects.Contains(
                        row.ObjectDiff.GetObjectId());
                    rowElement.BindHeader(
                        row.ObjectDiff,
                        isExpanded,
                        mSearchFilter,
                        row.IndentLevel);
                    return;

                case DiffRowKind.ContainerHeader:
                    rowElement.BindGroupHeader(
                        row.GroupDisplayName,
                        row.GroupDiffType,
                        row.GroupKey,
                        IsGroupExpanded(row.GroupKey),
                        mSearchFilter,
                        row.IndentLevel);
                    return;

                default:
                    rowElement.BindProperty(
                        row.PropertyDiff,
                        mSearchFilter,
                        row.IndentLevel);
                    return;
            }
        }

        void OnObjectToggled(ObjectDiff objDiff)
        {
            int id = objDiff.GetObjectId();
            if (!mExpandedObjects.Remove(id))
                mExpandedObjects.Add(id);
            RebuildVisibleRows();
        }

        void OnGroupToggled(string groupFullKey)
        {
            if (!mCollapsedGroups.Remove(groupFullKey))
                mCollapsedGroups.Add(groupFullKey);
            RebuildVisibleRows();
        }


        void SetDiffFilter(DiffFilter filter)
        {
            mDiffFilter = filter;
            RebuildVisibleRows();
        }

        void SetSearchFilter(string searchFilter)
        {
            mSearchFilter = searchFilter ?? string.Empty;
            RebuildVisibleRows();
        }

        void SetGameObjectFilter(HashSet<string> selected)
        {
            mSelectedGameObjects = selected;
            RebuildVisibleRows();
        }

        void SetTypeFilter(HashSet<string> selected)
        {
            mSelectedTypes = selected;
            RebuildVisibleRows();
        }

        void ExpandAll()
        {
            foreach (ObjectDiff objDiff in mObjectDiffs)
            {
                mExpandedObjects.Add(objDiff.GetObjectId());

                if (objDiff.ComponentDiffs == null)
                    continue;

                foreach (ObjectDiff compDiff in objDiff.ComponentDiffs)
                    mExpandedObjects.Add(compDiff.GetObjectId());
            }

            mCollapsedGroups.Clear();
            RebuildVisibleRows();
        }

        void CollapseAll()
        {
            mExpandedObjects.Clear();
            foreach (string fullKey in mKnownGroups)
                mCollapsedGroups.Add(fullKey);
            RebuildVisibleRows();
        }

        void UpdateDataLossBanner(List<ObjectDiff> diffs)
        {
            int dataLossCount = CountObjectsWithDataLoss(diffs);

            if (dataLossCount == 0)
            {
                mMessagePanel.Hide();
                return;
            }

            mMessagePanel.HandleMessage(
                DataLossDescriptions.GetBannerMessage(dataLossCount),
                DataLossDescriptions.GetViewAsTextDiffButtonText(),
                mOnViewAsTextDiff);
        }

        // Counts every ObjectDiff (top-level or nested under a GameObject
        // container) whose own DataLoss is non-None. We don't add the
        // container's rolled-up signal because containers don't have their
        // own data — counting them would double-count the children.
        static int CountObjectsWithDataLoss(List<ObjectDiff> diffs)
        {
            int n = 0;
            foreach (ObjectDiff d in diffs)
            {
                if (d.DataLoss != DataLossKind.None)
                    n++;

                if (d.ComponentDiffs == null)
                    continue;

                foreach (ObjectDiff c in d.ComponentDiffs)
                {
                    if (c.DataLoss != DataLossKind.None)
                        n++;
                }
            }
            return n;
        }

        void CreateGUI()
        {
            style.flexGrow = 1;

            mMessagePanel = new MessagePanel();
            mDiffToolbarPanel = new DiffToolbarPanel(
                SetDiffFilter, SetSearchFilter, ExpandAll, CollapseAll,
                SetGameObjectFilter, SetTypeFilter);
            mContributorsHeaderPanel = new ContributorsHeaderPanel();
            mListView = CreateListView();

            mContainerPanel = new VisualElement();
            mContainerPanel.style.flexGrow = 1;

            mContentPanel = new VisualElement();
            mContentPanel.style.flexGrow = 1;
            mContentPanel.Add(mMessagePanel);
            mContentPanel.Add(mDiffToolbarPanel);
            mContentPanel.Add(mContributorsHeaderPanel);
            mContentPanel.Add(mContainerPanel);

            Add(mContentPanel);

            ReplaceView(mListView);
        }

        ListView CreateListView()
        {
            ListView result = new ListView();

            result.style.flexGrow = 1;
            result.fixedItemHeight = DiffRowElement.ROW_HEIGHT;
            result.makeItem = () => new DiffRowElement(OnObjectToggled, OnGroupToggled);
            result.bindItem = OnBindRow;
            result.selectionType = SelectionType.None;
            result.itemsSource = mVisibleRows;

            return result;
        }

        DiffToolbarPanel mDiffToolbarPanel;
        ContributorsHeaderPanel mContributorsHeaderPanel;
        MessagePanel mMessagePanel;
        ListView mListView;
        VisualElement mContentPanel;
        VisualElement mContainerPanel;
        VisualElement mCurrentView;
        BigFileMessagePanel mBigFileMessagePanel;
        PurgedRevisionControl mPurgedRevisionControl;
        DiffMessagePanel mEmptyStatePanel;

        List<ObjectDiff> mObjectDiffs = new List<ObjectDiff>();
        readonly HashSet<int> mExpandedObjects = new HashSet<int>();
        readonly HashSet<string> mCollapsedGroups = new HashSet<string>();
        readonly HashSet<string> mKnownGroups = new HashSet<string>();
        readonly List<DiffRow> mVisibleRows = new List<DiffRow>();
        PropertyDiffSearchIndex mSearchIndex = PropertyDiffSearchIndex.Build(
            new List<ObjectDiff>(), null);

        DiffFilter mDiffFilter = DiffFilter.All;
        HashSet<string> mSelectedGameObjects;
        HashSet<string> mSelectedTypes;

        string mSearchFilter = string.Empty;
        DiffViewerData mDiffViewerData;

        readonly IBigFileDownloader mBigFileDownloader;
        readonly IBigFileChecker mBigFileChecker;
        readonly Action mOnViewAsTextDiff;

        static readonly PropertyTreeWalker<PropertyDiffNode> mWalker =
            new PropertyTreeWalker<PropertyDiffNode>(
                node => node.Kind,
                node => node.Children);

        class ExpansionSnapshot
        {
            internal readonly Dictionary<string, bool> Objects =
                new Dictionary<string, bool>();
            internal readonly HashSet<string> CollapsedGroups =
                new HashSet<string>();
        }
    }
}
