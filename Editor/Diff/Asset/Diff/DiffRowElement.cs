using System;
using Unity.PlasticSCM.Editor.Diff.Asset.Diff.Property;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff
{
    internal class DiffRowElement : VisualElement
    {
        internal const float ROW_HEIGHT = 22f;

        internal DiffRowElement(
            Action<ObjectDiff> toggleObjectCallback,
            Action<string> toggleGroupCallback)
        {
            mToggleObjectCallback = toggleObjectCallback;
            mToggleGroupCallback = toggleGroupCallback;

            CreateGUI();

            RegisterCallback<ClickEvent>(OnClicked);
        }

        internal void BindHeader(
            ObjectDiff objDiff,
            bool isExpanded,
            string searchFilter,
            int indentLevel = 0)
        {
            mBoundObjectDiff = objDiff;
            mBoundGroupKey = null;
            mBoundKind = BoundKind.ObjectHeader;
            pickingMode = PickingMode.Position;

            mHeaderRow.style.display = DisplayStyle.Flex;
            mPropertyRow.style.display = DisplayStyle.None;
            mGroupHeaderRow.style.display = DisplayStyle.None;

            mHeaderRow.Bind(objDiff, isExpanded, searchFilter, indentLevel);
        }

        internal void BindGroupHeader(
            string label,
            DiffType groupDiffType,
            string groupKey,
            bool isExpanded,
            string searchFilter,
            int indentLevel)
        {
            mBoundObjectDiff = null;
            mBoundGroupKey = groupKey;
            mBoundKind = BoundKind.GroupHeader;
            pickingMode = PickingMode.Position;

            mHeaderRow.style.display = DisplayStyle.None;
            mPropertyRow.style.display = DisplayStyle.None;
            mGroupHeaderRow.style.display = DisplayStyle.Flex;

            mGroupHeaderRow.Bind(
                label, groupDiffType, isExpanded, searchFilter, indentLevel);
        }

        internal void BindProperty(
            PropertyDiff propDiff,
            string searchFilter,
            int indentLevel = 0)
        {
            mBoundObjectDiff = null;
            mBoundGroupKey = null;
            mBoundKind = BoundKind.PropertyRow;
            pickingMode = PickingMode.Position;

            mHeaderRow.style.display = DisplayStyle.None;
            mPropertyRow.style.display = DisplayStyle.Flex;
            mGroupHeaderRow.style.display = DisplayStyle.None;

            mPropertyRow.Bind(propDiff, searchFilter, indentLevel);
        }

        internal void BindSpacer(PropertyDiff propDiff, int indentLevel)
        {
            mBoundObjectDiff = null;
            mBoundGroupKey = null;
            mBoundKind = BoundKind.PropertyRow;
            pickingMode = PickingMode.Ignore;

            mHeaderRow.style.display = DisplayStyle.None;
            mPropertyRow.style.display = DisplayStyle.Flex;
            mGroupHeaderRow.style.display = DisplayStyle.None;

            mPropertyRow.BindSpacer(propDiff, indentLevel);
        }

        void CreateGUI()
        {
            style.flexDirection = FlexDirection.Column;
            style.minHeight = ROW_HEIGHT;
            style.overflow = Overflow.Visible;

            mHeaderRow = new DiffHeaderRow();
            Add(mHeaderRow);

            mGroupHeaderRow = new GroupHeaderRow();
            Add(mGroupHeaderRow);

            mPropertyRow = new DiffPropertyRow();
            Add(mPropertyRow);
        }

        void OnClicked(ClickEvent evt)
        {
            switch (mBoundKind)
            {
                case BoundKind.ObjectHeader:
                    if (mBoundObjectDiff != null)
                        mToggleObjectCallback?.Invoke(mBoundObjectDiff);
                    return;

                case BoundKind.GroupHeader:
                    if (mBoundGroupKey != null)
                        mToggleGroupCallback?.Invoke(mBoundGroupKey);
                    return;
            }
        }

        readonly Action<ObjectDiff> mToggleObjectCallback;
        readonly Action<string> mToggleGroupCallback;

        DiffHeaderRow mHeaderRow;
        GroupHeaderRow mGroupHeaderRow;
        DiffPropertyRow mPropertyRow;

        ObjectDiff mBoundObjectDiff;
        string mBoundGroupKey;
        BoundKind mBoundKind;

        enum BoundKind : byte
        {
            None,
            ObjectHeader,
            GroupHeader,
            PropertyRow
        }
    }
}
