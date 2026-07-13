using System.Collections.Generic;
using System.Text;

using Codice.Client.Common;
using Codice.CM.Client.Differences.Graphic;
using MergetoolGui;
using UnityEditor;
using UnityEngine;
using XDiffGui.Options;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class EncodingMenu
    {
        internal EncodingMenu(IEncodingListener encodingListener)
        {
            mEncodingListener = encodingListener;

            mPresetEncodings = new Dictionary<string, Encoding>
            {
                { MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.NoneEncoding), null },
                { MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.AsciiEncoding), Encoding.ASCII },
                { MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.UnicodeEncoding), Encoding.Unicode },
                { MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.UnicodeBigEndianEncoding), Encoding.BigEndianUnicode },
                { MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.Utf8Encoding), EncodingManager.UTF8WithoutBOM },
            };
        }

        internal void SetEncodingMenu(
            TextBoxContributor contributor, EntryData entryData)
        {
            mContributor = contributor;
            mEntryData = entryData;

            bool bIsFound = false;
            foreach (Encoding preset in mPresetEncodings.Values)
            {
                if (IsSameEncoding(preset, entryData.Encoding))
                {
                    bIsFound = true;
                    break;
                }
            }

            ConfigureOtherEncodingLabel(bIsFound ? null : entryData.Encoding);
        }

        internal void BuildMenuItems(GenericMenu menu, string submenuPath)
        {
            foreach (KeyValuePair<string, Encoding> entry in mPresetEncodings)
            {
                bool isSelected = mEntryData != null &&
                    IsSameEncoding(entry.Value, mEntryData.Encoding);

                Encoding capturedEncoding = entry.Value;
                menu.AddItem(
                    new GUIContent(submenuPath + entry.Key),
                    isSelected,
                    () => Encoding_Click(capturedEncoding));
            }

            menu.AddSeparator(submenuPath);

            bool isOtherSelected = mEntryData != null &&
                !string.IsNullOrEmpty(mOtherEncodingLabel);

            menu.AddItem(
                new GUIContent(submenuPath + GetOtherMenuItemLabel()),
                isOtherSelected,
                OtherEncoding_Click);
        }

        void Encoding_Click(Encoding newEncoding)
        {
            if (mEntryData == null)
                return;

            if (!mEncodingListener.OnEncodingChanged(mContributor, newEncoding))
                return;

            mEntryData.Encoding = newEncoding;

            ConfigureOtherEncodingLabel(null);
        }

        void OtherEncoding_Click()
        {
            if (mEntryData == null)
                return;

            EncodingDialog.Show(
                mEntryData.Encoding,
                OnOtherEncodingSelected);
        }

        void OnOtherEncodingSelected(Encoding newEncoding)
        {
            if (newEncoding == null)
                return;

            if (!mEncodingListener.OnEncodingChanged(mContributor, newEncoding))
                return;

            mEntryData.Encoding = newEncoding;

            ConfigureOtherEncodingLabel(newEncoding);
        }

        void ConfigureOtherEncodingLabel(Encoding encoding)
        {
            if (encoding == null)
            {
                mOtherEncodingLabel = null;
                return;
            }

            mOtherEncodingLabel = string.Format(
                "{0} ({1})",
                encoding.EncodingName,
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.ChangeEncoding));
        }

        string GetOtherMenuItemLabel()
        {
            if (!string.IsNullOrEmpty(mOtherEncodingLabel))
                return mOtherEncodingLabel;

            return MergetoolLocalization.GetString(
                MergetoolLocalization.Name.OtherEncoding);
        }

        static bool IsSameEncoding(Encoding menuItemEncoding, Encoding entryEncoding)
        {
            if (menuItemEncoding == null)
                return entryEncoding == null;

            return menuItemEncoding.Equals(entryEncoding);
        }

        readonly IEncodingListener mEncodingListener;
        readonly Dictionary<string, Encoding> mPresetEncodings;

        TextBoxContributor mContributor;
        EntryData mEntryData;
        string mOtherEncodingLabel;
    }
}
