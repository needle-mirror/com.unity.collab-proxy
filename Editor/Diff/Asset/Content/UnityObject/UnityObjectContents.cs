using System.Collections.Generic;
using System.Diagnostics;
using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using Codice.LogWrapper;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

using Unity.PlasticSCM.Editor.Diff.Asset.Common.UnityObject;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content.UnityObject
{
    internal static class UnityObjectContents
    {
        internal static List<ObjectContent> BuildContentData(string file)
        {
            Stopwatch sw = Stopwatch.StartNew();

            List<ObjectContent> objectContents = new List<ObjectContent>();

            UnityEngine.Object[] objects = InternalEditorUtility.LoadSerializedFileAndForget(file);

            foreach (UnityEngine.Object obj in objects)
            {
                if (obj == null)
                    continue;

                objectContents.Add(BuildObjectContent(obj));
            }

            GroupComponentsUnderGameObjects(objectContents);

            RemoveEmptyNonGameObjects(objectContents);

            HierarchyOrderSorting.SortByHierarchyOrder(
                objectContents, c => c.Object, objects);

            mLog.DebugFormat(
                "{0} object contents calculated in {1} ms",
                objectContents.Count, sw.ElapsedMilliseconds);

            return objectContents;
        }

        static ObjectContent BuildObjectContent(UnityEngine.Object obj)
        {
            ObjectContent content = new ObjectContent
            {
                Object = obj
            };

            using (SerializedObject so = new SerializedObject(obj))
                content.PropertyTree = PropertyTreeBuilder.Build(so);

            return content;
        }

        static void GroupComponentsUnderGameObjects(List<ObjectContent> objectContents)
        {
            Dictionary<GameObject, ObjectContent> gameObjectMap =
                new Dictionary<GameObject, ObjectContent>();

            foreach (ObjectContent content in objectContents)
            {
                if (content.Object is GameObject go)
                    gameObjectMap[go] = content;
            }

            if (gameObjectMap.Count == 0)
                return;

            List<ObjectContent> topLevel = new List<ObjectContent>(objectContents.Count);

            foreach (ObjectContent content in objectContents)
            {
                if (content.Object is Component c && c.gameObject != null
                    && gameObjectMap.TryGetValue(c.gameObject, out ObjectContent parent))
                {
                    if (parent.ComponentContents == null)
                        parent.ComponentContents = new List<ObjectContent>();

                    parent.ComponentContents.Add(content);
                    continue;
                }

                topLevel.Add(content);
            }

            objectContents.Clear();
            objectContents.AddRange(topLevel);

            foreach (ObjectContent content in objectContents)
                SortComponentsByGameObjectOrder(content);
        }

        static void SortComponentsByGameObjectOrder(ObjectContent gameObjectContent)
        {
            if (gameObjectContent.ComponentContents == null)
                return;

            if (!(gameObjectContent.Object is GameObject go))
                return;

            Component[] orderedComponents = go.GetComponents<Component>();
            Dictionary<Component, int> orderIndex = new Dictionary<Component, int>(
                orderedComponents.Length);

            for (int i = 0; i < orderedComponents.Length; i++)
            {
                if (orderedComponents[i] != null)
                    orderIndex[orderedComponents[i]] = i;
            }

            gameObjectContent.ComponentContents.Sort((a, b) =>
            {
                int ia = a.Object is Component ca && orderIndex.TryGetValue(ca, out int va)
                    ? va : int.MaxValue;
                int ib = b.Object is Component cb && orderIndex.TryGetValue(cb, out int vb)
                    ? vb : int.MaxValue;
                return ia.CompareTo(ib);
            });
        }

        // Drops objects whose property tree is empty after IgnoredProperties
        // filtering — e.g. SceneRoots, whose only fields (m_ObjectHideFlags
        // and m_Roots) are both ignored. GameObjects are kept regardless
        // because they own their components.
        static void RemoveEmptyNonGameObjects(List<ObjectContent> objectContents)
        {
            for (int i = objectContents.Count - 1; i >= 0; i--)
            {
                ObjectContent content = objectContents[i];
                if (content.Object is GameObject)
                    continue;

                PropertyTreeNode tree = content.PropertyTree;
                if (tree != null && tree.Children.Count > 0)
                    continue;

                objectContents.RemoveAt(i);
            }
        }

        static readonly ILog mLog = PlasticApp.GetLogger("UnityObjectContents");
    }
}
