using System.Collections.Generic;

namespace Unity.CodeEditor
{
    internal interface ICodeEditorSettingsChangeListener
    {
        void OnFontFamilyChanged(string font);

        void OnTabSizeChanged(int tabSize);

        void OnColumnGuidesChanged(IEnumerable<int> columnGuides);

        void OnConvertTabsToWhitespacesChanged(bool value);

        void OnViewWhitespacesChanged(bool value);

        void OnViewEOLChanged(bool value);
    }
}
