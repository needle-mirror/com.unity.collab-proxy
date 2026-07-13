using System.Text;
using Codice.CM.Client.Differences.Graphic;
using Unity.CodeEditor.TextMate;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class EditorDiffContent : IDiffContent
    {
        internal EditorDiffContent(DocumentSnapshot docSnapshot)
        {
            mDocSnapshot = docSnapshot;
        }

        internal void Dispose()
        {
            mDocSnapshot = null;
        }

        public string Text => mDocSnapshot.GetText();

        int IDiffContent.NumLines => mDocSnapshot.LineCount;

        string IDiffContent.GetLineTerminator(int line)
        {
            return mDocSnapshot.GetLineTerminator(line);
        }

        string IDiffContent.GetLineText(int line)
        {
            return mDocSnapshot.GetLineText(line);
        }

        string IDiffContent.GetLineTextIncludingLineTerminator(int line)
        {
            return mDocSnapshot.GetLineTextIncludingTerminator(line);
        }

        string IDiffContent.GetTextRange(int initialLine, int endLine)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = initialLine; i <= endLine; i++)
            {
                builder.Append(mDocSnapshot.GetLineTextIncludingTerminator(i));
            }
            return builder.ToString();
        }

        DocumentSnapshot mDocSnapshot;
    }
}
