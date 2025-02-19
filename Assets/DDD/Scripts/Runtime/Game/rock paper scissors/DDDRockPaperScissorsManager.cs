using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DDD.Game;
using DDD.Scripts.Core;
using DDD.Scripts.Game.rock_paper_scissors;
using DDD.Scripts.Game.rock_paper_scissors.Network;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace DDD.Scripts.Game.RockPaperScissors
{
    
    public class DDDRockPaperScissorsManager : DDDMonoBehaviour
    {
        #region Constants
        private const float BOT_TURN_DELAY = 1f;
        private const int REQUIRED_CHOICES = 2;
        private const string NETWORK_GAME_TYPE = "sweeps";
        private const string ERROR_NETWORK_MANAGER = "[Network] Network manager instance is missing or of incorrect type.";
        private const string ERROR_NETWORK_INIT = "[Network] Init request failed. Falling back to offline mode.";
        private const string ERROR_NETWORK_REMOVE = "[Network] Remove-hand request failed, switching to offline mode.";
        private const string ERROR_INVALID_STATE = "[Game] Invalid game state for player choice.";
        private const string LOG_SESSION_INIT = "[Network] Session initialized: {0}";
        #endregion

        #region Serialized Fields
        [Header("Player Visuals")]
        [FormerlySerializedAs("rockVisualPlayer")]
        [SerializeField] private Sprite _rockVisualPlayer;
        
        [FormerlySerializedAs("paperVisualPlayer")]
        [SerializeField] private Sprite _paperVisualPlayer;
        
        [FormerlySerializedAs("scissorsVisualPlayer")]
        [SerializeField] private Sprite _scissorsVisualPlayer;
        
        [FormerlySerializedAs("firstPickPlayer")]
        [SerializeField] private Image _firstPickPlayer;
        
        [FormerlySerializedAs("secondPickPlayer")]
        [SerializeField] private Image _secondPickPlayer;
        
        [Header("Bot Visuals")]
        [FormerlySerializedAs("rockVisualBot")]
        [SerializeField] private Sprite _rockVisualBot;
        
        [FormerlySerializedAs("paperVisualBot")]
        [SerializeField] private Sprite _paperVisualBot;
        
        [FormerlySerializedAs("scissorsVisualBot")]
        [SerializeField] private Sprite _scissorsVisualBot;
        
        [FormerlySerializedAs("firstPickBot")]
        [SerializeField] private Image _firstPickBot;
        
        [FormerlySerializedAs("secondPickBot")]
        [SerializeField] private Image _secondPickBot;
        
        [Header("UI Elements")]
        [FormerlySerializedAs("endOfGamePopup")]
        [SerializeField] private EndOfGamePopup _endOfGamePopup;
        
        [FormerlySerializedAs("playerChoiceButtons")]
        [SerializeField] private PlayerChoiceButton[] _playerChoiceButtons;
        [SerializeField] private CanvasGroup _betScreen;
        #endregion

        #region Private Fields
        private Sprite _blackHandsSprite;
        private GameState _currentState;
        private readonly List<Choice> _playerChoices = new();
        private List<Choice> _botChoices = new();
        private readonly System.Random _random = new();
        private DDDNetworkManagerRockPeper _networkManager;
        private string _sessionId;
        #endregion

        #region Events
        public event Action OnResetGame;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            InitializeReferences();
            _betScreen.alpha = 1;
            _betScreen.interactable = true;
            _betScreen.ignoreParentGroups = true;
            SetupNetwork();
            InitializeGame();
            BindEvents();
        }

        private void OnDestroy()
        {
            UnbindEvents();
        }

        private void InitializeReferences()
        {
            _blackHandsSprite = _firstPickPlayer.sprite;
        }

        private void SetupNetwork()
        {
            if (DDDNetworkManager.Instance is DDDNetworkManagerRockPeper netMgr)
            {
                _networkManager = netMgr;
                InitializeNetworkSession();
            }
            else
            {
                DDDDebug.LogError(ERROR_NETWORK_MANAGER);
            }
        }

        private void InitializeNetworkSession()
        {
            _networkManager.CallInitRequest(NETWORK_GAME_TYPE, OnNetworkInitialized);
        }

        private void OnNetworkInitialized(RPSMinusOneInitResponse initResponse)
        {
            if (initResponse != null)
            {
                _sessionId = initResponse.sessionId;
                DDDDebug.Log(string.Format(LOG_SESSION_INIT, _sessionId));
                return;
            }
            
            DDDDebug.LogError(ERROR_NETWORK_INIT);
        }

        private void BindEvents()
        {
            if (_playerChoiceButtons == null) return;
            
            foreach (var button in _playerChoiceButtons)
            {
                if (button != null)
                {
                    button.OnButtonClicked += OnPlayerChoiceButtonClicked;
                }
            }
        }

        private void UnbindEvents()
        {
            if (_playerChoiceButtons == null) return;

            foreach (var button in _playerChoiceButtons)
            {
                if (button != null)
                {
                    button.OnButtonClicked -= OnPlayerChoiceButtonClicked;
                }
            }
        }
        #endregion

        #region Game Flow Methods
        public void ResetGame()
        {
            InitializeGame();
            OnResetGame?.Invoke();
        }

        private void InitializeGame()
        {
            _currentState = GameState.PlayerPickFirst;
            _playerChoices.Clear();
            _botChoices.Clear();
            ResetVisuals();
        }

        public void HandlePlayerChoice(Choice choice, Action<GameState> callback)
        {
            if (!IsValidPlayerMove(choice))
            {
                return;
            }

            switch (_currentState)
            {
                case GameState.PlayerPickFirst:
                    HandleFirstPlayerTurn(choice, callback);
                    break;
                case GameState.PlayerPickSecond:
                    HandleSecondPlayerTurn(choice, callback);
                    break;
                default:
                    DDDDebug.LogWarning(ERROR_INVALID_STATE);
                    break;
            }
        }

        private bool IsValidPlayerMove(Choice choice)
        {
            return _currentState switch
            {
                GameState.PlayerPickFirst => !_playerChoices.Contains(choice) && _playerChoices.Count < REQUIRED_CHOICES,
                GameState.PlayerPickSecond => _playerChoices.Contains(choice),
                _ => false,
            };
        }

        private void HandleFirstPlayerTurn(Choice choice, Action<GameState> callback)
        {
            _betScreen.alpha = 0;
            _betScreen.interactable = false;
            _betScreen.ignoreParentGroups = false;
            _playerChoices.Add(choice);
            callback?.Invoke(_currentState);

            bool isFirstPick = _playerChoices.Count == 1;
            ShowVisualForChoice(choice, true, isPlayer: true, isFirstPick);

            if (_playerChoices.Count == REQUIRED_CHOICES)
            {
                TransitionToState(GameState.BotPickFirst);
                StartCoroutine(ExecuteBotFirstTurn());
            }
        }

        private void HandleSecondPlayerTurn(Choice choice, Action<GameState> callback)
        {
            callback?.Invoke(_currentState);

            if (_networkManager != null)
            {
                _networkManager.CallRemoveHandRequest(
                    choice.ToString().ToLower(), 
                    _sessionId, 
                    response =>
                    {
                        ProcessRemoveHandResponse(response, choice);
                        return;
                    });
                HandleOfflineRemoveHand(choice);
            }
            else
            {
                HandleOfflineRemoveHand(choice);
            }
          
        }

        private void ProcessRemoveHandResponse(RPSMinusOneRemoveHandResponse response, Choice playerChoice)
        {
            if (response == null)
            {
                DDDDebug.LogWarning(ERROR_NETWORK_REMOVE);
                HandleOfflineRemoveHand(playerChoice);
                return;
            }

            if (Enum.TryParse(FirstCharToUpper(response.pcHandToRemove), out Choice botRemoved))
            {
                UpdateBotVisuals(botRemoved);
            }
            
           

            GameResult result = DetermineGameResult(response.winAmount);
            ShowGameResult(result);
        }

        private void ShowGameResult(GameResult result)
        {
            if (_endOfGamePopup != null)
            {
                _endOfGamePopup.Show(result);
            }
            TransitionToState(GameState.EndGame);
        }

        private GameResult DetermineGameResult(float winAmount)
        {
            if (winAmount <= 0)
            {
                return GameResult.Lose;
            }

            return winAmount == BetManager.instance.betAmount 
                ? GameResult.Draw 
                : GameResult.Win;
        }

        private IEnumerator ExecuteBotFirstTurn()
        {
            _botChoices = GenerateBotChoices(REQUIRED_CHOICES);
            
            for (int i = 0; i < _botChoices.Count; i++)
            {
                bool isFirstPick = i == 0;
                ShowVisualForChoice(_botChoices[i], true, isPlayer: false, isFirstPick);
                yield return new WaitForSeconds(BOT_TURN_DELAY);
            }
            
            TransitionToState(GameState.PlayerPickSecond);
        }

        private List<Choice> GenerateBotChoices(int amount)
        {
            return Enum.GetValues(typeof(Choice))
                .Cast<Choice>()
                .OrderBy(_ => _random.Next())
                .Take(amount)
                .ToList();
        }

        private void HandleOfflineRemoveHand(Choice playerChoiceToRemove)
        {
            if (!_botChoices.Any()) return;

            Choice playerRemaining = _playerChoices.First(x => x != playerChoiceToRemove);
            int index = _random.Next(_botChoices.Count);
            Choice botRemoved = _botChoices[index];
            _botChoices.RemoveAt(index);

            ShowVisualForChoice(botRemoved, false, isPlayer: false, isFirstPick: index == 0);

            if (_botChoices.Any())
            {
                GameResult result = DetermineWinner(playerRemaining, _botChoices[0]);
                ShowGameResult(result);
            }
        }
        #endregion

        #region Visual Methods
        private void ShowVisualForChoice(Choice choice, bool show, bool isPlayer, bool isFirstPick)
        {
            Image targetImage = GetTargetImage(isPlayer, isFirstPick);
            Sprite sprite = isPlayer ? GetPlayerSprite(choice) : GetBotSprite(choice);

            if (targetImage != null)
            {
                targetImage.sprite = sprite;
                targetImage.gameObject.SetActive(show);
            }
        }

        private Image GetTargetImage(bool isPlayer, bool isFirstPick)
        {
            return isPlayer
                ? (isFirstPick ? _firstPickPlayer : _secondPickPlayer)
                : (isFirstPick ? _firstPickBot : _secondPickBot);
        }

        private void ResetVisuals()
        {
            _betScreen.alpha = 1;
            _betScreen.interactable = true;
            _betScreen.ignoreParentGroups = true;
            SetupImageDefaults(_firstPickPlayer);
            SetupImageDefaults(_secondPickPlayer);
            SetupImageDefaults(_firstPickBot);
            SetupImageDefaults(_secondPickBot);
        }

        private void SetupImageDefaults(Image image)
        {
            if (image == null) return;
            
            image.gameObject.SetActive(true);
            image.sprite = _blackHandsSprite;
        }

        private void UpdateBotVisuals(Choice botRemoved)
        {
            if (_botChoices.Count > 0 && _botChoices[0] == botRemoved)
            {
                ShowVisualForChoice(botRemoved, false, isPlayer: false, isFirstPick: true);
            }
            else if (_botChoices.Count > 1 && _botChoices[1] == botRemoved)
            {
                ShowVisualForChoice(botRemoved, false, isPlayer: false, isFirstPick: false);
            }
        }

        private Sprite GetPlayerSprite(Choice choice) => choice switch
        {
            Choice.Rock => _rockVisualPlayer,
            Choice.Paper => _paperVisualPlayer,
            Choice.Scissors => _scissorsVisualPlayer,
            _ => throw new ArgumentOutOfRangeException(nameof(choice))
        };

        private Sprite GetBotSprite(Choice choice) => choice switch
        {
            Choice.Rock => _rockVisualBot,
            Choice.Paper => _paperVisualBot,
            Choice.Scissors => _scissorsVisualBot,
            _ => throw new ArgumentOutOfRangeException(nameof(choice))
        };
        #endregion

        #region Event Handlers
        private void OnPlayerChoiceButtonClicked(Choice choice, bool isPlayer)
        {
            if (isPlayer)
            {
                HidePlayerChoiceVisual(choice);
            }
        }

        private void HidePlayerChoiceVisual(Choice choice)
        {
            if (_firstPickPlayer != null && _firstPickPlayer.gameObject.activeSelf && 
                _firstPickPlayer.sprite == GetPlayerSprite(choice))
            {
                _firstPickPlayer.gameObject.SetActive(false);
            }
            else if (_secondPickPlayer != null && _secondPickPlayer.gameObject.activeSelf && 
                     _secondPickPlayer.sprite == GetPlayerSprite(choice))
            {
                _secondPickPlayer.gameObject.SetActive(false);
            }
        }
        #endregion

        #region Utility Methods
        private void TransitionToState(GameState newState)
        {
            _currentState = newState;
        }

        private static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        private static GameResult DetermineWinner(Choice player, Choice bot)
        {
            if (player == bot)
            {
                return GameResult.Draw;
            }

            return (player, bot) switch
            {
                (Choice.Rock, Choice.Scissors) => GameResult.Win,
                (Choice.Paper, Choice.Rock) => GameResult.Win,
                (Choice.Scissors, Choice.Paper) => GameResult.Win,
                _ => GameResult.Lose
            };
        }
        #endregion

        #region Enums
        public enum Choice
        {
            Rock,
            Paper,
            Scissors
        }

        public enum GameState
        {
            PlayerPickFirst,
            BotPickFirst,
            PlayerPickSecond,
            EndGame
        }
        #endregion
    }
}