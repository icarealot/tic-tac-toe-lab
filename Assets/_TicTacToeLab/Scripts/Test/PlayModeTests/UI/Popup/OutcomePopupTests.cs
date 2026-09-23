#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class OutcomePopupTests
    {
        private const string OUTCOME_POPUP_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/UI/Popup/OutcomePopup.prefab";

        private GameObject _instance;
        private OutcomePopup _sut;
        private TMP_Text _titleText;

        [SetUp]
        public void InstantiateProductionOutcomePopup()
        {
            GameObject outcomePopupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(OUTCOME_POPUP_PREFAB_PATH);
            Assert.That(outcomePopupPrefab, Is.Not.Null, "The production outcome popup prefab must exist at its shipped path.");

            _instance = Object.Instantiate(outcomePopupPrefab);
            _sut = _instance.GetComponent<OutcomePopup>();
            Assert.That(_sut, Is.Not.Null, "The outcome popup prefab must carry the OutcomePopup component.");

            _titleText = _sut.GetComponentInChildren<TMP_Text>();
            Assert.That(_titleText, Is.Not.Null, "The outcome popup prefab must carry its title text.");
        }

        [UnityTearDown]
        public IEnumerator DestroyOutcomePopupInstance()
        {
            if (_instance == null)
            {
                yield break;
            }

            Object.Destroy(_instance);
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _instance == null,
                "The outcome popup fixture should be destroyed after the test.");
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
