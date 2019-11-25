using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Cloud.Collaborate.Components;
using Unity.Cloud.Collaborate.Components.ChangeListEntries;
using Unity.Cloud.Collaborate.Models;
using Unity.Cloud.Collaborate.Models.Structures;
using Unity.Cloud.Collaborate.Presenters;
using Unity.Cloud.Collaborate.Tests.Components;
using Unity.Cloud.Collaborate.Tests.Models;
using Unity.Cloud.Collaborate.Views;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.Cloud.Collaborate.Tests.Views
{
    internal class ChangesViewTests : BaseViewTests
    {
        TestSourceControlProvider m_Provider;
        IMainModel m_MainModel;
        IChangesModel m_ChangesModel;
        TestChangesTabPageView m_View;
        IChangesPresenter m_Presenter;

        class TestChangesTabPageView : ChangesTabPageView
        {

        }

        [SetUp]
        public void Setup()
        {
            m_Provider = new TestSourceControlProvider();
            m_MainModel = new MainModel(m_Provider);
            m_ChangesModel = m_MainModel.ConstructChangesModel();
            m_View = new TestChangesTabPageView();
            m_Presenter = new ChangesPresenter(m_View, m_ChangesModel, m_MainModel);
            m_View.Presenter = m_Presenter;
            m_Window.rootVisualElement.Add(m_View);
        }

        [TearDown]
        public override void TearDown()
        {
            m_Provider = null;
            m_MainModel = null;
            m_ChangesModel = null;
            m_View = null;
            m_Presenter = null;
            m_Window.rootVisualElement.Clear();
        }

        [UnityTest]
        public IEnumerator TestPublishButtonWithNothingSelected()
        {
            return atc.Run(async () =>
            {
                m_View.SetActive(true);

                await WaitForUiThreadRender();

                Assert.IsNotNull(m_Provider.RequestedChangeListCallback, "m_Provider.RequestedChangeListCallback != null");
                m_Provider.RequestedChangeListCallback.Invoke(new List<IChangeEntry>());
                var publishButton = m_View.Q<IconTextButton>(className: ChangesTabPageView.PublishButtonUssClassName);
                UITestHelpers.SendClickEvent(m_Window, publishButton);

                // Publishing with nothing should be blocked.
                Assert.AreEqual(0, m_Provider.RequestedPublishCount);
            });
        }

        [UnityTest]
        public IEnumerator TestPublishButtonWithAllToggleSelected()
        {
            return atc.Run(async () =>
            {
                m_View.SetActive(true);

                await WaitForUiThreadRender();

                Assert.IsNotNull(m_Provider.RequestedChangeListCallback, "m_Provider.RequestedChangeListCallback != null");
                m_Provider.RequestedChangeListCallback.Invoke(new List<IChangeEntry>
                {
                    new ChangeEntry("a", status: ChangeEntryStatus.Added),
                    new ChangeEntry("b", status: ChangeEntryStatus.Added),
                    new ChangeEntry("c", status: ChangeEntryStatus.Added),
                });

                await WaitForUiThreadRender();

                m_View.Query<ToggleableChangeListElement>().AtIndex(0).Q<Toggle>().value = true;
                var publishButton = m_View.Q<IconTextButton>(className: ChangesTabPageView.PublishButtonUssClassName);
                UITestHelpers.SendClickEvent(m_Window, publishButton);

                // Ensure publish list is correct.
                Assert.AreEqual(1, m_Provider.RequestedPublishCount);
                Assert.IsNotNull(m_Provider.RequestedPublishList);
                Assert.AreEqual(3, m_Provider.RequestedPublishList.Count);
            });
        }

        [UnityTest]
        public IEnumerator TestPublishButtonWithOneToggleSelected()
        {
            return atc.Run(async () =>
            {
                m_View.SetActive(true);

                await WaitForUiThreadRender();

                Assert.IsNotNull(m_Provider.RequestedChangeListCallback, "m_Provider.RequestedChangeListCallback != null");
                m_Provider.RequestedChangeListCallback.Invoke(new List<IChangeEntry>
                {
                    new ChangeEntry("a", status: ChangeEntryStatus.Added),
                    new ChangeEntry("b", status: ChangeEntryStatus.Added),
                    new ChangeEntry("c", status: ChangeEntryStatus.Added),
                });

                await WaitForUiThreadRender();

                m_View.Query<ToggleableChangeListElement>().AtIndex(2).Q<Toggle>().value = true;
                var publishButton = m_View.Q<IconTextButton>(className: ChangesTabPageView.PublishButtonUssClassName);
                UITestHelpers.SendClickEvent(m_Window, publishButton);

                // Ensure publish list is correct.
                Assert.AreEqual(1, m_Provider.RequestedPublishCount);
                Assert.IsNotNull(m_Provider.RequestedPublishList);
                Assert.AreEqual(1, m_Provider.RequestedPublishList.Count);
                Assert.AreEqual("b", m_Provider.RequestedPublishList[0].Path);
            });
        }

        [UnityTest]
        public IEnumerator TestSearchBarAddingText()
        {
            return atc.Run(async () =>
            {
                m_View.SetActive(true);

                await WaitForUiThreadRender();

                Assert.IsNotNull(m_Provider.RequestedChangeListCallback, "m_Provider.RequestedChangeListCallback != null");
                m_Provider.RequestedChangeListCallback.Invoke(new List<IChangeEntry>
                {
                    new ChangeEntry("a", status: ChangeEntryStatus.Added),
                    new ChangeEntry("b", status: ChangeEntryStatus.Added),
                    new ChangeEntry("c", status: ChangeEntryStatus.Added),
                });

                await WaitForUiThreadRender();

                m_View.Q<ToolbarSearchField>().value = "c";

                // Wait for search timeout.
                await WaitForUiThreadRender();
                await Task.Delay(SearchBar.timeoutMilliseconds);

                var elms = m_View.Query<ToggleableChangeListElement>().ToList();
                // All shouldn't be shown in this situation, so there should only be one element.
                Assert.AreEqual(1, elms.Count);
                Assert.AreEqual("c", elms[0].Q<Label>(className: BaseChangeListElement.FileNameUssClassName).text);
            });
        }
    }
}
