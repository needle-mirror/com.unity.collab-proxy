using System;

using UnityEditor;
using UnityEngine;

using PlasticGui;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor.Views.Properties
{
    internal class CommentsPanel
    {
        internal CommentsPanel(
            Action repaintAction,
            bool expandCommentsHeight)
        {
            mRepaintAction = repaintAction;
            mExpandCommentsHeight = expandCommentsHeight;
        }

        internal void SetComment(string comment)
        {
            mComment = comment;
        }

        internal void ClearInfo()
        {
            mNeedsCommentScroll = false;
            mCommentScrollPosition = Vector2.zero;
            mCommentTextHeight = 0f;
            mViewAvailableWidth = 0f;
            mComment = null;
        }

        internal void OnGUI()
        {
            if (mComment == null)
                return;

            GUIContent commentContent = new GUIContent(
                mComment == string.Empty ?
                    PlasticLocalization.Name.NoCommentSet.GetString() :
                    mComment);

            // Get the available width
            Rect availableWidthRect = GUILayoutUtility.GetRect(
                0,
                0,
                GUILayout.ExpandWidth(true));

            GUIStyle commentsStyle = mComment == string.Empty
                ? UnityStyles.PropertiesPanel.EmptyComment
                : UnityStyles.PropertiesPanel.Comment;

            bool needsScroll = mNeedsCommentScroll;

            // Max height is 4 lines of text
            float maxCommentHeight = commentsStyle.lineHeight * 4;

            // calculate the text height only if we have a valid width
            // so the layout pass has already happened
            if (Event.current.type == EventType.Repaint && availableWidthRect.width > 1)
            {
                mViewAvailableWidth = availableWidthRect.width;
                mCommentTextHeight = commentsStyle.CalcHeight(
                    commentContent,
                    mViewAvailableWidth);

                needsScroll = mCommentTextHeight >= maxCommentHeight ||
                              mExpandCommentsHeight;
            }

            // Use cached height, or maxHeight as fallback
            float scrollViewHeight = Mathf.Min(
                mCommentTextHeight > 0 ? mCommentTextHeight : maxCommentHeight,
                maxCommentHeight);

            if (mNeedsCommentScroll)
            {
                mCommentScrollPosition = GUILayout.BeginScrollView(
                    mCommentScrollPosition,
                    mExpandCommentsHeight ?
                        GUILayout.ExpandHeight(true) :
                        GUILayout.Height(scrollViewHeight));
            }

            Rect textRect = GUILayoutUtility.GetRect(
                commentContent,
                commentsStyle,
                GUILayout.ExpandHeight(true));

            EditorGUI.SelectableLabel(textRect, commentContent.text, commentsStyle);

            if (mNeedsCommentScroll)
                GUILayout.EndScrollView();
            else
                GUILayout.Space(3);

            if (mNeedsCommentScroll != needsScroll)
            {
                mNeedsCommentScroll = needsScroll;
                mRepaintAction();
            }
        }

        readonly Action mRepaintAction;
        readonly bool mExpandCommentsHeight;

        string mComment;
        Vector2 mCommentScrollPosition = Vector2.zero;

        bool mNeedsCommentScroll;
        float mCommentTextHeight = 0f;
        float mViewAvailableWidth = 0f;
    }
}
