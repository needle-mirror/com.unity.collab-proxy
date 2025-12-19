using System;

using UnityEditor;
using UnityEngine;

using Codice.CM.Common;
using Codice.Utils;
using PlasticGui;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Avatar;

namespace Unity.PlasticSCM.Editor.Views.Properties
{
    internal class PropertiesPanel
    {
        internal PropertiesPanel(
            Action repaintAction,
            bool expandCommentsHeight = false)
        {
            mRepaintAction = repaintAction;

            mCommentsPanel = new CommentsPanel(
                repaintAction,
                expandCommentsHeight);
        }

        internal void ClearInfo()
        {
            mCommentsPanel.ClearInfo();

            mSelectedObject = null;
            mRepSpec = null;

            mRepaintAction();
        }

        internal void UpdateInfo(
            RepObjectInfo selectedObject,
            RepositorySpec repSpec)
        {
            mCommentsPanel.ClearInfo();

            mSelectedObject = selectedObject;
            mRepSpec = repSpec;

            mRepaintAction();
        }

        internal void OnGUI()
        {
            if (mSelectedObject == null || mRepSpec == null)
                return;

            EditorGUILayout.BeginVertical(UnityStyles.ToolbarBackground);

            GUILayout.Space(5);

            Texture image = GetImage(mSelectedObject, mRepSpec, () =>
            {
                mRepaintAction();
            });

            string title = GetTitle(mSelectedObject);
            string description = GetDescription(mSelectedObject, mRepSpec);
            mCommentsPanel.SetComment(GetComment(mSelectedObject));

            DrawPropertiesArea(
                image,
                title,
                description);

            mCommentsPanel.OnGUI();

            EditorGUILayout.EndVertical();
        }

        static void DrawPropertiesArea(
            Texture image,
            string title,
            string description)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Space(5);

            DrawImage(image);

            GUILayout.Space(1);

            GUILayout.BeginVertical();

            // Get the available width
            Rect availableWidthRect = GUILayoutUtility.GetRect(
                0,
                0,
                GUILayout.ExpandWidth(true));

            DrawSelectableLabel(
                title,
                UnityStyles.PropertiesPanel.Title,
                availableWidthRect.width);

            DrawSelectableLabel(
                description,
                UnityStyles.PropertiesPanel.Description,
                availableWidthRect.width);

            GUILayout.Space(3);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        static void DrawImage(Texture image)
        {
            if (image == null)
                return;

            Rect imageRect = GUILayoutUtility.GetRect(
                IMAGE_SIZE, IMAGE_SIZE,
                GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

            imageRect.y += 3;

            GUI.DrawTexture(imageRect, image, ScaleMode.ScaleToFit);
        }

        static void DrawSelectableLabel(string text, GUIStyle style, float availableWidth)
        {
            Vector2 objectNameContentSize = style.CalcSize(new GUIContent(text));
            float objectNameClampedWidth = Mathf.Min(objectNameContentSize.x, availableWidth);
            Rect objectNameRect = GUILayoutUtility.GetRect(
                objectNameClampedWidth,
                objectNameContentSize.y,
                style);
            EditorGUI.SelectableLabel(objectNameRect, text, style);
        }

        static Texture GetImage(
            RepObjectInfo selectedObject,
            RepositorySpec repSpec,
            Action avatarLoadedAction)
        {
            if (selectedObject is ChangesetInfo)
            {
                string userName = PlasticGui.Plastic.API.GetUserName(
                    repSpec.Server, selectedObject.Owner);

                Texture2D image = GetAvatar.ForEmail(userName, avatarLoadedAction);
                return image;
            }

            if (selectedObject is BranchInfo)
                return Images.GetBranchIcon();

            if (selectedObject is MarkerInfo)
                return Images.GetLabelIcon();

            return null;
        }

        static string GetTitle(RepObjectInfo selectedObject)
        {
            if (selectedObject is ChangesetInfo changeset)
            {
                string objectName = changeset.ChangesetId < 0 ?
                    PlasticLocalization.Name.Shelve.GetString() :
                    PlasticLocalization.Name.Changeset.GetString();

                return string.Concat(
                    objectName,
                    " ",
                    Math.Abs(changeset.ChangesetId));
            }

            if (selectedObject is BranchInfo branch)
            {
                return string.Concat(
                    PlasticLocalization.Name.Branch.GetString(),
                    " ",
                    branch.Name);
            }

            if (selectedObject is MarkerInfo marker)
            {
                return string.Concat(
                    PlasticLocalization.Name.Label.GetString(),
                    " ",
                    marker.Name);
            }

            return null;
        }

        static string GetDescription(
            RepObjectInfo selectedObject,
            RepositorySpec repSpec)
        {
            string userName = PlasticGui.Plastic.API.GetUserName(
                repSpec.Server, selectedObject.Owner);

            string result = PlasticLocalization.Name.DiffHeaderDetailsStringFormat.GetString(
                userName,
                selectedObject.LocalTimeStamp);

            if (selectedObject is ChangesetInfo changesetInfo)
                result += " | " + ShortGuid.Get(changesetInfo.GUID.ToString());

            return result;
        }

        static string GetComment(RepObjectInfo selectedObject)
        {
            return selectedObject.Comment;
        }

        readonly CommentsPanel mCommentsPanel;
        readonly Action mRepaintAction;

        RepObjectInfo mSelectedObject;
        RepositorySpec mRepSpec;

        const int IMAGE_SIZE = 26;
    }
}
