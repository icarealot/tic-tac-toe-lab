#if UNITY_EDITOR
using NUnit.Framework;
using TicTacToeLab.Runtime;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class OutcomePopupTests
    {
        private const string OUTCOME_POPUP_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/UI/Popup/OutcomePopup.prefab";

        private OutcomePopup _sut;
        private TMP_Text _titleText;

        [SetUp]
        public void InstantiateProductionOutcomePopup()
        {
            GameObject outcomePopupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(OUTCOME_POPUP_PREFAB_PATH);
            Assert.That(outcomePopupPrefab, Is.Not.Null, "The production outcome popup prefab must exist at its shipped path.");

            _sut = UnityEngine.Object.Instantiate(outcomePopupPrefab).GetComponent<OutcomePopup>();
            Assert.That(_sut, Is.Not.Null, "The outcome popup prefab must carry the OutcomePopup component.");

            _titleText = _sut.GetComponentInChildren<TMP_Text>();
            Assert.That(_titleText, Is.Not.Null, "The outcome popup prefab must carry its title text.");
        }

        [TearDown]
        public void DestroyOutcomePopupInstance()
        {
            if (_sut != null)
            {
                UnityEngine.Object.Destroy(_sut.gameObject);
            }
        }

        [TestCase(Outcome.XWin, "X Wins!")]
        [TestCase(Outcome.OWin, "O Wins!")]
        [TestCase(Outcome.Draw, "Draw!")]
        public void The_popup_reports_the_terminal_outcome_as_its_title(Outcome outcome, string expectedTitle)
        {
            _sut.Setup(outcome, () => { });

            Assert.That(_titleText.text, Is.EqualTo(expectedTitle));
        }
    }
}
#endif
