using NUnit.Framework;
using Unity.Cloud.Collaborate.Components;
using Unity.Cloud.Collaborate.Views.Adapters.ListAdapters;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Cloud.Collaborate.Tests.Components
{
    [TestFixture]
    internal class AdapterListViewTests : BaseComponentTests
    {
        AdapterListView m_AdapterListView;

        [SetUp]
        public void SetUp()
        {
            m_AdapterListView = new AdapterListView();
            m_AdapterListView.style.width = 400;
            m_AdapterListView.style.height = 400;
            m_Window.rootVisualElement.Add(m_AdapterListView);
        }

        [Test]
        public void TestBasicAdapter()
        {
            atc.Run(async () =>
            {
                var adapter = new TestAdapter();
                m_AdapterListView.SetAdapter(adapter);

                await WaitForUiThreadRender();

                UITestHelpers.IsCompletelyVisible(m_Window, m_AdapterListView).ShouldBe(true, "The adapter listview should be visible.");
                adapter.BindCount.ShouldBe(adapter.GetEntryCount(), "Number of entries are not being respected.");
            });
        }

        [Test]
        public void TestDataUpdate()
        {
            atc.Run(async () =>
            {
                var adapter = new TestAdapter();
                m_AdapterListView.SetAdapter(adapter);

                await WaitForUiThreadRender();

                adapter.NotifyDataSetChanged();

                await WaitForUiThreadRender();

                adapter.BindCount.ShouldBe(adapter.GetEntryCount() * 2, "Entries are not being reloaded.");
            });
        }

        [Test]
        public void TestBasicAdapterClearBox()
        {
            atc.Run(async () =>
            {
                var adapter = new TestAdapter();
                m_AdapterListView.SetAdapter(adapter);

                await WaitForUiThreadRender();

                adapter.BindCount.ShouldBe(adapter.GetEntryCount(), "Number of entries are not being respected.");
                var listView = m_AdapterListView.Q<ListView>(className: AdapterListView.ListViewUssClassName);
                var entries = listView.Query<TextElement>().ToList();

                // Ensure the entries are displayed correctly and in order.
                entries.Count.ShouldBe(adapter.GetEntryCount(), "Should be the correct number of entries in the layout");
                for (var i = 0; i < adapter.GetEntryCount(); i++)
                {
                    entries[i].text.ShouldBe(i.ToString(), "The values should match");
                }
            });
        }

        class TestAdapter : BaseListAdapter<TextElement>
        {
            public int MakeCount;
            public int BindCount;

            public override int Height => 20;

            public override int GetEntryCount()
            {
                return 2;
            }

            protected override void BindItem(TextElement element, int index)
            {
                BindCount++;
                element.text = index.ToString();
            }

            protected override TextElement MakeItem()
            {
                MakeCount++;
                return new TextElement();
            }
        }
    }
}
