using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor.IMGUI.Controls;
using UnityEngine;

using Codice.Client.Common;
using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow.Security;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Avatar;
using Unity.PlasticSCM.Editor.UI.Tree;

#if UNITY_6000_2_OR_NEWER
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
#endif

namespace Unity.PlasticSCM.Editor.CloudDrive.CreateWorkspace
{
    internal class CollaboratorsListView : PlasticTreeView,
        SelectUserGroupDialogOperations.ISelectUserGroupDialog
    {
        internal CollaboratorsListView(
            IProgressControls progressControls,
            CollaboratorsListViewHeaderState headerState,
            Action repaintAction)
        {
            mProgressControls = progressControls;

            multiColumnHeader = new CollaboratorsMultiColumnHeader(this, headerState);
            multiColumnHeader.canSort = true;
            multiColumnHeader.sortingChanged += SortingChanged;

            mUsersComparer = BuildPlasticTableComparer<SecurityMember>.Build(
                PlasticTableColumn.Type.String, (s) => s.Name);

            mEmptyStatePanel = new EmptyStatePanel(repaintAction);

            mDelayedFilterAction = new DelayedActionBySecondsRunner(
                DelayedSearchChanged, UnityConstants.SEARCH_DELAYED_INPUT_ACTION_INTERVAL);
        }

        internal void Refresh(string server, string projectGuid)
        {
            SelectUserGroupDialogOperations.LoadSecurityMembers(
                server,
                projectGuid,
                EnumSecurityMembers.Users,
                string.Empty,
                false,
                mbShouldFilterOnServer,
                mProgressControls,
                this);
        }

        internal List<SecurityMember> GetCollaborators()
        {
            List<SecurityMember> result = new List<SecurityMember>();

            foreach (CollaboratorsListViewItem item in mCheckedItems)
                result.Add(item.User);

            return result;
        }

        internal bool IsAnyItemChecked()
        {
            return mCheckedItems.Count > 0;
        }

        internal bool AreAllItemsChecked()
        {
            if (mCheckedItems.Count == 0)
                return false;

            return mCheckedItems.Count == mUnfilteredUsers.Count;
        }

        internal void CheckAllItems()
        {
            mCheckedItems.Clear();

            foreach (CollaboratorsListViewItem row in mRows)
                mCheckedItems.Add(row);
        }

        internal void UnCheckAllItems()
        {
            mCheckedItems.Clear();
        }

        void SelectUserGroupDialogOperations.ISelectUserGroupDialog.FillUsers(List<SecurityMember> users)
        {
            mUnfilteredUsers = users
                .Distinct()
                .Where(user => user.Name != UserInfo.Get().GetCurrentUserName())
                .ToList();
            mCheckedItems.Clear();

            mEmptyStatePanel.UpdateContent(mUnfilteredUsers.Count == 0 ?
                PlasticLocalization.Name.CollaboratorsEmptyState.GetString() : string.Empty);

            Refilter();

            Sort();

            Reload();
        }

        void SelectUserGroupDialogOperations.ISelectUserGroupDialog.SetShouldFilterOnServer()
        {
            mbShouldFilterOnServer = true;
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);

            if (mRows.Count == 0 && !mEmptyStatePanel.IsEmpty())
                mEmptyStatePanel.OnGUI(rect);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem rootItem)
        {
            RegenerateRows(mUsers, rootItem, mRows);

            return mRows;
        }

        protected override void SearchChanged(string newSearch)
        {
            mDelayedFilterAction.Run();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (args.item is CollaboratorsListViewItem)
            {
                CollaboratorListViewItemGUI(
                    args.rowRect,
                    rowHeight,
                    (CollaboratorsListViewItem)args.item,
                    Repaint,
                    args.selected,
                    args.focused,
                    mCheckedItems);
                return;
            }

            base.RowGUI(args);
        }

        void DelayedSearchChanged()
        {
            Refilter();

            Sort();

            Reload();

            TableViewOperations.ScrollToSelection(this);
        }

        void SortingChanged(MultiColumnHeader multiColumnHeader)
        {
            Sort();

            Reload();
        }

        void Sort()
        {
            bool sortAscending = multiColumnHeader.IsSortedAscending(0);

            mUsers.Sort(mUsersComparer);

            if (!sortAscending)
                mUsers.Reverse();
        }

        void Refilter()
        {
            if (string.IsNullOrEmpty(searchString))
            {
                mUsers = mUnfilteredUsers;
                return;
            }

            mUsers = new List<SecurityMember>();

            foreach (SecurityMember user in mUnfilteredUsers)
            {
                if (user.Name.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) != -1)
                    mUsers.Add(user);
            }
        }

        static void RegenerateRows(
            List<SecurityMember> users,
            TreeViewItem rootItem,
            List<TreeViewItem> rows)
        {
            ClearRows(rootItem, rows);

            if (users.Count == 0)
                return;

            for (int i = 0; i < users.Count; i++)
            {
                CollaboratorsListViewItem collaboratorsListViewItem =
                    new CollaboratorsListViewItem(i + 1, users[i]);

                rootItem.AddChild(collaboratorsListViewItem);
                rows.Add(collaboratorsListViewItem);
            }
        }

        static void ClearRows(
            TreeViewItem rootItem,
            List<TreeViewItem> rows)
        {
            if (rootItem.hasChildren)
                rootItem.children.Clear();

            rows.Clear();
        }

        static void CollaboratorListViewItemGUI(
            Rect rowRect,
            float rowHeight,
            CollaboratorsListViewItem item,
            Action avatarLoadedAction,
            bool isSelected,
            bool isFocused,
            HashSet<CollaboratorsListViewItem> checkedItems)
        {
            bool wasChecked = checkedItems.Any(checkedItem => checkedItem.User.Name == item.User.Name);
            bool isChecked = DrawTreeViewItem.ForCheckableItemCell(
                rowRect,
                rowHeight,
                0,
                GetAvatar.ForEmail(item.User.Name, avatarLoadedAction),
                null,
                item.User.Name,
                isSelected,
                isFocused,
                false,
                wasChecked);

            if (wasChecked != isChecked)
                UpdateCheckedState(checkedItems, item, isChecked);
        }

        static void UpdateCheckedState(
            HashSet<CollaboratorsListViewItem> checkedItems,
            CollaboratorsListViewItem item,
            bool isChecked)
        {
            if (isChecked)
            {
                checkedItems.Add(item);
                return;
            }

            checkedItems.RemoveWhere(checkedItem => checkedItem.User.Name == item.User.Name);
        }

        bool mbShouldFilterOnServer = false;
        List<SecurityMember> mUnfilteredUsers = new List<SecurityMember>();
        List<SecurityMember> mUsers = new List<SecurityMember>();
        HashSet<CollaboratorsListViewItem> mCheckedItems = new HashSet<CollaboratorsListViewItem>();
        DelayedActionBySecondsRunner mDelayedFilterAction;

        readonly EmptyStatePanel mEmptyStatePanel;
        readonly IComparer<SecurityMember> mUsersComparer;
        readonly IProgressControls mProgressControls;
    }
}
