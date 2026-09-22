#if UNITY_EDITOR
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class ProductionUiPrefabTests
    {
        private const string MAIN_MENU_PANEL_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/UI/Panel/MainMenuPanel.prefab";
        private const string GAMEPLAY_PANEL_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/UI/Panel/GameplayPanel.prefab";
        private const string CONFIRM_QUIT_POPUP_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/UI/Popup/ConfirmQuitPopup.prefab";
        private const string OUTCOME_POPUP_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/UI/Popup/OutcomePopup.prefab";

        [TestCase(MAIN_MENU_PANEL_PREFAB_PATH)]
        [TestCase(GAMEPLAY_PANEL_PREFAB_PATH)]
        [TestCase(CONFIRM_QUIT_POPUP_PREFAB_PATH)]
        [TestCase(OUTCOME_POPUP_PREFAB_PATH)]
        public void Every_player_facing_ui_prefab_keeps_its_safe_area_adapter(string prefabPath)
        {
            // Arrange
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            // Assert
            Assert.That(prefab, Is.Not.Null, $"The production UI prefab must exist at {prefabPath}.");
            Assert.That(
                prefab.GetComponentInChildren<SafeAreaRect>(includeInactive: true),
                Is.Not.Null,
                $"{prefab.name} should keep its safe-area adapter so interactive UI follows the current safe area.");
        }
    }
}
#endif
