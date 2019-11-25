using System;
using System.Collections.Generic;
using Unity.Cloud.Collaborate.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cloud.Collaborate.UserInterface
{
    [Location("Cache/Window.yml", LocationAttribute.Location.LibraryFolder)]
    internal class WindowCache : ScriptableObjectSingleton<WindowCache>
    {
        public event Action BeforeSerialize;

        public void Serialize()
        {
            BeforeSerialize?.Invoke();
            Save();
        }

        [SerializeField]
        public SelectedItemsDictionary SimpleSelectedItems = new SelectedItemsDictionary();

        [FormerlySerializedAs("CommitMessage")]
        [SerializeField]
        public string RevisionSummary;

        [SerializeField]
        public string ChangesSearchValue;

        [SerializeField]
        public string SelectedHistoryRevision;

        [SerializeField]
        public int HistoryPageNumber;

        [SerializeField]
        public int TabIndex;
    }

    [Serializable]
    internal class SelectedItemsDictionary : SerializableDictionary<string, bool>
    {
        public SelectedItemsDictionary() { }

        public SelectedItemsDictionary(IDictionary<string, bool> dictionary) : base(dictionary) { }
    }
}


