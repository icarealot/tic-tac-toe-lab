#if UNITY_EDITOR
using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class HomeScreenTests
    {
        private GeneratedAppUIFixture _fixture;

        [SetUp]
        public void CreateGeneratedHomeScreen()
        {
            _fixture = new GeneratedAppUIFixture();
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedHomeScreen()
        {
            yield return _fixture.IE_DestroyAll();
        }

        [Test]
        public void A_home_screen_without_a_PvP_button_fails_during_initialization()
        {
            // Arrange
            (HomeScreen screen, Button _, Button _) = _fixture.CreateHomeScreen(
                includePvpButton: false,
                activate: false);
            LogAssert.Expect(LogType.Exception, new Regex("NullReferenceException"));

            // Act
            screen.gameObject.SetActive(true);
        }

        [Test]
        public void A_home_screen_without_a_PvE_button_fails_during_initialization()
        {
            // Arrange
            (HomeScreen screen, Button _, Button _) = _fixture.CreateHomeScreen(
                includePveButton: false,
                activate: false);
            LogAssert.Expect(LogType.Exception, new Regex("NullReferenceException"));

            // Act
            screen.gameObject.SetActive(true);
        }

    }
}
#endif
