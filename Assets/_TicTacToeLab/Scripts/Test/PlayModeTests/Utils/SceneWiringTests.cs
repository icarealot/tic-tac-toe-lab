#if UNITY_EDITOR
using UnityEngine.InputSystem;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// A play-mode test that loads the real scene. Every such test shares this fixture, because the
    /// app enables input actions the moment it comes up: one that ran outside the input fixture's
    /// sandbox would leave those actions bound to devices the next test's reset takes away, and the
    /// throw would land in that next test rather than in the one that caused it.
    /// </summary>
    public abstract class SceneWiringTests : InputTestFixture
    {
    }
}
#endif
