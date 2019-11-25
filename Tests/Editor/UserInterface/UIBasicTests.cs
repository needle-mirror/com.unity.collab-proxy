using System;
using UnityEngine;

namespace Unity.Cloud.Collaborate.Tests.UserInterface
{
    public class UIBasicTests : ScenarioTestBase
    {
//        [UnityTest]
//        public IEnumerator PublishButtonEnabledWhenCommitMessageIsFilled()
//        {
//            return atc.Run(async () =>
//            {
//                await EnsureCleanChangesPageInitially();
//
//                var mainWindow = EditorWindow.GetWindow<CollaborateWindow>();
//                var rootElement = mainWindow.rootVisualElement;
//
//                var publish = rootElement.Q<IconTextButton>(className: ChangesTabPageView.PublishButtonUssClassName);
//                var commitMsg = rootElement.Q<BetterTextField>(className: ChangesTabPageView.TextFieldUssClassName);
//
//                // let go of the uithread to actually render the objects
//                await Task.Delay(TestConstants.RENDER_UI_THREAD_DELAY);
//
//                UITestHelpers.IsPartiallyVisible(mainWindow, publish).ShouldBe(true, "publish button should be at least partially visible");
//
//                var text = !string.IsNullOrEmpty(commitMsg.text) ? commitMsg.text : string.Empty;
//                text.ShouldBe(string.Empty, "initial text of commit msg box should be empty");
//
//                // the publish button should initially be disabled.
//                publish.enabledInHierarchy.ShouldBe(false, "initial state of publish button should NOT be enabled");
//
//                commitMsg.Focus();
//                UITestHelpers.SendClickEvent(mainWindow, commitMsg); // select the commitMsg.
//
//                // simulate typing some commit message !
//                const string commitText = "This is just a dummy text !";
//                UITestHelpers.SimulateTyping(mainWindow, commitText);
//
//                // verify commit message is no longer empty !
//                text = !string.IsNullOrEmpty(commitMsg.text) ? commitMsg.text : string.Empty;
//                text.ShouldBe(commitText, "text of commit msg box should match simulated typing");
//
//                // now the button should be enabled !
//                publish.enabledInHierarchy.ShouldBe(true, "state of publish button after typing commit message should be Enabled");
//
//                // todo: clean up after test. this should be done in an after() method.
//                // TestHelpers.ShouldBe(text, string.Empty, "post text of commit msg box should be empty");
//            });
//        }
//
//        [UnityTest]
//        public IEnumerator ChangesPageWithoutConflictsHidesConflicts()
//        {
//            return atc.Run(async () =>
//            {
//                // Close the MainWindow.
//                var mainWindow = EditorWindow.GetWindow<CollaborateWindow>();
//                mainWindow.Close();
//
//                // Set up the mock provider.
//                TestSourceControlProvider.SetMockProvider();
//                var changes = new List<IChangeEntry>
//                {
//                    CreateSourceControlEntry("alpha", ChangeEntryStatus.Modified, false),
//                    CreateSourceControlEntry("bravo", ChangeEntryStatus.Modified, false),
//                    CreateSourceControlEntry("charlie", ChangeEntryStatus.Modified, false),
//                    CreateSourceControlEntry("delta", ChangeEntryStatus.Modified, false),
//                    CreateSourceControlEntry("echo", ChangeEntryStatus.Modified, false)
//                };
//
//                // Create the MainWindow.
//                mainWindow = EditorWindow.GetWindow<CollaborateWindow>();
//                mainWindow.Show();
//
//                // Set the changes, allowing notifications to be sent.
//                TestSourceControlProvider.MockProvider.SetChanges(changes);
//
//                // Let go of the uithread to actually render the objects
//                await Task.Delay(TestConstants.RENDER_UI_THREAD_DELAY);
//
//                // Find the conflict AdapterListView and ensure it is visible and has the appropriate number of items.
//                var rootElement = mainWindow.rootVisualElement;
//                var conflicted = rootElement.Q<AdapterListView>(StringAssets.changeListConflictedList, AdapterListView.UssClassName);
//                UITestHelpers.IsPartiallyVisible(mainWindow, conflicted).ShouldBe(false, $"conflicted {conflicted.name} AdapterListView should be hidden");
//                conflicted.Query<ConflictedChangeListElement>().ToList().Count.ShouldBe(0, "conflicted item count should be 0");
//                // Close the MainWindow and reset the provider.
//                mainWindow.Close();
//                TestSourceControlProvider.ResetProvider();
//            });
//        }
//
//        [UnityTest]
//        public IEnumerator ChangesPageWithConflictsShowsOnlyConflicts()
//        {
//            return atc.Run(async () =>
//            {
//                // Close the MainWindow.
//                var mainWindow = EditorWindow.GetWindow<CollaborateWindow>();
//                mainWindow.Close();
//
//                // Set up the mock provider.
//                TestSourceControlProvider.SetMockProvider();
//                var changes = new List<IChangeEntry>
//                {
//                    CreateSourceControlEntry("alpha", ChangeEntryStatus.Modified, false),
//                    CreateSourceControlEntry("conflictedbravo", ChangeEntryStatus.Modified, false, true),
//                    CreateSourceControlEntry("charlie", ChangeEntryStatus.Modified, false),
//                    CreateSourceControlEntry("delta", ChangeEntryStatus.Modified, false),
//                    CreateSourceControlEntry("conflictedecho", ChangeEntryStatus.Modified, false, true)
//                };
//
//                // Create the MainWindow.
//                mainWindow = EditorWindow.GetWindow<CollaborateWindow>();
//                mainWindow.Show();
//
//
//                // Set the changes, allowing notifications to be sent.
//                TestSourceControlProvider.MockProvider.SetChanges(changes);
//
//                // Let go of the uithread to actually render the objects
//                await Task.Delay(TestConstants.RENDER_UI_THREAD_DELAY);
//
//                // Find the conflict ListView and ensure it is visible and has the appropriate number of items.
//                var rootElement = mainWindow.rootVisualElement;
//                var conflicted = rootElement.Q<AdapterListView>(StringAssets.changeListConflictedList, AdapterListView.UssClassName);
//                UITestHelpers.IsPartiallyVisible(mainWindow, conflicted).ShouldBe(true, $"conflicted {conflicted.name} AdapterListView should be at least partially visible");
//                conflicted.Query<ConflictedChangeListElement>().ToList().Count.ShouldBe(2, "conflicted item count should be 2");
//
//                // Close the MainWindow and reset the provider.
//                mainWindow.Close();
//                TestSourceControlProvider.ResetProvider();
//            });
//        }
//
//
//        static ChangeEntry CreateSourceControlEntry(string path, ChangeEntryStatus status, bool staged, bool unmerged = false)
//        {
//            return new ChangeEntry(path, $"Original{path}", status, staged, unmerged);
//        }
    }
}
