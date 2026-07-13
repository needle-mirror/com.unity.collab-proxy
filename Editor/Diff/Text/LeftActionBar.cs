using Codice.CM.Client.Differences.Graphic;

using Unity.CodeEditor;

using UnityEngine;

using XDiffGui;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class LeftActionBar : ActionBar
    {
        internal LeftActionBar(
            Unity.CodeEditor.TextEditor textEditor,
            IActionBarClickListener listener)
            : base(textEditor, listener)
        { }

        protected override Vector2 GetLineStartPoint()
        {
            return new Vector2(0, 0);
        }

        protected override Vector2 GetLineEndPoint()
        {
            return new Vector2(0, resolvedStyle.height);
        }

        protected override ColorTextRegion GetTextRegion(DiffAction action)
        {
            return action.LeftRegion;
        }

        protected override AreaButton GetDifferenceButton(Rect rectangle)
        {
            if (rectangle.height == 0)
                return null;

            Rect buttonRectangle = GetAreaButtonRectangle(rectangle);

            AreaButton button = new AreaButton(DiffButtonActions.Restore);
            button.Bounds = buttonRectangle;

            return button;
        }

        protected override float GetButtonX()
        {
            return 0;
        }
    }
}
