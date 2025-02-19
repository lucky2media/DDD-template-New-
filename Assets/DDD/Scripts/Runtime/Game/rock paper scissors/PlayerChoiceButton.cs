using System;
using DDD.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace DDD.Scripts.Game.RockPaperScissors
{
    public class PlayerChoiceButton : DDDMonoBehaviour
    {
        #region Constants
        private static readonly Color SELECTED_COLOR = Color.green;
        private static readonly Color DEFAULT_COLOR = Color.white;
        #endregion

        #region Serialized Fields
        [FormerlySerializedAs("choice")]
        [SerializeField] private DDDRockPaperScissorsManager.Choice _choice;

        [FormerlySerializedAs("me")]
        [SerializeField] private Button _button;

        [FormerlySerializedAs("manager")]
        [SerializeField] private DDDRockPaperScissorsManager _manager;

        [FormerlySerializedAs("vis")]
        [SerializeField] private GameObject _visualIndicator;
        #endregion

        #region Events
        public event Action<DDDRockPaperScissorsManager.Choice, bool> OnButtonClicked;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            InitializeButton();
            BindEvents();
        }

        private void OnDestroy()
        {
            UnbindEvents();
        }
        #endregion

        #region Initialization
        private void InitializeButton()
        {
            if (_button == null)
            {
                DDDDebug.LogError($"[{nameof(PlayerChoiceButton)}] Button reference is missing!");
                return;
            }

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(HandleButtonClick);
        }

        private void BindEvents()
        {
            if (_manager != null)
            {
                _manager.OnResetGame += Deselect;
            }
            else
            {
                DDDDebug.LogError($"[{nameof(PlayerChoiceButton)}] Manager reference is missing!");
            }
        }

        private void UnbindEvents()
        {
            if (_manager != null)
            {
                _manager.OnResetGame -= Deselect;
            }
        }
        #endregion

        #region Button Handlers
        private void HandleButtonClick()
        {
            if (_manager == null || _button == null) return;

            _manager.HandlePlayerChoice(_choice, HandleStateCallback);
        }

        private void HandleStateCallback(DDDRockPaperScissorsManager.GameState state)
        {
            switch (state)
            {
                case DDDRockPaperScissorsManager.GameState.PlayerPickFirst:
                    SelectButton();
                    break;
                case DDDRockPaperScissorsManager.GameState.PlayerPickSecond:
                    Deselect();
                    break;
            }
        }
        #endregion

        #region Visual State
        private void SelectButton()
        {
            if (_button != null)
            {
                _button.image.color = SELECTED_COLOR;
            }

            if (_visualIndicator != null)
            {
                _visualIndicator.SetActive(true);
            }
        }

        public void Deselect()
        {
            if (_button != null)
            {
                _button.image.color = DEFAULT_COLOR;
            }

            OnButtonClicked?.Invoke(_choice, true);
            
        }
        #endregion
    }
}