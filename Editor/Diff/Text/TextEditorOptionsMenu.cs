using System.Collections.Generic;

using MergetoolGui;
using PlasticGui;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class TextEditorOptionsMenu
    {
        internal TextEditorOptionsMenu(
            ICodeEditorSettingsChangeListener settingsChangeListener)
        {
            mSettingsChangeListener = settingsChangeListener;
        }

        internal void SetOptions(
            bool viewWhiteSpaces,
            bool convertTabsToSpaces,
            bool viewEOL,
            int tabSize,
            IEnumerable<int> columnRulers)
        {
            SetViewWhitespaces(viewWhiteSpaces);
            SetConvertTabsToSpaces(convertTabsToSpaces);
            SetViewEOL(viewEOL);
            SetTabSize(tabSize);
            SetColumnGuides(columnRulers);
        }

        internal void SetViewWhitespaces(bool viewWhiteSpaces)
        {
            mViewWhitespaces = viewWhiteSpaces;
        }

        internal void SetConvertTabsToSpaces(bool convertTabsToSpaces)
        {
            mConvertTabsToSpaces = convertTabsToSpaces;
        }

        internal void SetViewEOL(bool viewEOL)
        {
            mViewEOL = viewEOL;
        }

        internal void SetTabSize(int tabSize)
        {
            mTabSize = tabSize;
        }

        internal void SetColumnGuides(IEnumerable<int> columnRulers)
        {
            mColumnGuides = columnRulers != null
                ? new List<int>(columnRulers)
                : new List<int>();
        }

        internal void BuildMenuItems(GenericMenu menu, string submenuPath)
        {
            menu.AddItem(
                new GUIContent(submenuPath + MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.EditorOptionsConvertTabsToSpaces)),
                mConvertTabsToSpaces,
                ConvertTabsToSpacesMenuItem_Click);

            menu.AddItem(
                new GUIContent(submenuPath + MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.EditorOptionsWhistespaces)),
                mViewWhitespaces,
                ViewWhitespacesMenuItem_Click);

            menu.AddItem(
                new GUIContent(submenuPath + MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.EditorOptionsEOL)),
                mViewEOL,
                ViewEOLMenuItem_Click);

            string tabsSubmenu = submenuPath + MergetoolLocalization.GetString(
                MergetoolLocalization.Name.EditorOptionsTabs) + "/";

            menu.AddItem(
                new GUIContent(tabsSubmenu + MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.EditorOptionsTabsSpacesNumber, 2)),
                mTabSize == 2,
                () => TabsMenuItem_Click(2));

            menu.AddItem(
                new GUIContent(tabsSubmenu + MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.EditorOptionsTabsSpacesNumber, 4)),
                mTabSize == 4,
                () => TabsMenuItem_Click(4));

            menu.AddItem(
                new GUIContent(tabsSubmenu + MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.EditorOptionsTabsSpacesNumber, 8)),
                mTabSize == 8,
                () => TabsMenuItem_Click(8));

            string columnGuidesSubmenu = submenuPath + MergetoolLocalization.GetString(
                MergetoolLocalization.Name.EditorOptionsColumnGuides) + "/";

            BuildColumnGuideMenuItem(menu, columnGuidesSubmenu, 60);
            BuildColumnGuideMenuItem(menu, columnGuidesSubmenu, 80);
            BuildColumnGuideMenuItem(menu, columnGuidesSubmenu, 100);
            BuildColumnGuideMenuItem(menu, columnGuidesSubmenu, 120);
            BuildColumnGuideMenuItem(menu, columnGuidesSubmenu, 140);
        }

        void BuildColumnGuideMenuItem(
            GenericMenu menu, string submenuPath, int column)
        {
            menu.AddItem(
                new GUIContent(submenuPath + MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.EditorOptionsColumnsNumber, column)),
                mColumnGuides.Contains(column),
                () => ColumnGuideMenuItem_Click(column));
        }

        void ViewWhitespacesMenuItem_Click()
        {
            mViewWhitespaces = !mViewWhitespaces;

            mSettingsChangeListener.OnViewWhitespacesChanged(mViewWhitespaces);

            PlasticGuiConfig.Get().Configuration.EditorOptionsShowWhiteSpaces =
                mViewWhitespaces;
            PlasticGuiConfig.Get().Save();
        }

        void ConvertTabsToSpacesMenuItem_Click()
        {
            mConvertTabsToSpaces = !mConvertTabsToSpaces;

            mSettingsChangeListener.OnConvertTabsToWhitespacesChanged(mConvertTabsToSpaces);

            PlasticGuiConfig.Get().Configuration.EditorOptionsConvertTabsToSpaces =
                mConvertTabsToSpaces;
            PlasticGuiConfig.Get().Save();
        }

        void ViewEOLMenuItem_Click()
        {
            mViewEOL = !mViewEOL;

            mSettingsChangeListener.OnViewEOLChanged(mViewEOL);

            PlasticGuiConfig.Get().Configuration.EditorOptionsShowEOL = mViewEOL;
            PlasticGuiConfig.Get().Save();
        }

        void TabsMenuItem_Click(int tabSize)
        {
            mTabSize = tabSize;

            mSettingsChangeListener.OnTabSizeChanged(tabSize);

            PlasticGuiConfig.Get().Configuration.EditorOptionsTabSize = tabSize;
            PlasticGuiConfig.Get().Save();
        }

        void ColumnGuideMenuItem_Click(int column)
        {
            if (mColumnGuides.Contains(column))
                mColumnGuides.Remove(column);
            else
                mColumnGuides.Add(column);

            mColumnGuides.Sort();

            mSettingsChangeListener.OnColumnGuidesChanged(mColumnGuides);

            PlasticGuiConfig.Get().Configuration.EditorOptionsColumnGuides = mColumnGuides;
            PlasticGuiConfig.Get().Save();
        }

        bool mViewWhitespaces;
        bool mConvertTabsToSpaces;
        bool mViewEOL;
        int mTabSize;
        List<int> mColumnGuides = new List<int>();

        readonly ICodeEditorSettingsChangeListener mSettingsChangeListener;
    }
}
