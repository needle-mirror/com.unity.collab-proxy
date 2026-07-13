using Codice.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.UI.UIElements
{
    internal static class KeyboardEvents
    {
        internal static bool MatchesShortcut(KeyDownEvent e, string shortcut)
        {
            if (string.IsNullOrEmpty(shortcut))
                return false;

            bool needCtrl = false;
            bool needCmd = false;
            bool needShift = false;
            bool needAlt = false;

            int keyIndex = 0;
            while (keyIndex < shortcut.Length)
            {
                switch (shortcut[keyIndex])
                {
                    case '%':
                        if (PlatformIdentifier.IsMac())
                            needCmd = true;
                        else
                            needCtrl = true;
                        break;
                    case '^':
                        needCtrl = true;
                        break;
                    case '#':
                        needShift = true;
                        break;
                    case '&':
                        needAlt = true;
                        break;
                    case '_':
                        break;
                    default:
                        goto doneParsingModifiers;
                }

                keyIndex++;
            }

            doneParsingModifiers:

            if (keyIndex >= shortcut.Length)
                return false;

            string keyStr = shortcut.Substring(keyIndex);
            if (keyStr.Length != 1)
                return false;

            KeyCode expectedKey = (KeyCode)char.ToLower(keyStr[0]);

            return e.keyCode == expectedKey
                   && e.ctrlKey == needCtrl
                   && e.commandKey == needCmd
                   && e.shiftKey == needShift
                   && e.altKey == needAlt;
        }

        internal static bool IsAltPressed(KeyDownEvent evt)
        {
            return evt.altKey;
        }

        internal static bool IsControlPressed(KeyDownEvent evt)
        {
            return evt.ctrlKey;
        }

        internal static bool IsCommandPressed(KeyDownEvent evt)
        {
            return evt.commandKey;
        }

        internal static bool IsShiftPressed(KeyDownEvent evt)
        {
            return evt.shiftKey;
        }

        internal static bool IsEscapePressed(KeyDownEvent evt)
        {
            return evt.keyCode == KeyCode.Escape;
        }

        internal static bool IsEnterPressed(KeyDownEvent evt)
        {
            return evt.keyCode == KeyCode.Return ||
                   evt.keyCode == KeyCode.KeypadEnter;
        }

        internal static bool IsFindShortcutPressed(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.F)
                return false;

            if (PlatformIdentifier.IsMac())
                return evt.commandKey;

            return evt.ctrlKey;
        }
    }
}
