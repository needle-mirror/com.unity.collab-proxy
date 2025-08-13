using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
#if UNITY_6000_2_OR_NEWER
using TreeView = UnityEditor.IMGUI.Controls.TreeView<int>;
#endif

namespace Unity.PlasticSCM.Editor.UI
{
    // Assumption: Members are called from an OnGUI method ( otherwise style composition will fail)
    internal static class UnityStyles
    {
        internal static void Initialize(
            Action uvcsWindowRepaint)
        {
            mUVCSWindowRepaint = uvcsWindowRepaint;

            mLazyBackgroundStyles.Add(WarningMessage);
            mLazyBackgroundStyles.Add(SplitterIndicator);
            mLazyBackgroundStyles.Add(UVCSWindow.ActiveTabUnderline);
            mLazyBackgroundStyles.Add(Notification.GreenNotification);
            mLazyBackgroundStyles.Add(Notification.RedNotification);
            mLazyBackgroundStyles.Add(CancelButton);
            mLazyBackgroundStyles.Add(Inspector.HeaderBackgroundStyle);
            mLazyBackgroundStyles.Add(Inspector.DisabledHeaderBackgroundStyle);
        }

        internal static class Colors
        {
            internal static Color Transparent = new Color(255f / 255, 255f / 255, 255f / 255, 0f / 255);
            internal static Color GreenBackground = new Color(34f / 255, 161f / 255, 63f / 255);

            internal static Color DefaultText = EditorGUIUtility.isProSkin ?
                new Color(210f / 255, 210f / 255, 210f / 255) :
                new Color(9f / 255, 9f / 255, 9f / 255);
            internal static Color GreenText = new Color(0f / 255, 100f / 255, 0f / 255);
            internal static Color Red = new Color(194f / 255, 51f / 255, 62f / 255);
            internal static Color Warning = new Color(255f / 255, 255f / 255, 176f / 255);
            internal static Color Splitter = new Color(100f / 255, 100f / 255, 100f / 255);
            internal static Color BarBorder = EditorGUIUtility.isProSkin ?
                (Color)new Color32(35, 35, 35, 255) :
                (Color)new Color32(153, 153, 153, 255);
            internal static Color DarkGray = new Color(88f / 255, 88f / 255, 88f / 255);

            internal static Color InspectorHeaderBackground = Transparent;

            internal static Color InspectorHeaderBackgroundDisabled = EditorGUIUtility.isProSkin ?
                new Color(58f / 255, 58f / 255, 58f / 255) :
                new Color(199f / 255, 199f / 255, 199f / 255);

            internal static Color TabUnderline = new Color(58f / 255, 121f / 255, 187f / 255);
            internal static Color Link = new Color(76f / 255, 126f / 255, 255f / 255);
            internal static Color SecondaryLabel = EditorGUIUtility.isProSkin ?
                new Color(165f / 255, 165f / 255, 165f / 255) :
                new Color(70f / 255, 70f / 255, 70f / 255);
            internal static Color BackgroundBar = EditorGUIUtility.isProSkin ?
                new Color(35f / 255, 35f / 255, 35f / 255) :
                new Color(160f / 255, 160f / 255, 160f / 255);

            internal static Color TreeViewBackground = EditorGUIUtility.isProSkin ?
               new Color(48f / 255, 48f / 255, 48f / 255) :
               new Color(194f / 255, 194f / 255, 194f / 255);

            internal static Color CommentsBackground = EditorGUIUtility.isProSkin ?
               new Color(60f / 255, 60f / 255, 60f / 255) :
               new Color(160f / 255, 160f / 255, 160f / 255);

            internal static Color ColumnsBackground = EditorGUIUtility.isProSkin ?
              new Color(56f / 255, 56f / 255, 56f / 255) :
              new Color(221f / 255, 221f / 255, 221f / 255);

            internal static Color BackgroundLighter = EditorGUIUtility.isProSkin ?
                new Color(51f / 255, 51f / 255, 51f / 255) :
                new Color(190f / 255, 190f / 255, 190f / 255);

            internal static Color FooterBarBackground = EditorGUIUtility.isProSkin ?
                new Color(64f / 255, 64f / 255, 64f / 255) :
                new Color(207f / 255, 207f / 255, 207f / 255);

            internal static Color SelectedUnfocusedTextBackground = EditorGUIUtility.isProSkin ?
                new Color(77f / 255, 77f / 255, 77f / 255) :
                new Color(174f / 255, 174f / 255, 174f / 255);

            internal static Color SelectedFocusedTextBackground = EditorGUIUtility.isProSkin ?
                new Color(44f / 255, 93f / 255, 135f / 255) :
                new Color(58f / 255, 114f / 255, 176f / 255);

            internal static Color SelectedFocusedDropLineBackground =
                new Color(55f / 255, 82f / 255, 204f / 255);

            internal static Color SelectedFocusedDropLabelBackground = EditorGUIUtility.isProSkin ?
                new Color(255f / 255, 255f / 255, 255f / 255, 0f / 255) :
                new Color(194f / 255, 212f / 255, 247f / 255);

            internal static Color LineSelectionText =
                new Color(255f / 255, 255f / 255, 255f / 255);

            internal static Color SelectedIcon = new Color(0.85f, 0.9f, 1f);

            internal static Color ToggleOffText = EditorGUIUtility.isProSkin ?
                new Color(131f / 255, 131f / 255, 131f / 255) :
                new Color(151f / 255, 151f / 255, 151f / 255);

            internal static Color ToggleHoverText = EditorGUIUtility.isProSkin ?
                new Color(129f / 255, 180f / 255, 255f / 255) :
                new Color(7f / 255, 68f / 255, 146f / 255);

            // see EditorGUI.kSplitLineSkinnedColor
            internal static Color SplitLineColor = EditorGUIUtility.isProSkin ?
                new Color(0.12f, 0.12f, 0.12f, 1.333f) :
                new Color(0.6f, 0.6f, 0.6f, 1.333f);
        }

        internal static class Dialog
        {
            internal static readonly LazyStyle MessageTitle = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.boldLabel);
                style.contentOffset = new Vector2(0, -5);
                style.wordWrap = true;
                style.fontSize = MODAL_FONT_SIZE + 1;
                return style;
            });

            internal static readonly LazyStyle SectionTitle = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.boldLabel);
                style.contentOffset = new Vector2(0, -5);
                style.wordWrap = true;
                style.fontSize = MODAL_FONT_SIZE;
                return style;
            });

            internal static readonly LazyStyle MessageText = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.wordWrap = true;
                style.fontSize = MODAL_FONT_SIZE;
                return style;
            });

            internal static readonly LazyStyle BoldText = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.wordWrap = true;
                style.fontSize = MODAL_FONT_SIZE;
                style.fontStyle = FontStyle.Bold;
                return style;
            });

            internal static readonly LazyStyle Title = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.boldLabel);
                style.fontSize = MODAL_FONT_SIZE;
                style.clipping = TextClipping.Overflow;
                return style;
            });

            internal static readonly LazyStyle CheckBox = new LazyStyle(() =>
            {
                var radioToggleStyle = new GUIStyle(EditorStyles.toggle);
                radioToggleStyle.fontSize = MODAL_FONT_SIZE - 1;
                radioToggleStyle.clipping = TextClipping.Overflow;
                radioToggleStyle.contentOffset = new Vector2(5, 0);
                return radioToggleStyle;
            });

            internal static readonly LazyStyle RadioToggle = new LazyStyle(() =>
            {
                var radioToggleStyle = new GUIStyle(EditorStyles.radioButton);
                radioToggleStyle.fontSize = MODAL_FONT_SIZE - 1;
                radioToggleStyle.clipping = TextClipping.Overflow;
                radioToggleStyle.contentOffset = new Vector2(5, -2);
                return radioToggleStyle;
            });

            internal static readonly LazyStyle BoldRadioToggle = new LazyStyle(() =>
            {
                var radioToggleStyle = new GUIStyle(EditorStyles.radioButton);
                radioToggleStyle.fontSize = MODAL_FONT_SIZE;
                radioToggleStyle.fontStyle = FontStyle.Bold;
                radioToggleStyle.clipping = TextClipping.Overflow;
                radioToggleStyle.contentOffset = new Vector2(5, -2);
                return radioToggleStyle;
            });

            internal static readonly LazyStyle Foldout = new LazyStyle(() =>
            {
                GUIStyle paragraphStyle = Paragraph;
                var foldoutStyle = new GUIStyle(EditorStyles.foldout);
                foldoutStyle.fontSize = MODAL_FONT_SIZE;
                foldoutStyle.font = paragraphStyle.font;
                return foldoutStyle;
            });

            internal static readonly LazyStyle EntryLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.textField);
                style.wordWrap = true;
                style.fontSize = MODAL_FONT_SIZE;
                return style;
            });

            internal static readonly LazyStyle AcceptButtonText = new LazyStyle(() =>
            {
                var style = new GUIStyle(GetEditorSkin().GetStyle("WhiteLabel"));
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = MODAL_FONT_SIZE + 1;
                style.normal.background = null;
                return style;
            });

            internal static readonly LazyStyle NormalButton = new LazyStyle(() =>
            {
                var style = new GUIStyle(GetEditorSkin().button);
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = MODAL_FONT_SIZE;
                return style;
            });

            internal static readonly LazyStyle FlatButton = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.miniButton);
                style.alignment = TextAnchor.MiddleCenter;
                style.fixedWidth = 22;
                style.fixedHeight = 22;
                style.margin = new RectOffset(10, 0, 0, 0);
                style.padding = new RectOffset(0, 0, 0, 0);
                return style;
            });

            internal static readonly LazyStyle ParagraphForMultiLinkLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(Paragraph);
                style.margin = new RectOffset(0, 0, style.margin.top, style.margin.bottom);
                style.padding = new RectOffset(0, 0, style.padding.top, style.padding.bottom);
                return style;
            });

            internal static readonly LazyStyle LinkForMultiLinkLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(MultiLinkLabel);
                style.fontSize = ((GUIStyle)Paragraph).fontSize;
                return style;
            });
        }

        internal static class Tree
        {
            internal static readonly LazyStyle IconStyle = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.largeLabel);
                style.alignment = TextAnchor.MiddleLeft;
                return style;
            });

            internal static readonly LazyStyle Label = new LazyStyle(() =>
            {
                var style = new GUIStyle(TreeView.DefaultStyles.label);
                style.fontSize = UnityConstants.LABEL_FONT_SIZE;
                style.alignment = TextAnchor.MiddleLeft;
                return style;
            });

            internal static readonly LazyStyle SecondaryLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(TreeView.DefaultStyles.label);
                style.fontSize = UnityConstants.LABEL_FONT_SIZE;
                style.alignment = TextAnchor.MiddleLeft;

                style.active = new GUIStyleState() { textColor = Colors.SecondaryLabel };
                style.focused = new GUIStyleState() { textColor = Colors.SecondaryLabel };
                style.hover = new GUIStyleState() { textColor = Colors.SecondaryLabel };
                style.normal = new GUIStyleState() { textColor = Colors.SecondaryLabel };

                return style;
            });

            internal static readonly LazyStyle InfoLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(MultiColumnHeader.DefaultStyles.columnHeader);
                return style;
            });

            internal static readonly LazyStyle SecondaryBoldLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(SecondaryLabel);
                style.fontStyle = FontStyle.Bold;
                return style;
            });

            internal static readonly LazyStyle RedLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(Label);
                style.active = new GUIStyleState() { textColor = Colors.Red };
                style.focused = new GUIStyleState() { textColor = Colors.Red };
                style.hover = new GUIStyleState() { textColor = Colors.Red };
                style.normal = new GUIStyleState() { textColor = Colors.Red };
                return style;
            });

            internal static readonly LazyStyle GreenLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(Label);
                style.active = new GUIStyleState() { textColor = Colors.GreenText };
                style.focused = new GUIStyleState() { textColor = Colors.GreenText };
                style.hover = new GUIStyleState() { textColor = Colors.GreenText };
                style.normal = new GUIStyleState() { textColor = Colors.GreenText };
                return style;
            });

            internal static readonly LazyStyle BoldLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(TreeView.DefaultStyles.boldLabel);
                style.fontSize = UnityConstants.LABEL_FONT_SIZE;
                style.alignment = TextAnchor.MiddleLeft;
                return style;
            });

            internal static readonly LazyStyle LabelRightAligned = new LazyStyle(() =>
            {
                var style = new GUIStyle(TreeView.DefaultStyles.label);
                style.fontSize = UnityConstants.LABEL_FONT_SIZE;
                style.alignment = TextAnchor.MiddleRight;
                return style;
            });

            internal static readonly LazyStyle SecondaryLabelRightAligned = new LazyStyle(() =>
            {
                var style = new GUIStyle(SecondaryLabel);
                style.alignment = TextAnchor.MiddleRight;
                return style;
            });

            internal static readonly LazyStyle SecondaryLabelBoldRightAligned = new LazyStyle(() =>
            {
                var style = new GUIStyle(SecondaryLabelRightAligned);
                style.fontStyle = FontStyle.Bold;
                return style;
            });

            internal static readonly LazyStyle Columns = new LazyStyle(() =>
            {
                var style = new GUIStyle();
                style.normal.background = Images.GetColumnsBackgroundTexture();
                return style;
            });
        }

        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static class Inspector
        {
            // Internal usage. This isn't a public API.
            [EditorBrowsable(EditorBrowsableState.Never)]
            public static readonly LazyStyle HeaderBackgroundStyle = new LazyStyle(() =>
            {
                return CreateUnderlineStyle(
                    Colors.InspectorHeaderBackground,
                    UnityConstants.INSPECTOR_ACTIONS_HEADER_BACK_RECTANGLE_HEIGHT);
            });

            // Internal usage. This isn't a public API.
            [EditorBrowsable(EditorBrowsableState.Never)]
            public static readonly LazyStyle DisabledHeaderBackgroundStyle = new LazyStyle(() =>
            {
                return CreateUnderlineStyle(
                    Colors.InspectorHeaderBackgroundDisabled,
                    UnityConstants.INSPECTOR_ACTIONS_HEADER_BACK_RECTANGLE_HEIGHT);
            });
        }

        internal static class ProjectSettings
        {
            internal static readonly LazyStyle ToggleOn = new LazyStyle(() =>
            {
                GUIStyle result = new GUIStyle(Toggle);
                result.hover.textColor = Colors.ToggleHoverText;
                return result;
            });

            internal static readonly LazyStyle FoldoutHeader = new LazyStyle(() =>
            {
                GUIStyle result = new GUIStyle(EditorStyles.foldoutHeader);
                result.fontStyle = FontStyle.Bold;
                result.fontSize = MODAL_FONT_SIZE;
                return result;
            });

            internal static readonly LazyStyle SectionTitle = new LazyStyle(() =>
            {
                GUIStyle result = new GUIStyle(EditorStyles.label);
                result.fontStyle = FontStyle.Bold;
                result.fontSize = MODAL_FONT_SIZE;
                return result;
            });

            internal static readonly LazyStyle Title = new LazyStyle(() =>
            {
                GUIStyle result = new GUIStyle(EditorStyles.label);
                result.fontStyle = FontStyle.Bold;
                result.fontSize = MODAL_FONT_SIZE - 1;
                return result;
            });

            internal static readonly LazyStyle Paragraph = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.wordWrap = true;
                style.richText = true;
                style.fontSize = MODAL_FONT_SIZE - 1;
                return style;
            });

            static readonly LazyStyle Toggle = new LazyStyle(() =>
            {
                GUIStyle result = new GUIStyle(EditorStyles.miniButton);
                result.fixedHeight = 22;
                result.fixedWidth = 85;
                result.fontSize = 12;
                return result;
            });
        }

        internal static class UVCSWindow
        {
            internal static readonly LazyStyle TabButton = new LazyStyle(() =>
            {
                GUIStyle result = new GUIStyle(EditorStyles.label);
                result.padding = EditorStyles.toolbarButton.padding;
                result.margin = EditorStyles.toolbarButton.margin;
                result.contentOffset = EditorStyles.toolbarButton.contentOffset;
                result.alignment = EditorStyles.toolbarButton.alignment;
                result.fixedHeight = EditorStyles.toolbarButton.fixedHeight;
                return result;
            });

            internal static readonly LazyStyle ActiveTabUnderline = new LazyStyle(() =>
            {
                return CreateUnderlineStyle(
                    Colors.TabUnderline,
                    UnityConstants.ACTIVE_TAB_UNDERLINE_HEIGHT);
            });
        }

        internal static class StatusBar
        {
            internal static readonly LazyStyle Bar = new LazyStyle(() =>
            {
                var style = new GUIStyle();

                var bg = new Texture2D(1, 1);
                bg.SetPixel(0, 0, Colors.BackgroundBar);
                bg.Apply();
                style.normal.background = bg;
                return style;
            });
            internal static readonly LazyStyle Icon = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.padding.left = 0;
                style.padding.right = 0;
                return style;
            });

            internal static readonly LazyStyle Label = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                return style;
            });

            internal static readonly LazyStyle NotificationLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.fontStyle = FontStyle.Bold;
                return style;
            });

            internal static readonly LazyStyle NotificationLabelForMultiLinkLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(NotificationLabel);
                style.margin = new RectOffset(0, 0, style.margin.top, style.margin.bottom);
                style.padding = new RectOffset(0, 0, style.padding.top, style.padding.bottom);
                return style;
            });

            internal static readonly LazyStyle LinkForMultiLinkLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(MultiLinkLabel);
                style.fontSize = ((GUIStyle)NotificationLabel).fontSize;
                style.fontStyle = ((GUIStyle)NotificationLabel).fontStyle;
                style.margin = new RectOffset(0, 0, 0, 0);
                style.padding = new RectOffset(0, 0, 0, 0);
                style.contentOffset = new Vector2(0, 2);
                return style;
            });

            internal static readonly LazyStyle Button = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.miniButtonLeft);
                style.fixedWidth = 60;
                return style;
            });

            internal static readonly LazyStyle NotificationPanel = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.helpBox);
                style.fixedHeight = 24;
                return style;
            });

            internal static readonly LazyStyle NotificationPanelCloseButton = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.fixedHeight = 16;
                style.fixedWidth = 16;
                return style;
            });

            internal static readonly LazyStyle NotificationPanelLink = new LazyStyle(() =>
            {
                var style = new GUIStyle(LinkLabel);
                style.padding = EditorStyles.label.padding;
                return style;
            });

            internal static readonly LazyStyle CopyToClipboardButton = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.miniButton);
                style.alignment = TextAnchor.MiddleCenter;
                style.fixedWidth = 28;
                style.fixedHeight = 22;
                style.margin = new RectOffset(0, 0, 0, 0);
                style.padding = new RectOffset(0, 0, 2, 2);
                return style;
            });
        }

        internal static class DiffPanel
        {
            internal static readonly LazyStyle HeaderLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.fontSize = 10;
                style.fontStyle = FontStyle.Bold;
                style.contentOffset = new Vector2(0, 1.5f);
                return style;
            });
        }

        internal static class PendingChangesTab
        {
            internal static readonly LazyStyle CommentPlaceHolder = new LazyStyle(() =>
            {
                var style = new GUIStyle();
                style.normal = new GUIStyleState() { textColor = Color.gray };
                style.padding = new RectOffset(7, 7, 4, 4);
                style.clipping = TextClipping.Clip;
                return style;
            });

            internal static readonly LazyStyle UserIcon = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.textArea);
                style.margin = new RectOffset(7, 4, 0, 0);
                style.padding = new RectOffset(0, 0, 4, 0);

                return style;
            });

            internal static readonly LazyStyle CommentTextArea = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.textArea);
                style.padding.left = 7;
                style.padding.top = 5;
                style.stretchWidth = false;
                style.stretchHeight = false;

                return style;
            });

            internal static readonly LazyStyle CommentWarningIcon = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.fontSize = 10;
                return style;
            });

            internal static readonly LazyStyle HeaderLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.fontSize = 10;
                style.fontStyle = FontStyle.Bold;
                style.contentOffset = new Vector2(0, 1.5f);
                return style;
            });

            internal static readonly LazyStyle Comment = new LazyStyle(() =>
            {
                var style = new GUIStyle();
                style.normal.background = Images.GetCommentBackgroundTexture();
                return style;
            });

            internal static readonly GUIStyle DefaultMultiColumHeader = MultiColumnHeader.DefaultStyles.background;
        }

        internal static class MergeTab
        {
            internal static readonly LazyStyle PendingConflictsLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.fontSize = 11;
                style.padding.top = 2;
                style.fontStyle = FontStyle.Bold;
                return style;
            });

            internal static readonly LazyStyle RedPendingConflictsOfTotalLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(PendingConflictsLabel);
                style.normal = new GUIStyleState() { textColor = Colors.Red };
                return style;
            });

            internal static readonly LazyStyle GreenPendingConflictsOfTotalLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(PendingConflictsLabel);
                style.normal = new GUIStyleState() { textColor = Colors.GreenText };
                return style;
            });

            internal static readonly LazyStyle TitleLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.fontSize = 12;
                style.fontStyle = FontStyle.Bold;
                return style;
            });

            internal static readonly LazyStyle InfoLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.fontSize = 11;
                style.padding.top = 2;
                return style;
            });

            internal static readonly LazyStyle LinkLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(UnityStyles.LinkLabel);
                style.fontSize = ((GUIStyle)InfoLabel).fontSize;
                style.padding.top = ((GUIStyle)InfoLabel).padding.top;
                style.padding.left = 0;
                style.stretchWidth = false;
                return style;
            });
        }

        internal static class ChangesetsTab
        {
            internal static readonly LazyStyle HeaderLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.fontSize = 10;
                style.fontStyle = FontStyle.Bold;
                style.contentOffset = new Vector2(0, 1.5f);
                return style;
            });
        }

        internal static class HistoryTab
        {
            internal static readonly LazyStyle HeaderLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.fontSize = 12;
                style.fontStyle = FontStyle.Bold;
                return style;
            });
        }

        internal static class DirectoryConflictResolution
        {
            internal readonly static LazyStyle WarningLabel
                = new LazyStyle(() =>
                {
                    var style = new GUIStyle(EditorStyles.label);
                    style.alignment = TextAnchor.MiddleLeft;
                    return style;
                });
        }

        internal static class Notification
        {
            internal static readonly LazyStyle Label = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.normal = new GUIStyleState() { textColor = Color.white };
                return style;
            });

            internal static readonly LazyStyle GreenNotification = new LazyStyle(() =>
            {
                var style = new GUIStyle();
                style.wordWrap = true;
                style.margin = new RectOffset();
                style.padding = new RectOffset(4, 4, 2, 2);
                style.stretchWidth = true;
                style.stretchHeight = true;
                style.alignment = TextAnchor.UpperLeft;

                var bg = new Texture2D(1, 1);
                bg.SetPixel(0, 0, Colors.GreenBackground);
                bg.Apply();
                style.normal.background = bg;
                return style;
            });

            internal static readonly LazyStyle RedNotification = new LazyStyle(() =>
            {
                var style = new GUIStyle();
                style.wordWrap = true;
                style.margin = new RectOffset();
                style.padding = new RectOffset(4, 4, 2, 2);
                style.stretchWidth = true;
                style.stretchHeight = true;
                style.alignment = TextAnchor.UpperLeft;

                var bg = new Texture2D(1, 1);
                bg.SetPixel(0, 0, Colors.Red);
                bg.Apply();
                style.normal.background = bg;
                return style;
            });
        }

        internal static class DirectoryConflicts
        {
            internal readonly static LazyStyle TitleLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.largeLabel);
                RectOffset margin = new RectOffset(
                    style.margin.left,
                    style.margin.right,
                    style.margin.top - 1,
                    style.margin.bottom);
                style.margin = margin;
                style.fontStyle = FontStyle.Bold;
                return style;
            });

            internal readonly static LazyStyle BoldLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.fontStyle = FontStyle.Bold;
                return style;
            });

            internal readonly static LazyStyle FileNameTextField = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.textField);
                RectOffset margin = new RectOffset(
                    style.margin.left,
                    style.margin.right,
                    style.margin.top + 2,
                    style.margin.bottom);
                style.margin = margin;
                return style;
            });
        }

        internal static class EmptyState
        {
            internal static readonly LazyStyle Label = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = UnityConstants.EMPTY_STATE_FONT_SIZE;
                style.stretchWidth = false;
                style.padding = new RectOffset(
                    UnityConstants.EMPTY_STATE_HORIZONTAL_PADDING,
                    UnityConstants.EMPTY_STATE_HORIZONTAL_PADDING,
                    UnityConstants.EMPTY_STATE_VERTICAL_PADDING,
                    UnityConstants.EMPTY_STATE_VERTICAL_PADDING);
                style.normal.textColor = Colors.DefaultText;
                style.active.textColor = Colors.DefaultText;
                style.focused.textColor = Colors.DefaultText;
                style.hover.textColor = Colors.DefaultText;

                return style;
            });

            internal static readonly LazyStyle LabelForMultiLinkLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(Label);
                style.margin = new RectOffset(0, 0, style.margin.top, style.margin.bottom);
                style.padding = new RectOffset(0, 0, style.padding.top, style.padding.bottom);
                return style;
            });

            internal static readonly LazyStyle LinkForMultiLinkLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(MultiLinkLabel);
                style.fontSize = ((GUIStyle)Label).fontSize;
                return style;
            });

            internal static readonly LazyStyle Icon = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.alignment = TextAnchor.MiddleCenter;
                style.fixedWidth = 16;
                style.fixedHeight = 16;
                style.contentOffset = new Vector2(0, 3);
                return style;
            });

            internal static readonly LazyStyle Link = new LazyStyle(() =>
            {
                var style = new GUIStyle(LinkLabel);
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = UnityConstants.EMPTY_STATE_FONT_SIZE;
                style.stretchWidth = false;
                style.padding = new RectOffset(
                    UnityConstants.EMPTY_STATE_HORIZONTAL_PADDING,
                    UnityConstants.EMPTY_STATE_HORIZONTAL_PADDING,
                    UnityConstants.EMPTY_STATE_VERTICAL_PADDING,
                    UnityConstants.EMPTY_STATE_VERTICAL_PADDING);

                return style;
            });

            internal static readonly LazyStyle CopyToClipboardButton = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.miniButton);
                style.alignment = TextAnchor.MiddleCenter;
                style.fixedWidth = 28;
                style.fixedHeight = 22;
                style.margin = new RectOffset(0, 0, 3, 0);
                style.padding = new RectOffset(0, 0, 2, 2);
                return style;
            });
        }

        internal static class EditorToolbar
        {
            internal static class Button
            {
                internal static readonly LazyStyle AppCmdButton = new LazyStyle(() =>
                {
                    GUIStyle appCommandStyle = new GUIStyle("AppCommand");
                    appCommandStyle.fixedWidth = 0;
                    appCommandStyle.fixedHeight = appCommandStyle.fixedHeight - 1;
                    appCommandStyle.imagePosition = ImagePosition.TextOnly;
                    appCommandStyle.padding = new RectOffset(0, 0, 0, 0);
                    appCommandStyle.margin = new RectOffset(5, 2, 0, 0);
                    return appCommandStyle;
                });

                internal static readonly LazyStyle ButtonText = new LazyStyle(() =>
                {
                    GUIStyle appCommandStyle = new GUIStyle("AppCommand");

                    GUIStyle textStyle = new GUIStyle(EditorStyles.label);
                    textStyle.font = textStyle.font;
                    textStyle.normal.textColor = appCommandStyle.normal.textColor;
                    textStyle.border = appCommandStyle.border;
                    textStyle.margin = new RectOffset(0, 0, 0, 0);
                    textStyle.padding = appCommandStyle.padding;
                    textStyle.alignment = TextAnchor.MiddleCenter;
                    textStyle.fontSize = appCommandStyle.fontSize;
                    textStyle.fontStyle = appCommandStyle.fontStyle;

                    return textStyle;
                });
            }

            internal static class Popup
            {
                const int PADDING = 8;

                internal static readonly LazyStyle Hover = new LazyStyle(() =>
                {
                    GUIStyle menuItemStyle = new GUIStyle("MenuItem");
                    menuItemStyle.fixedHeight = 0;
                    return menuItemStyle;
                });

                internal static readonly LazyStyle Label = new LazyStyle(() =>
                {
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
                    labelStyle.padding = new RectOffset(PADDING, PADDING, 0, 0);
                    return labelStyle;
                });

                internal static readonly LazyStyle Shortcut = new LazyStyle(() =>
                {
                    GUIStyle shortcutStyle = new GUIStyle(EditorStyles.label);
                    shortcutStyle.padding = new RectOffset(PADDING, PADDING, 0, 0);
                    shortcutStyle.alignment = TextAnchor.MiddleRight;
                    shortcutStyle.normal.textColor = new Color(
                        shortcutStyle.normal.textColor.r,
                        shortcutStyle.normal.textColor.g,
                        shortcutStyle.normal.textColor.b,
                        0.6f);
                    return shortcutStyle;
                });

                internal static readonly LazyStyle LabelHover = new LazyStyle(() =>
                {
                    GUIStyle menuItemStyle = new GUIStyle(Hover);

                    GUIStyle labelStyleHighlight = new GUIStyle(Label);
                    labelStyleHighlight.normal.textColor = menuItemStyle.hover.textColor;
                    labelStyleHighlight.hover.textColor = menuItemStyle.hover.textColor;

                    return labelStyleHighlight;
                });

                internal static readonly LazyStyle ShortcutHover = new LazyStyle(() =>
                {
                    GUIStyle menuItemStyle = new GUIStyle(Hover);

                    GUIStyle shortcutStyleHighlight = new GUIStyle(Shortcut);
                    shortcutStyleHighlight.normal.textColor = menuItemStyle.hover.textColor;
                    shortcutStyleHighlight.hover.textColor = menuItemStyle.hover.textColor;

                    return shortcutStyleHighlight;
                });

                internal static readonly LazyStyle SearchField = new LazyStyle(() =>
                {
                    GUIStyle searchFieldStyle = new GUIStyle("SearchTextField");
                    searchFieldStyle.fixedHeight = 19;
                    return searchFieldStyle;
                });

                internal static class BranchesList
                {
                    internal static readonly LazyStyle Category = new LazyStyle(() =>
                    {
                        GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.boldLabel);
                        boldLabelStyle.padding = new RectOffset(PADDING, 0, 0, 0);
                        return boldLabelStyle;
                    });

                    internal static readonly LazyStyle Title = new LazyStyle(() =>
                    {
                        GUIStyle titleLabelStyle = new GUIStyle(EditorStyles.label);
                        titleLabelStyle.normal.textColor = Colors.DefaultText;
                        titleLabelStyle.padding = new RectOffset(PADDING, 0, 0, 1);
                        titleLabelStyle.richText = true;
                        return titleLabelStyle;
                    });

                    internal static readonly LazyStyle TitleHover = new LazyStyle(() =>
                    {
                        GUIStyle hoverStyle = new GUIStyle(Hover);

                        GUIStyle titleLabelHoverStyle = new GUIStyle(Title);
                        titleLabelHoverStyle.normal.textColor = hoverStyle.hover.textColor;
                        titleLabelHoverStyle.hover.textColor = hoverStyle.hover.textColor;
                        return titleLabelHoverStyle;
                    });

                    internal static readonly LazyStyle Description = new LazyStyle(() =>
                    {
                        GUIStyle descriptionLabelStyle = new GUIStyle(EditorStyles.miniLabel);
                        descriptionLabelStyle.normal.textColor = Colors.SecondaryLabel;
                        descriptionLabelStyle.padding = new RectOffset(PADDING, 0, 0, 0);
                        descriptionLabelStyle.richText = true;
                        return descriptionLabelStyle;
                    });

                    internal static readonly LazyStyle DescriptionHover = new LazyStyle(() =>
                    {
                        GUIStyle hoverStyle = new GUIStyle(Hover);

                        GUIStyle descriptionLabelHoverStyle = new GUIStyle(Description);
                        descriptionLabelHoverStyle.normal.textColor = hoverStyle.hover.textColor;
                        descriptionLabelHoverStyle.hover.textColor = hoverStyle.hover.textColor;

                        return descriptionLabelHoverStyle;
                    });

                    internal static readonly LazyStyle TimeAgo = new LazyStyle(() =>
                    {
                        GUIStyle timeAgoLabelStyle = new GUIStyle(EditorStyles.miniLabel);
                        timeAgoLabelStyle.normal.textColor = Colors.SecondaryLabel;
                        timeAgoLabelStyle.padding = new RectOffset(PADDING, PADDING, 0, 0);
                        timeAgoLabelStyle.richText = true;
                        return timeAgoLabelStyle;
                    });

                    internal static readonly LazyStyle TimeAgoHover = new LazyStyle(() =>
                    {
                        GUIStyle hoverStyle = new GUIStyle(Hover);

                        GUIStyle timeAgoLabelHoverStyle = new GUIStyle(TimeAgo);
                        timeAgoLabelHoverStyle.normal.textColor = hoverStyle.hover.textColor;
                        timeAgoLabelHoverStyle.hover.textColor = hoverStyle.hover.textColor;

                        return timeAgoLabelHoverStyle;
                    });
                }
            }
        }

        internal static class CloudDrive
        {

            internal static readonly LazyStyle ItemsListLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.miniLabel);
                style.alignment = TextAnchor.MiddleCenter;
                return style;
            });

            internal static readonly LazyStyle ItemsListLabelFocused = new LazyStyle(() =>
            {
                var style = new GUIStyle(ItemsListLabel);
                style.normal.textColor = Colors.LineSelectionText;
                return style;
            });

            internal static readonly LazyStyle ItemNameBarLabel = new LazyStyle(() =>
            {
                var style = new GUIStyle(EditorStyles.label);
                style.padding = new RectOffset(2, 2, 2, 2);
                return style;
            });
        }

        internal static readonly LazyStyle ActionToolbar = new LazyStyle(() =>
        {
            var style = new GUIStyle(EditorStyles.toolbar);
            style.fixedHeight = 40f;
            style.padding = new RectOffset(5, 5, 5, 5);
            return style;
        });

        internal static readonly LazyStyle SplitterIndicator = new LazyStyle(() =>
        {
            return CreateUnderlineStyle(
                Colors.Splitter,
                UnityConstants.SPLITTER_INDICATOR_HEIGHT);
        });

        internal static readonly LazyStyle HelpBoxLabel = new LazyStyle(() =>
        {
            var style = new GUIStyle(EditorStyles.label);
            style.fontSize = 10;
            style.wordWrap = true;
            return style;
        });

        internal static readonly LazyStyle HeaderWarningLabel
            = new LazyStyle(() =>
        {
            var style = new GUIStyle(EditorStyles.label);
            style.fontSize = 11;
            return style;
        });

        internal static readonly LazyStyle ProgressLabel = new LazyStyle(() =>
        {
            var style = new GUIStyle(EditorStyles.label);
            style.fontSize = 10;
            return style;
        });

        internal static readonly LazyStyle TextFieldWithWrapping = new LazyStyle(() =>
        {
            var style = new GUIStyle(GetEditorSkin().textArea);
            style.normal = new GUIStyleState() {
                textColor = GetEditorSkin().textArea.normal.textColor,
                background = Images.GetTreeviewBackgroundTexture()
            };

            style.wordWrap = true;
            return style;
        });

        internal static readonly LazyStyle WarningMessage = new LazyStyle(() =>
        {
            var style = new GUIStyle(GetEditorSkin().box);
            style.wordWrap = true;
            style.margin = new RectOffset();
            style.padding = new RectOffset(8, 8, 6, 6);
            style.stretchWidth = true;
            style.alignment = TextAnchor.UpperLeft;

            var bg = new Texture2D(1, 1);
            bg.SetPixel(0, 0, Colors.Warning);
            bg.Apply();
            style.normal.background = bg;
            return style;
        });

        internal static readonly LazyStyle CancelButton = new LazyStyle(() =>
        {
            var normalIcon = Images.GetImage(Images.Name.IconCloseButton);
            var pressedIcon = Images.GetImage(Images.Name.IconPressedCloseButton);

            var style = new GUIStyle();
            style.normal = new GUIStyleState() { background = normalIcon };
            style.onActive = new GUIStyleState() { background = pressedIcon };
            style.active = new GUIStyleState() { background = pressedIcon };
            return style;
        });

        internal static readonly LazyStyle MiniToggle = new LazyStyle(() =>
        {
            var style = new GUIStyle(EditorStyles.boldLabel);
            style.fontSize = MODAL_FONT_SIZE - 1;
            style.clipping = TextClipping.Overflow;
            return style;
        });

        internal static readonly LazyStyle Paragraph = new LazyStyle(() =>
        {
            var style = new GUIStyle(EditorStyles.largeLabel);
            style.wordWrap = true;
            style.richText = true;
            style.fontSize = MODAL_FONT_SIZE;
            return style;
        });

        internal static readonly LazyStyle LinkLabel = new LazyStyle(() =>
        {
            var style = new GUIStyle(EditorStyles.linkLabel);
            style.normal.textColor = Colors.Link;
            return style;
        });

        internal static readonly LazyStyle MultiLinkLabel = new LazyStyle(() =>
        {
            var style = new GUIStyle(EditorStyles.linkLabel);
            style.margin = new RectOffset(0, 0, 3, 3);
            style.stretchWidth = false;
            return style;
        });

        internal static readonly LazyStyle NoSizeStyle = new LazyStyle(() =>
        {
            var style = new GUIStyle();
            style.margin = new RectOffset(0, 0, 0, 0);
            style.padding = new RectOffset(0, 0, 0, 0);
            style.border = new RectOffset(0, 0, 0, 0);
            style.stretchWidth = false;
            style.stretchHeight = false;
            return style;
        });

        static GUISkin GetEditorSkin()
        {
            if (EditorGUIUtility.isProSkin)
                return EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);

            return EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
        }

        static GUIStyle CreateUnderlineStyle(Color color, int height)
        {
            GUIStyle style = new GUIStyle();

            Texture2D pixel = new Texture2D(1, height);

            for (int i = 0; i < height; i++)
                pixel.SetPixel(0, i, color);

            pixel.wrapMode = TextureWrapMode.Repeat;
            pixel.Apply();

            style.normal.background = pixel;
            style.fixedHeight = height;

            return style;
        }

        static void EnsureBackgroundStyles(LazyStyle lazy)
        {
            // The editor cleans the GUIStyleState.background property
            // when entering the edit mode (exiting the play mode)
            // and also in other situations (e.g when you use Zoom app)
            // Because of this, we have to reset them in order to
            // re-instantiate them the next time they're used

            if (!mLazyBackgroundStyles.Contains(lazy))
                return;

            bool needsRepaint = false;

            foreach (LazyStyle style in mLazyBackgroundStyles)
            {
                if (!style.IsInitialized)
                    continue;

                if (style.Value.normal.background != null)
                    continue;

                style.Reset();

                needsRepaint = true;
            }

            if (!needsRepaint)
                return;

            if (mUVCSWindowRepaint != null)
                mUVCSWindowRepaint();
        }

        static List<LazyStyle> mLazyBackgroundStyles = new List<LazyStyle>();

        internal class LazyStyle
        {
            internal bool IsInitialized { get; private set; }

            internal LazyStyle(Func<GUIStyle> builder)
            {
                mBuilder = builder;
                IsInitialized = false;
            }
            internal GUIStyle Value { get; private set; }

            internal void Reset()
            {
                IsInitialized = false;
            }

            // Note: User-defined operator must be declared static and public
            public static implicit operator GUIStyle(LazyStyle lazy)
            {
                if (lazy.IsInitialized)
                {
                    EnsureBackgroundStyles(lazy);
                    return lazy.Value;
                }

                lazy.Value = lazy.mBuilder();
                lazy.IsInitialized = true;
                return lazy.Value;
            }

            readonly Func<GUIStyle> mBuilder;
        }

        static Action mUVCSWindowRepaint;

        const int MODAL_FONT_SIZE = 13;
    }
}
