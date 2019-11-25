using System.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Unity.Cloud.Collaborate.Tests.Components
{
    [TestFixture]
    internal class BaseComponentTests
    {
        protected AsyncToCoroutine atc;
        protected EditorWindow m_Window;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            atc = new AsyncToCoroutine();
            m_Window = EditorWindow.GetWindow<TestWindow>();
            m_Window.minSize = new Vector2(400, 400);
            m_Window.Show();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            m_Window.Close();
        }

        [TearDown]
        public void TearDown()
        {
            m_Window.rootVisualElement.Clear();
        }

        protected async Task WaitForUiThreadRender()
        {
            await Task.Delay(TestConstants.RENDER_UI_THREAD_DELAY);
        }
    }
}
