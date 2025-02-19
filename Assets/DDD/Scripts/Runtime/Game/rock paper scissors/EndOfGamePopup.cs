using DDD.Scripts.Core;
using DDD.Scripts.Lobby;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace DDD.Scripts.Game.RockPaperScissors
{
    public class EndOfGamePopup : DDDMonoBehaviour
    {
        #region Serialized Fields
        [FormerlySerializedAs("screen")]
        [SerializeField] private GameObject _popupScreen;

        [FormerlySerializedAs("banner")]
        [SerializeField] private Image _bannerImage;

        [FormerlySerializedAs("winSprite")]
        [SerializeField] private GameObject _winSprite;

        [FormerlySerializedAs("loseSprite")]
        [SerializeField] private GameObject _loseSprite;

        [FormerlySerializedAs("drawSprite")]
        [SerializeField] private GameObject _drawSprite;

        [FormerlySerializedAs("playAgain")]
        [SerializeField] private Button _playAgainButton;

        [FormerlySerializedAs("manager")]
        [SerializeField] private DDDRockPaperScissorsManager _gameManager;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            InitializeButton();
        }

        private void OnDisable()
        {
            CleanupButton();
        }
        #endregion

        #region Public Methods
        public void Show(GameResult result)
        {
            if (!ValidateReferences()) return;

            HideAllHeadlines();
            ShowPopup();
            ShowResultSprite(result);
            SetupPlayAgainButton();
        }

        public void Hide()
        {
            if (_playAgainButton != null)
            {
                _playAgainButton.gameObject.SetActive(false);
            }
        }
        #endregion

        #region Private Methods
        private void InitializeButton()
        {
            if (_playAgainButton != null)
            {
                _playAgainButton.onClick.RemoveAllListeners();
            }
        }

        private void CleanupButton()
        {
            if (_playAgainButton != null)
            {
                _playAgainButton.onClick.RemoveAllListeners();
            }
        }

        private bool ValidateReferences()
        {
            if (_popupScreen == null || _gameManager == null || _playAgainButton == null)
            {
                DDDDebug.LogError($"[{nameof(EndOfGamePopup)}] Missing required references!");
                return false;
            }
            return true;
        }

        private void ShowPopup()
        {
            _popupScreen.SetActive(true);
            _playAgainButton.gameObject.SetActive(true);
        }

        private void ShowResultSprite(GameResult result)
        {
            switch (result)
            {
                case GameResult.Win:
                    DDDAudioManager.instance.PlayWinSound();
                    _winSprite?.SetActive(true);
                    break;
                case GameResult.Lose:
                    DDDAudioManager.instance.PlayLoseSound();
                    _loseSprite?.SetActive(true);
                    break;
                case GameResult.Draw:
                     DDDAudioManager.instance.PlayDrawSound();
                    _drawSprite?.SetActive(true);
                    break;
            }
        }

        private void SetupPlayAgainButton()
        {
            if (_playAgainButton == null) return;

            _playAgainButton.interactable = true;
            _playAgainButton.onClick.RemoveAllListeners();
            _playAgainButton.onClick.AddListener(HandlePlayAgainClick);
        }

        private void HandlePlayAgainClick()
        {
            if (_gameManager != null)
            {
                _gameManager.ResetGame();
            }

            if (_popupScreen != null)
            {
                _popupScreen.SetActive(false);
            }
        }

        private void HideAllHeadlines()
        {
            if (_winSprite != null) _winSprite.SetActive(false);
            if (_loseSprite != null) _loseSprite.SetActive(false);
            if (_drawSprite != null) _drawSprite.SetActive(false);
        }
        #endregion
    }

    public enum GameResult
    {
        Win = 0,
        Lose = 1,
        Draw = 2  // Fixed capitalization to match C# naming conventions
    }
}