#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class InputServiceTests : InputTestFixture
    {
        private InputService _sut;
        private InputActionAsset _serviceActions;

        public override void Setup()
        {
            base.Setup();
            InputActionAsset[] assetsBeforeService = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            _sut = new InputService();
            _serviceActions = Resources.FindObjectsOfTypeAll<InputActionAsset>()
                .Single(asset => !assetsBeforeService.Contains(asset));
        }

        public override void TearDown()
        {
            _sut.Dispose();
            base.TearDown();
        }

        [Test]
        public void A_newly_constructed_service_does_not_publish_pointer_presses()
        {
            // Arrange
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            int pressCount = 0;
            _sut.Pressed += _ => pressCount++;

            // Act
            PressPointer(mouse, new Vector2(50f, 50f));

            // Assert
            Assert.That(pressCount, Is.EqualTo(0), "A newly constructed service should not publish presses until player press is enabled.");
        }

        [Test]
        public void A_pointer_press_publishes_exactly_one_press_with_the_pointer_position()
        {
            // Arrange
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            Vector2 screenPosition = new(321f, 654f);
            int pressCount = 0;
            Vector2 publishedPosition = default;
            _sut.Pressed += position =>
            {
                pressCount++;
                publishedPosition = position;
            };
            _sut.EnablePlayerPress();

            // Act
            PressPointer(mouse, screenPosition);

            // Assert
            Assert.That(pressCount, Is.EqualTo(1), "A pointer gesture should publish exactly one Pressed event.");
            Assert.That(publishedPosition, Is.EqualTo(screenPosition), "The Pressed event should carry the pointer's screen position.");
        }

        [Test]
        public void Disabling_player_press_suppresses_pointer_publication()
        {
            // Arrange
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            int pressCount = 0;
            _sut.Pressed += _ => pressCount++;
            _sut.EnablePlayerPress();
            PressPointer(mouse, new Vector2(100f, 100f));

            // Act
            _sut.DisablePlayerPress();
            PressPointer(mouse, new Vector2(200f, 200f));

            // Assert
            Assert.That(pressCount, Is.EqualTo(1), "Disabling player press should suppress further pointer publication.");
        }

        [Test]
        public void Re_enabling_player_press_restores_pointer_publication()
        {
            // Arrange
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            int pressCount = 0;
            _sut.Pressed += _ => pressCount++;
            _sut.DisablePlayerPress();
            PressPointer(mouse, new Vector2(10f, 10f));

            // Act
            _sut.EnablePlayerPress();
            PressPointer(mouse, new Vector2(20f, 20f));

            // Assert
            Assert.That(pressCount, Is.EqualTo(1), "Re-enabling player press should restore pointer publication.");
        }

        [Test]
        public void Back_publishes_while_player_press_is_disabled()
        {
            // Arrange
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            int backCount = 0;
            _sut.BackPressed += () => backCount++;
            _sut.DisablePlayerPress();

            // Act
            PressBack(keyboard);

            // Assert
            Assert.That(backCount, Is.EqualTo(1), "Back should publish while player press is disabled.");
        }

        [UnityTest]
        public IEnumerator Disposal_is_repeatable_and_releases_its_input_actions()
        {
            // Act
            _sut.Dispose();
            TestDelegate disposeRepeatedly = () =>
            {
                _sut.Dispose();
                _sut.Dispose();
            };

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _serviceActions == null,
                "Disposal should destroy the Input System action asset the service created.");
            Assert.That(disposeRepeatedly, Throws.Nothing, "Disposing the service repeatedly should be harmless.");
        }

        [Test]
        public void A_disposed_service_is_inert()
        {
            // Arrange
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            int pressCount = 0;
            int backCount = 0;
            _sut.Pressed += _ => pressCount++;
            _sut.BackPressed += () => backCount++;
            _sut.EnablePlayerPress();
            _sut.Dispose();

            // Act
            _sut.EnablePlayerPress();
            _sut.DisablePlayerPress();
            PressPointer(mouse, new Vector2(60f, 60f));
            PressBack(keyboard);

            // Assert
            Assert.That(pressCount, Is.EqualTo(0), "A disposed service should not publish pointer presses.");
            Assert.That(backCount, Is.EqualTo(0), "A disposed service should not publish Back presses.");
        }

        private void PressPointer(Mouse mouse, Vector2 screenPosition)
        {
            Set(mouse.position, screenPosition, queueEventOnly: true);
            Press(mouse.leftButton, queueEventOnly: true);
            InputSystem.Update();
            Release(mouse.leftButton, queueEventOnly: true);
            InputSystem.Update();
        }

        private void PressBack(Keyboard keyboard)
        {
            Press(keyboard.escapeKey, queueEventOnly: true);
            InputSystem.Update();
            Release(keyboard.escapeKey, queueEventOnly: true);
            InputSystem.Update();
        }
    }
}
#endif
