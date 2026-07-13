using System.Collections.Generic;

using Codice.Client.Common.Threading;
using Codice.CM.Client.Differences;
using Codice.CM.Client.Differences.Graphic;
using MergetoolGui;
using PlasticGui;

using Unity.PlasticSCM.Editor.Diff.Asset.Content.Meta;
using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;
using Unity.PlasticSCM.Editor.Diff.Asset.Content.Property;
using Unity.PlasticSCM.Editor.Diff.Asset.Content.UnityObject;
using Unity.PlasticSCM.Editor.Diff.Asset.Content.Filtering;
using Unity.PlasticSCM.Editor.Diff.Asset.Content.Search;
using Unity.PlasticSCM.Editor.Diff.Purged;
using Unity.PlasticSCM.Editor.Diff.Text;
using Unity.PlasticSCM.Editor.UI;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using XDiffGui;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content
{
    internal class AssetContentControl : VisualElement, IBigFilePanelListener
    {
        internal AssetContentControl(
            IBigFileDownloader bigFileDownloader,
            IBigFileChecker bigFileChecker)
        {
            mBigFileDownloader = bigFileDownloader;
            mBigFileChecker = bigFileChecker;

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

        internal void Dispose()
        {
            mToolbarPanel.Dispose();

            if (mBigFileMessagePanel != null)
                mBigFileMessagePanel.Dispose();

            if (mPurgedRevisionControl != null)
                mPurgedRevisionControl.Dispose();
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

        void DisplayCurrentData(bool isReload)
        {
            DiffViewerData diffData = mDiffViewerData;

            ReplaceMainView(mNormalContentPanel);

            EntryData entryData = diffData.Left;

            mContributorLabel.text = PlasticLocalization.Name.Content.GetString(
                entryData.SymbolicName);

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

            ShowAssetContent(entryData.File, isReload);
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
        // loaded into mObjectContents via LoadSerializedFileAndForget.
        // Without a fresh load the next re-bind would render every header
        // as a fallback ("Unknown" / generic file icon), since the
        // underlying native objects are gone even though the C# refs remain.
        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (mDiffViewerData == null)
                return;

            if (state != PlayModeStateChange.EnteredEditMode
                && state != PlayModeStateChange.EnteredPlayMode)
                return;

            DisplayCurrentData(isReload: true);
        }

        void ShowAssetContent(string file, bool isReload)
        {
            mContributorLabel.style.borderBottomWidth = 0;

            ReplaceView(mListView);

            mVisibleRows.Clear();

            ExpansionSnapshot previousCache = isReload
                ? SnapshotExpansionState()
                : null;

            bool isMetaContent = MetaPath.IsMetaPath(file);

            mObjectContents = isMetaContent
                ? MetaFileContents.BuildContentData(file)
                : UnityObjectContents.BuildContentData(file);

            ApplyExpansionState(previousCache);

            UpdateAvailableFilters(isMetaContent);

            RebuildVisibleRows();
        }

        void UpdateAvailableFilters(bool isMetaContent)
        {
            if (isMetaContent)
            {
                mToolbarPanel.UpdateAvailableFilters(null, null, show: false);
                return;
            }

            mToolbarPanel.UpdateAvailableFilters(
                ObjectContentOptions.ExtractGameObjects(mObjectContents, GetGameObjectIcon),
                ObjectContentOptions.ExtractTypes(mObjectContents, GetObjectIcon),
                show: true);
        }

        static UnityEngine.Texture GetGameObjectIcon(ObjectContent content)
        {
            GameObject go = content.GetGameObject();
            if (go != null)
                return EditorGUIUtility.ObjectContent(go, typeof(GameObject)).image;

            return GetObjectIcon(content);
        }

        static UnityEngine.Texture GetObjectIcon(ObjectContent content)
        {
            UnityEngine.Object obj = content.Object;
            if (obj == null)
                return null;

            return EditorGUIUtility.ObjectContent(obj, obj.GetType()).image;
        }

        ExpansionSnapshot SnapshotExpansionState()
        {
            ExpansionSnapshot snapshot = new ExpansionSnapshot();

            foreach (ObjectContent obj in mObjectContents)
            {
                snapshot.Objects[BuildObjectKey(obj)] =
                    mExpandedObjects.Contains(obj.GetObjectId());

                if (obj.ComponentContents == null)
                    continue;

                for (int i = 0; i < obj.ComponentContents.Count; i++)
                {
                    ObjectContent comp = obj.ComponentContents[i];
                    snapshot.Objects[BuildComponentKey(obj, comp, i)] =
                        mExpandedObjects.Contains(comp.GetObjectId());
                }
            }

            foreach (string fullKey in mCollapsedGroups)
                snapshot.CollapsedGroups.Add(fullKey);

            return snapshot;
        }

        void ApplyExpansionState(ExpansionSnapshot previousCache)
        {
            mExpandedObjects.Clear();
            mCollapsedGroups.Clear();

            foreach (ObjectContent obj in mObjectContents)
            {
                if (ShouldExpand(previousCache?.Objects, BuildObjectKey(obj), DefaultExpand(obj)))
                    mExpandedObjects.Add(obj.GetObjectId());

                if (obj.ComponentContents == null)
                    continue;

                for (int i = 0; i < obj.ComponentContents.Count; i++)
                {
                    ObjectContent comp = obj.ComponentContents[i];
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

        static bool DefaultExpand(ObjectContent obj)
        {
            return true;
        }

        static string BuildObjectKey(ObjectContent obj)
        {
            return "o:" + obj.GetDisplayName();
        }

        static string BuildComponentKey(
            ObjectContent parent, ObjectContent comp, int index)
        {
            return "c:" + parent.GetDisplayName() + "/" + index + ":" + comp.GetDisplayName();
        }

        static string BuildGroupFullKey(ObjectContent owner, string groupKey)
        {
            return owner.GetObjectId() + ":" + groupKey;
        }

        void ShowBigFileMessage()
        {
            if (mBigFileMessagePanel == null)
                mBigFileMessagePanel = new BigFileMessagePanel(this, false);

            mBigFileMessagePanel.UpdateDisplayData(
                BigFileDisplayData.Build(
                    mDiffViewerData, mBigFileDownloader));

            mContributorLabel.style.borderBottomWidth = 1;

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

        void RebuildVisibleRows()
        {
            mVisibleRows.Clear();
            mKnownGroups.Clear();
            mSearchIndex = PropertyContentSearchIndex.Build(mObjectContents, mSearchFilter);

            CollectAllGroupKeys();

            foreach (ObjectContent objContent in mObjectContents)
            {
                if (!PassesGameObjectFilter(objContent))
                    continue;

                if (mSearchIndex.IsActive)
                    AddObjectRowsWithSearch(objContent);
                else
                    AddObjectRows(objContent);
            }

            mListView.RefreshItems();
        }

        bool PassesGameObjectFilter(ObjectContent content)
        {
            return ObjectContentFilters.PassesGameObjectFilter(content, mSelectedGameObjects);
        }

        bool PassesTypeFilter(ObjectContent content)
        {
            return ObjectContentFilters.PassesTypeFilter(content, mSelectedTypes);
        }

        void AddObjectRows(ObjectContent objContent)
        {
            bool selfTypePasses = PassesTypeFilter(objContent);
            bool anyChildPasses = HasVisibleChildComponent(objContent);

            if (!selfTypePasses && !anyChildPasses)
                return;

            mVisibleRows.Add(new ContentRow(
                ContentRowKind.ObjectHeader, objContent, null));

            if (!mExpandedObjects.Contains(objContent.GetObjectId()))
                return;

            if (selfTypePasses)
                AddPropertyRows(objContent, indentLevel: 0);

            AddComponentRows(objContent);
        }

        bool HasVisibleChildComponent(ObjectContent parent)
        {
            if (parent.ComponentContents == null)
                return false;

            foreach (ObjectContent comp in parent.ComponentContents)
            {
                if (PassesTypeFilter(comp))
                    return true;
            }

            return false;
        }

        void AddComponentRows(ObjectContent parent)
        {
            if (parent.ComponentContents == null)
                return;

            foreach (ObjectContent compContent in parent.ComponentContents)
            {
                if (!PassesTypeFilter(compContent))
                    continue;

                mVisibleRows.Add(new ContentRow(
                    ContentRowKind.ComponentHeader, compContent, null, indentLevel: 1));

                if (!mExpandedObjects.Contains(compContent.GetObjectId()))
                    continue;

                AddPropertyRows(compContent, indentLevel: 1);
            }
        }

        void AddPropertyRows(ObjectContent objContent, int indentLevel)
        {
            PropertyTreeNode root = objContent.PropertyTree;
            if (root == null)
                return;

            mWalker.Walk(
                root.Children,
                indentLevel,
                mSearchIndex.IsNodeVisible,
                node => BuildGroupFullKey(objContent, node.Path),
                IsGroupExpanded,
                (node, indent) => AddPropertyRow(
                    objContent, ToPropertyContent(node), indent),
                (node, indent, fullKey) => AddContainerHeaderRow(
                    objContent, node, fullKey, indent));
        }

        // Containers with zero children render as a regular property row with
        // the empty literal in the value column — that matches how Unity's
        // Inspector shows empty arrays and keeps the visual hierarchy honest
        // (a bold expandable header without children would lie about being
        // expandable).
        void AddContainerHeaderRow(
            ObjectContent objContent,
            PropertyTreeNode node,
            string fullKey,
            int indent)
        {
            if (node.Children.Count == 0)
            {
                AddPropertyRow(
                    objContent, ToEmptyContainerProperty(node), indent);
                return;
            }

            mVisibleRows.Add(new ContentRow(
                ContentRowKind.ContainerHeader, objContent, fullKey,
                node.DisplayName, indent));
        }

        static PropertyContent ToEmptyContainerProperty(PropertyTreeNode node)
        {
            return new PropertyContent
            {
                Path = node.Path,
                DisplayName = node.DisplayName,
                TypeTag = node.TypeTag,
                Value = node.Kind == NodeKind.Array ? "[]" : "{}",
                Tag = null
            };
        }

        void CollectAllGroupKeys()
        {
            foreach (ObjectContent objContent in mObjectContents)
            {
                CollectGroupKeysForOwner(objContent);

                if (objContent.ComponentContents == null)
                    continue;

                foreach (ObjectContent comp in objContent.ComponentContents)
                    CollectGroupKeysForOwner(comp);
            }
        }

        void CollectGroupKeysForOwner(ObjectContent owner)
        {
            if (owner.PropertyTree == null)
                return;

            mWalker.CollectGroupKeys(
                owner.PropertyTree.Children,
                node => BuildGroupFullKey(owner, node.Path),
                key => mKnownGroups.Add(key));
        }

        bool IsGroupExpanded(string fullKey)
        {
            if (mSearchIndex.IsActive)
                return true;
            return !mCollapsedGroups.Contains(fullKey);
        }

        static PropertyContent ToPropertyContent(PropertyTreeNode node)
        {
            return new PropertyContent
            {
                Path = node.Path,
                DisplayName = node.DisplayName,
                TypeTag = node.TypeTag,
                Value = node.Value,
                Tag = node.Tag
            };
        }

        void AddPropertyRow(
            ObjectContent owner, PropertyContent prop, int indentLevel)
        {
            mVisibleRows.Add(new ContentRow(
                ContentRowKind.Property, owner, prop, indentLevel));

            if (PropertyRow.IsTallProperty(prop.Tag))
                mVisibleRows.Add(new ContentRow(
                    ContentRowKind.Property, owner, prop,
                    indentLevel, isSpacer: true));
        }

        void AddObjectRowsWithSearch(ObjectContent objContent)
        {
            bool selfTypePasses = PassesTypeFilter(objContent);

            bool nameMatches = SearchMatcher.Contains(objContent.GetDisplayName(), mSearchFilter);

            bool hasOwnPropertyMatch = selfTypePasses && SubtreeHasSearchMatch(objContent);
            bool hasComponentMatch = HasComponentSearchMatch(objContent);

            bool selfWouldShow = selfTypePasses
                && (hasOwnPropertyMatch || nameMatches);

            bool nameMatchSurfacesChildren =
                nameMatches && HasVisibleChildComponent(objContent);

            if (!selfWouldShow && !hasComponentMatch && !nameMatchSurfacesChildren)
                return;

            mVisibleRows.Add(new ContentRow(
                ContentRowKind.ObjectHeader, objContent, null));

            if (hasOwnPropertyMatch
                || (selfTypePasses && nameMatches
                    && mExpandedObjects.Contains(objContent.GetObjectId())))
            {
                AddPropertyRows(objContent, indentLevel: 0);
            }

            AddComponentRowsWithSearch(objContent, nameMatches);
        }

        void AddComponentRowsWithSearch(
            ObjectContent parent, bool parentNameMatches)
        {
            if (parent.ComponentContents == null)
                return;

            foreach (ObjectContent compContent in parent.ComponentContents)
            {
                if (!PassesTypeFilter(compContent))
                    continue;

                bool compNameMatches = SearchMatcher.Contains(
                    compContent.GetDisplayName(), mSearchFilter);

                bool compHasPropertyMatch = SubtreeHasSearchMatch(compContent);

                if (!parentNameMatches && !compNameMatches && !compHasPropertyMatch)
                    continue;

                mVisibleRows.Add(new ContentRow(
                    ContentRowKind.ComponentHeader, compContent, null, indentLevel: 1));

                if (compHasPropertyMatch
                    || mExpandedObjects.Contains(compContent.GetObjectId()))
                {
                    AddPropertyRows(compContent, indentLevel: 1);
                }
            }
        }

        bool SubtreeHasSearchMatch(ObjectContent objContent)
        {
            return mSearchIndex.SubtreeHasMatch(objContent);
        }

        bool HasComponentSearchMatch(ObjectContent objContent)
        {
            if (objContent.ComponentContents == null)
                return false;

            foreach (ObjectContent compContent in objContent.ComponentContents)
            {
                if (!PassesTypeFilter(compContent))
                    continue;

                if (SearchMatcher.Contains(compContent.GetDisplayName(), mSearchFilter))
                    return true;

                if (SubtreeHasSearchMatch(compContent))
                    return true;
            }
            return false;
        }

        void OnBindRow(VisualElement element, int index)
        {
            if (index < 0 || index >= mVisibleRows.Count)
                return;

            ContentRow row = mVisibleRows[index];
            ContentRowElement rowElement = (ContentRowElement)element;

            if (row.IsSpacer)
            {
                rowElement.BindSpacer();
                return;
            }

            switch (row.Kind)
            {
                case ContentRowKind.ObjectHeader:
                case ContentRowKind.ComponentHeader:
                    bool isExpanded = mExpandedObjects.Contains(
                        row.ObjectContent.GetObjectId());
                    rowElement.BindHeader(
                        row.ObjectContent,
                        isExpanded,
                        mSearchFilter,
                        row.IndentLevel);
                    return;

                case ContentRowKind.ContainerHeader:
                    rowElement.BindGroupHeader(
                        row.GroupDisplayName,
                        row.GroupKey,
                        IsGroupExpanded(row.GroupKey),
                        mSearchFilter,
                        row.IndentLevel);
                    return;

                default:
                    rowElement.BindProperty(
                        row.PropertyContent,
                        mSearchFilter,
                        row.IndentLevel);
                    return;
            }
        }

        void OnObjectToggled(ObjectContent objContent)
        {
            int id = objContent.GetObjectId();
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
            foreach (ObjectContent objContent in mObjectContents)
            {
                mExpandedObjects.Add(objContent.GetObjectId());

                if (objContent.ComponentContents == null)
                    continue;

                foreach (ObjectContent compContent in objContent.ComponentContents)
                    mExpandedObjects.Add(compContent.GetObjectId());
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

        void CreateGUI()
        {
            style.flexGrow = 1;

            mToolbarPanel = new ContentToolbarPanel(
                SetSearchFilter, ExpandAll, CollapseAll,
                SetGameObjectFilter, SetTypeFilter);

            mContributorLabel = new Label();
            mContributorLabel.style.paddingLeft = 6;
            mContributorLabel.style.paddingTop = 4;
            mContributorLabel.style.paddingBottom = 4;
            mContributorLabel.style.borderBottomColor = UnityStyles.Colors.BarBorder;

            mListView = CreateListView();

            mContainerPanel = new VisualElement();
            mContainerPanel.style.flexGrow = 1;

            mNormalContentPanel = new VisualElement();
            mNormalContentPanel.style.flexGrow = 1;
            mNormalContentPanel.Add(mToolbarPanel);
            mNormalContentPanel.Add(mContributorLabel);
            mNormalContentPanel.Add(mContainerPanel);

            Add(mNormalContentPanel);

            ReplaceView(mListView);
        }

        ListView CreateListView()
        {
            ListView result = new ListView();

            result.style.flexGrow = 1;
            result.fixedItemHeight = ContentRowElement.ROW_HEIGHT;
            result.makeItem = () => new ContentRowElement(OnObjectToggled, OnGroupToggled);
            result.bindItem = OnBindRow;
            result.selectionType = SelectionType.None;
            result.itemsSource = mVisibleRows;

            return result;
        }

        ContentToolbarPanel mToolbarPanel;
        Label mContributorLabel;
        ListView mListView;
        VisualElement mNormalContentPanel;
        VisualElement mContainerPanel;
        VisualElement mCurrentView;
        BigFileMessagePanel mBigFileMessagePanel;
        PurgedRevisionControl mPurgedRevisionControl;

        List<ObjectContent> mObjectContents = new List<ObjectContent>();
        readonly HashSet<int> mExpandedObjects = new HashSet<int>();
        readonly HashSet<string> mCollapsedGroups = new HashSet<string>();
        readonly HashSet<string> mKnownGroups = new HashSet<string>();
        readonly List<ContentRow> mVisibleRows = new List<ContentRow>();
        PropertyContentSearchIndex mSearchIndex = PropertyContentSearchIndex.Build(
            new List<ObjectContent>(), null);

        string mSearchFilter = string.Empty;
        HashSet<string> mSelectedGameObjects;
        HashSet<string> mSelectedTypes;
        DiffViewerData mDiffViewerData;

        readonly IBigFileDownloader mBigFileDownloader;
        readonly IBigFileChecker mBigFileChecker;

        static readonly PropertyTreeWalker<PropertyTreeNode> mWalker =
            new PropertyTreeWalker<PropertyTreeNode>(
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
