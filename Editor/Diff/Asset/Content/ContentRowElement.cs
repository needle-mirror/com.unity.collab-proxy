using System;
using Unity.PlasticSCM.Editor.Diff.Asset.Content.Property;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content
{
    internal class ContentRowElement : VisualElement
    {
        internal const float ROW_HEIGHT = 22f;

        internal ContentRowElement(
            Action<ObjectContent> toggleObjectCallback,
            Action<string> toggleGroupCallback)
        {
            mToggleObjectCallback = toggleObjectCallback;
            mToggleGroupCallback = toggleGroupCallback;

            CreateGUI();

            RegisterCallback<ClickEvent>(OnClicked);
        }

        internal void BindHeader(
            ObjectContent objContent,
            bool isExpanded,
            string searchFilter,
            int indentLevel = 0)
        {
            mBoundObjectContent = objContent;
            mBoundGroupKey = null;
            mBoundKind = BoundKind.ObjectHeader;
            pickingMode = PickingMode.Position;

            mHeaderRow.style.display = DisplayStyle.Flex;
            mPropertyRow.style.display = DisplayStyle.None;
            mGroupHeaderRow.style.display = DisplayStyle.None;

            mHeaderRow.Bind(objContent, isExpanded, searchFilter, indentLevel);
        }

        internal void BindGroupHeader(
            string label,
            string groupKey,
            bool isExpanded,
            string searchFilter,
            int indentLevel)
        {
            mBoundObjectContent = null;
            mBoundGroupKey = groupKey;
            mBoundKind = BoundKind.GroupHeader;
            pickingMode = PickingMode.Position;

            mHeaderRow.style.display = DisplayStyle.None;
            mPropertyRow.style.display = DisplayStyle.None;
            mGroupHeaderRow.style.display = DisplayStyle.Flex;

            mGroupHeaderRow.Bind(label, isExpanded, searchFilter, indentLevel);
        }

        internal void BindProperty(
            PropertyContent prop,
            string searchFilter,
            int indentLevel = 0)
        {
            mBoundObjectContent = null;
            mBoundGroupKey = null;
            mBoundKind = BoundKind.PropertyRow;
            pickingMode = PickingMode.Position;

            mHeaderRow.style.display = DisplayStyle.None;
            mPropertyRow.style.display = DisplayStyle.Flex;
            mGroupHeaderRow.style.display = DisplayStyle.None;

            mPropertyRow.Bind(prop, searchFilter, indentLevel);
        }

        internal void BindSpacer()
        {
            mBoundObjectContent = null;
            mBoundGroupKey = null;
            mBoundKind = BoundKind.PropertyRow;
            pickingMode = PickingMode.Ignore;

            mHeaderRow.style.display = DisplayStyle.None;
            mPropertyRow.style.display = DisplayStyle.None;
            mGroupHeaderRow.style.display = DisplayStyle.None;
        }

        void CreateGUI()
        {
            style.flexDirection = FlexDirection.Column;
            style.minHeight = ROW_HEIGHT;
            style.overflow = Overflow.Visible;

            mHeaderRow = new ContentHeaderRow();
            Add(mHeaderRow);

            mGroupHeaderRow = new ContentGroupHeaderRow();
            Add(mGroupHeaderRow);

            mPropertyRow = new ContentPropertyRow();
            Add(mPropertyRow);
        }

        void OnClicked(ClickEvent evt)
        {
            switch (mBoundKind)
            {
                case BoundKind.ObjectHeader:
                    if (mBoundObjectContent != null)
                        mToggleObjectCallback?.Invoke(mBoundObjectContent);
                    return;

                case BoundKind.GroupHeader:
                    if (mBoundGroupKey != null)
                        mToggleGroupCallback?.Invoke(mBoundGroupKey);
                    return;
            }
        }

        readonly Action<ObjectContent> mToggleObjectCallback;
        readonly Action<string> mToggleGroupCallback;

        ContentHeaderRow mHeaderRow;
        ContentGroupHeaderRow mGroupHeaderRow;
        ContentPropertyRow mPropertyRow;

        ObjectContent mBoundObjectContent;
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
