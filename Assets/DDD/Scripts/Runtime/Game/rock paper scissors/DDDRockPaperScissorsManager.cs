using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DDD.Game;
using DDD.Scripts.Core;
using DDD.Scripts.Game.rock_paper_scissors;
using DDD.Scripts.Game.rock_paper_scissors.Network;
using DDD.Scripts.Game.UI;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace DDD.Scripts.Game.RockPaperScissors
{
    public class DDDRockPaperScissorsManager : DDDMonoBehaviour
    {
        #region Constants

        private const float BOT_TURN_DELAY = 1f;
        private const int REQUIRED_CHOICES = 2;
        private const string NETWORK_GAME_TYPE = "coins";

        private const string ERROR_NETWORK_MANAGER =
            "[Network] Network manager instance is missing or of incorrect type.";

        private const string ERROR_NETWORK_INIT = "[Network] Init request failed.";
        private const string ERROR_NETWORK_REMOVE = "[Network] Remove-hand request failed.";
        private const string ERROR_INVALID_STATE = "[Game] Invalid game state for player choice.";
        private const string LOG_SESSION_INIT = "[Network] Session initialized: {0}";

        #endregion

        #region Serialized Fields

        [Header("Player Visuals")] [SerializeField]
        private Sprite _rockVisualPlayer;

        [SerializeField] private Sprite _paperVisualPlayer;
        [SerializeField] private Sprite _scissorsVisualPlayer;
        [SerializeField] private Image _firstPickPlayer;
        [SerializeField] private Image _secondPickPlayer;

        [Header("Bot Visuals")] [SerializeField]
        private Sprite _rockVisualBot;

        [SerializeField] private Sprite _paperVisualBot;
        [SerializeField] private Sprite _scissorsVisualBot;
        [SerializeField] private Image _firstPickBot;
        [SerializeField] private Image _secondPickBot;

        [Header("UI Elements")] [SerializeField]
        private EndOfGamePopup _endOfGamePopup;

        [SerializeField] private PlayerChoiceButton[] _playerChoiceButtons;
        [SerializeField] private CanvasGroup _betScreen;
        [SerializeField] private int defaultBetAmount = 100;

        #endregion

        #region Private Fields

        private Sprite _blackHandsSprite;
        private GameState _currentState;
        private readonly List<Choice> _playerChoices = new List<Choice>();
        private readonly List<Choice> _botChoices = new List<Choice>();
        private DDDNetworkManagerRockPeper _networkManager;
        [SerializeField] private bool isBetInOn;
        public static string _sessionId;
        bool gameIsOn = false;
        [SerializeField] private GameObject blocker;
        #endregion

        public List<string> PlayerChoices
        {
            get
            {
                List<string> playerChoices = new List<string>();
                foreach (Choice choice in _playerChoices)
                {
                    playerChoices.Add(choice.ToString().ToLower());
                }

                return playerChoices;
            }
        }

        #region Events

        public event Action OnResetGame;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializeReferences();
            SetupBetScreen();
            SetupNetwork();
            InitializeGame();
            BindEvents();
            blocker.SetActive(false);
            ValidateVisualComponents();
            Manager.EventsManager.AddListener(DDDEventNames.OnCurrencyChanged, InitGame);
        }
        
        private void InitGame(object obj)
        {
            InitializeNetworkSession();
        }

        private void OnDestroy()
        {
            UnbindEvents();
        }

        private void InitializeReferences()
        {
            _blackHandsSprite = _firstPickPlayer.sprite;
        }

        private void SetupBetScreen()
        {
            _betScreen.alpha = 1;
            _betScreen.interactable = true;
            _betScreen.ignoreParentGroups = true;
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
                Application.Quit();
            }
        }

        [ContextMenu("Initialize Game")]
        private void InitializeNetworkSession()
        {
            Debug.Log("[Network] Initializing session with mode: " + NETWORK_GAME_TYPE);
            _networkManager.CallInitRequest(BetManager.instance.currencyType.ToString(), OnNetworkInitialized);
        }

        private void OnNetworkInitialized(string initResponseString)
        {
            Debug.Log("[Network] Init Response: " + initResponseString);

            if (string.IsNullOrEmpty(initResponseString))
            {
                DDDDebug.LogError(ERROR_NETWORK_INIT);
                return;
            }

            try
            {
                var initResponse = JsonConvert.DeserializeObject<RPSMinusOneInitResponseInit>(initResponseString);
                if (initResponse != null && initResponse.success)
                {
                    _sessionId = initResponse.data.sessionId;
                    Debug.Log(string.Format(LOG_SESSION_INIT, _sessionId));
                    BetManager.instance.PopulateBetAmounts(initResponse.data.betsValues.ToList());
                    // PlaceBet();
                }
                else
                {
                    DDDDebug.LogError(ERROR_NETWORK_INIT);
                }
            }
            catch (Exception ex)
            {
                DDDDebug.LogError($"{ERROR_NETWORK_INIT} Error: {ex.Message}");
            }
        }

        private void PlaceBet(Action callback)
        {
            Debug.Log("[Network] Placing bet of: " + defaultBetAmount);
            _networkManager.BetRequest(defaultBetAmount, ((response, action) =>
            {
                var t = JsonConvert.DeserializeObject<BetResponse>(response);
                if (t.success)
                {
                    Debug.Log("[Network] Success!");
                    Manager.EventsManager.InvokeEvent(DDDEventNames.OnUserBalanceChanged, null);
                    callback?.Invoke();
                    gameIsOn = true;
                    blocker.SetActive(true);
                }
                else
                {
                    ErrorPopupController.instance.ShowError("no more coins go buy");
                    Debug.Log("we are need to speek");
                }
            }));
        }

        private void OnBetPlaced(string response, Action callback)
        {
            var t = JsonConvert.DeserializeObject<BetResponse>(response);
            if (t.success)
            {
                Debug.Log("[Network] Success!");
                Manager.EventsManager.InvokeEvent(DDDEventNames.OnUserBalanceChanged, null);
                callback?.Invoke();
            }
            else
            {
                Debug.Log("we are need to speek");
            }
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
            Debug.Log("[Game] Resetting game");
            InitializeGame();
            gameIsOn = false;
            blocker.SetActive(false);
            //OnResetGame?.Invoke();
        }

        private void InitializeGame()
        {
            _currentState = GameState.PlayerPickFirst;
            _playerChoices.Clear();
            _botChoices.Clear();
            ResetVisuals();
            Debug.Log("[Game] Game initialized, state: " + _currentState);
        }

        public void HandlePlayerChoice(Choice choice, Action<GameState> callback)
        {
            Debug.Log("[Game] Player chosen: " + choice + ", current state: " + _currentState);

            switch (_currentState)
            {
                case GameState.PlayerPickFirst:
                    if (_playerChoices.Count < 1)
                    {
                        HandleFirstPlayerChoice(choice, callback);
                    }

                    break;
                case GameState.PlayerPickSecond:
                    if (_playerChoices.Count < 2 && !_playerChoices.Contains(choice))
                    {
                        HandleSecondPlayerChoice(choice, callback);
                    }

                    break;
                case GameState.PlayerRemoveHand:
                    HandlePlayerRemoveHand(choice, callback);
                    break;
                default:
                    DDDDebug.LogWarning(ERROR_INVALID_STATE);
                    break;
            }
        }

        [ContextMenu("PlaceBet")]
        public void Bet(Action callback)
        {
            PlaceBet( callback);
        }

        private void HandleFirstPlayerChoice(Choice choice, Action<GameState> callback)
        {
            Bet((() =>
            {
                _betScreen.alpha = 0;
                _betScreen.interactable = false;
                _betScreen.ignoreParentGroups = false;

                _playerChoices.Add(choice);
                Debug.Log("[Game] Player first choice: " + choice);

                ShowVisualForChoice(choice, true, isPlayer: true, isFirstPick: true);

                callback?.Invoke(_currentState);

                TransitionToState(GameState.PlayerPickSecond);
            }));
           
        }

        private void HandleSecondPlayerChoice(Choice choice, Action<GameState> callback)
        {
            _playerChoices.Add(choice);
            Debug.Log("[Game] Player second choice: " + choice);

            ShowVisualForChoice(choice, true, isPlayer: true, isFirstPick: false);

            callback?.Invoke(_currentState);

            SendPlayerFirstChoice();
        }

        private void SendPlayerFirstChoice()
        {
            Debug.Log("[Network] Sending first hand: " + _playerChoices[0] + ", sessionId: " + _sessionId);

            _networkManager.FirstPick(_playerChoices[0], _sessionId, firstResponse =>
            {
                Debug.Log("[Network] First hand response: " + JsonConvert.SerializeObject(firstResponse));

                if (firstResponse != null && firstResponse.success && firstResponse.data != null)
                {
                    try
                    {
                        // Get the raw string and use our robust parser
                        string botChoiceString = firstResponse.data.pcHand;
                        Debug.Log("[Game] Bot first choice raw value: " + botChoiceString);

                        // Use our new parsing method instead of Enum.TryParse
                        Choice botFirstChoice = ParseChoice(botChoiceString);
                        _botChoices.Add(botFirstChoice);

                        // Make sure we update on the main thread
                        StartCoroutine(DelayedBotVisualUpdate(botFirstChoice, true));

                        // Now send second choice
                        SendPlayerSecondChoice();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Game] Error processing first choice: {ex.Message}");
                        Debug.LogException(ex);
                    }
                }
                else
                {
                    Debug.LogError("[Network] First pick request failed or invalid response format");
                }
            });
        }

        // Helper method to ensure visuals update on the main thread
        private IEnumerator DelayedBotVisualUpdate(Choice choice, bool isFirstPick)
        {
            yield return null; // Wait for next frame

            // Directly set the image and sprite
            Image targetImage = isFirstPick ? _firstPickBot : _secondPickBot;
            Sprite sprite = GetBotSprite(choice);

            if (targetImage != null && sprite != null)
            {
                targetImage.gameObject.SetActive(true);
                targetImage.sprite = sprite;
                targetImage.enabled = true;

                // Ensure color is fully opaque
                Color color = targetImage.color;
                color.a = 1f;
                targetImage.color = color;

                Debug.Log($"[UI] Bot {(isFirstPick ? "first" : "second")} choice visual set to {choice}");
                Debug.Log(
                    $"[UI] Image active: {targetImage.gameObject.activeInHierarchy}, enabled: {targetImage.enabled}");
            }
            else
            {
                Debug.LogError(
                    $"[UI] Failed to set bot visual. Image: {(targetImage != null)}, Sprite: {(sprite != null)}");
            }
        }

        private void SendPlayerSecondChoice()
        {
            Debug.Log("[Network] Sending second hand: " + _playerChoices[1] + ", sessionId: " + _sessionId);

            _networkManager.SecondPick(_playerChoices[1], _sessionId, secondResponse =>
            {
                Debug.Log("[Network] Second hand response: " + JsonConvert.SerializeObject(secondResponse));

                if (secondResponse != null && secondResponse.success && secondResponse.data != null)
                {
                    try
                    {
                        // Get the raw string and use our robust parser
                        string botChoiceString = secondResponse.data.pcHand;
                        Debug.Log("[Game] Bot second choice raw value: " + botChoiceString);

                        // Use our new parsing method instead of Enum.TryParse
                        Choice botSecondChoice = ParseChoice(botChoiceString);
                        _botChoices.Add(botSecondChoice);

                        // Make sure we update on the main thread
                        StartCoroutine(DelayedBotVisualUpdate(botSecondChoice, false));

                        // Now player needs to choose which hand to remove
                        TransitionToState(GameState.PlayerRemoveHand);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Game] Error processing second choice: {ex.Message}");
                        Debug.LogException(ex);
                    }
                }
                else
                {
                    Debug.LogError("[Network] Second pick request failed or invalid response format");
                }
            });
        }

        private void HandlePlayerRemoveHand(Choice choice, Action<GameState> callback)
        {
            // Make sure the choice is one of the player's hands
            if (!_playerChoices.Contains(choice))
            {
                Debug.LogWarning("[Game] Invalid hand to remove: " + choice);
                return;
            }

            // Remove the hand visually
            HidePlayerChoiceVisual(choice);
            Debug.Log("[Game] Player removed: " + choice);

            // Call callback
            callback?.Invoke(_currentState);

            // Send remove-hand request to server
            string handToRemove = choice.ToString().ToLower();
            Debug.Log("[Network] Sending remove-hand request: " + handToRemove +
                      ", player hands: " + string.Join(",", PlayerChoices) +
                      ", sessionId: " + _sessionId);

            _networkManager.CallRemoveHandRequest(
                PlayerChoices.ToArray(),
                handToRemove,
                _sessionId,
                response =>
                {
                    Debug.Log("[Network] Remove-hand response: " + JsonConvert.SerializeObject(response));
                    ProcessRemoveHandResponse(response, handToRemove);
                }
            );
        }

        private void ProcessRemoveHandResponse(RPSMinusOneRemoveHandResponse response, string playerHandToRemove)
        {
            if (response == null || !response.success || response.data == null)
            {
                Debug.LogError($"[Network] {ERROR_NETWORK_REMOVE} Response: {JsonConvert.SerializeObject(response)}");
                return;
            }

            Debug.Log("[Game] Bot removed hand: " + response.data.pcHandToRemove +
                      ", Win amount: " + response.data.winAmount);

            try
            {
                // Use our robust parser instead of Enum.TryParse
                Choice botRemoved = ParseChoice(response.data.pcHandToRemove);

                // Make sure we update UI on the main thread
                StartCoroutine(FinalizeGameResult(botRemoved, playerHandToRemove, response.data.winAmount));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Game] Error processing remove hand response: {ex.Message}");
                Debug.LogException(ex);
            }
        }

        private IEnumerator FinalizeGameResult(Choice botRemoved, string playerHandToRemove, float winAmount)
        {
            yield return null; // Wait for next frame

            // Hide the bot's removed hand
            HideBotChoiceVisual(botRemoved);

            // Determine remaining hands
            Choice playerRemaining = _playerChoices.First(c => c.ToString().ToLower() != playerHandToRemove);
            Choice botRemaining = _botChoices.First(c => c != botRemoved);

            Debug.Log("[Game] Player remaining hand: " + playerRemaining +
                      ", Bot remaining hand: " + botRemaining);

            // Determine game result
            GameResult result = DetermineWinner(playerRemaining, botRemaining);

            // If server provides a win amount, use it to determine result
            if (winAmount > 0)
            {
                result = winAmount > BetManager.instance.betAmount
                    ? GameResult.Win
                    : (winAmount == BetManager.instance.betAmount ? GameResult.Draw : GameResult.Lose);
            }

            Debug.Log("[Game] Game result: " + result);

            // Show result
            ShowGameResult(result);
        }

        private void ShowGameResult(GameResult result)
        {
            Debug.Log("[Game] Showing result: " + result);
            if (_endOfGamePopup != null)
            {
                _endOfGamePopup.Show(result);
                Manager.EventsManager.InvokeEvent(DDDEventNames.OnUserBalanceChanged, null);
            }

            return;
            TransitionToState(GameState.EndGame);
            ResetVisuals();
            ResetGame();
        }

        #endregion

        #region Visual Methods

        // Enhanced visual display method
        private void ShowVisualForChoice(Choice choice, bool show, bool isPlayer, bool isFirstPick)
        {
            Image targetImage = GetTargetImage(isPlayer, isFirstPick);
            Sprite sprite = isPlayer ? GetPlayerSprite(choice) : GetBotSprite(choice);

            if (targetImage != null && sprite != null)
            {
                // First make sure the gameObject is active to modify it
                targetImage.gameObject.SetActive(true);

                // Set the sprite
                targetImage.sprite = sprite;

                // Make sure it's enabled
                targetImage.enabled = true;

                // Ensure color is fully opaque
                Color color = targetImage.color;
                color.a = 1f;
                targetImage.color = color;

                // Set the active state
                targetImage.gameObject.SetActive(show);

                Debug.Log(
                    $"[UI] {(isPlayer ? "Player" : "Bot")} {(isFirstPick ? "first" : "second")} choice visual set: {choice}, active: {show}");
                Debug.Log(
                    $"[UI] GameObject active: {targetImage.gameObject.activeInHierarchy}, Image enabled: {targetImage.enabled}");
            }
            else
            {
                string error = "[UI] Failed to set visual: ";
                if (targetImage == null) error += "Target image is null. ";
                if (sprite == null) error += "Sprite is null for choice: " + choice;
                Debug.LogError(error);
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

            Debug.Log("[UI] Visuals reset");
        }

        private void SetupImageDefaults(Image image)
        {
            if (image == null) return;

            image.gameObject.SetActive(true);
            image.enabled = true;
            image.sprite = _blackHandsSprite;

            Color c = image.color;
            c.a = 1.0f;
            image.color = c;
        }

        private void HidePlayerChoiceVisual(Choice choice)
        {
            if (_firstPickPlayer != null && _firstPickPlayer.gameObject.activeSelf &&
                _firstPickPlayer.sprite == GetPlayerSprite(choice))
            {
                _firstPickPlayer.gameObject.SetActive(false);
                Debug.Log("[UI] Hiding player first choice: " + choice);
            }
            else if (_secondPickPlayer != null && _secondPickPlayer.gameObject.activeSelf &&
                     _secondPickPlayer.sprite == GetPlayerSprite(choice))
            {
                _secondPickPlayer.gameObject.SetActive(false);
                Debug.Log("[UI] Hiding player second choice: " + choice);
            }
        }

        private void HideBotChoiceVisual(Choice choice)
        {
            if (_firstPickBot != null && _firstPickBot.gameObject.activeSelf &&
                _firstPickBot.sprite == GetBotSprite(choice))
            {
                _firstPickBot.gameObject.SetActive(false);
                Debug.Log("[UI] Hiding bot first choice: " + choice);
            }
            else if (_secondPickBot != null && _secondPickBot.gameObject.activeSelf &&
                     _secondPickBot.sprite == GetBotSprite(choice))
            {
                _secondPickBot.gameObject.SetActive(false);
                Debug.Log("[UI] Hiding bot second choice: " + choice);
            }
            else
            {
                Debug.LogWarning($"[UI] Could not find active bot visual to hide for choice: {choice}");
                // Try harder to find and hide it - sometimes sprite comparison can fail
                if (_botChoices.Count > 0 && _botChoices[0] == choice && _firstPickBot != null)
                {
                    _firstPickBot.gameObject.SetActive(false);
                    Debug.Log("[UI] Forcefully hiding bot first choice based on list position");
                }
                else if (_botChoices.Count > 1 && _botChoices[1] == choice && _secondPickBot != null)
                {
                    _secondPickBot.gameObject.SetActive(false);
                    Debug.Log("[UI] Forcefully hiding bot second choice based on list position");
                }
            }
        }

        private Sprite GetPlayerSprite(Choice choice) => choice switch
        {
            Choice.rock => _rockVisualPlayer,
            Choice.paper => _paperVisualPlayer,
            Choice.scissors => _scissorsVisualPlayer,
            _ => throw new ArgumentOutOfRangeException(nameof(choice))
        };

        private Sprite GetBotSprite(Choice choice) => choice switch
        {
            Choice.rock => _rockVisualBot,
            Choice.paper => _paperVisualBot,
            Choice.scissors => _scissorsVisualBot,
            _ => throw new ArgumentOutOfRangeException(nameof(choice))
        };

        [ContextMenu("ValidateVisualComponents")]
        private void ValidateVisualComponents()
        {
            Debug.Log("=== Validating Visual Components ===");

            // Check player sprites
            Debug.Log($"Player Sprites - Rock: {_rockVisualPlayer != null}, " +
                      $"Paper: {_paperVisualPlayer != null}, " +
                      $"Scissors: {_scissorsVisualPlayer != null}");

            // Check bot sprites
            Debug.Log($"Bot Sprites - Rock: {_rockVisualBot != null}, " +
                      $"Paper: {_paperVisualBot != null}, " +
                      $"Scissors: {_scissorsVisualBot != null}");

            // Check image components
            Debug.Log($"Player Images - First: {_firstPickPlayer != null}, " +
                      $"Second: {_secondPickPlayer != null}");

            Debug.Log($"Bot Images - First: {_firstPickBot != null}, " +
                      $"Second: {_secondPickBot != null}");

            // Validate player image settings
            if (_firstPickPlayer != null)
            {
                Debug.Log($"First Player Image - Active: {_firstPickPlayer.gameObject.activeInHierarchy}, " +
                          $"Enabled: {_firstPickPlayer.enabled}, " +
                          $"Alpha: {_firstPickPlayer.color.a}");
            }

            // Validate bot image settings
            if (_firstPickBot != null)
            {
                Debug.Log($"First Bot Image - Active: {_firstPickBot.gameObject.activeInHierarchy}, " +
                          $"Enabled: {_firstPickBot.enabled}, " +
                          $"Alpha: {_firstPickBot.color.a}");
            }

            Debug.Log("=== Validation Complete ===");
        }

        [ContextMenu("TestVisuals")]
        private void TestVisuals()
        {
            Debug.Log("Testing visuals for all choices");

            // Reset visuals first
            ResetVisuals();

            // Test player visuals
            ShowVisualForChoice(Choice.rock, true, true, true);
            ShowVisualForChoice(Choice.paper, true, true, false);

            // Test bot visuals
            ShowVisualForChoice(Choice.scissors, true, false, true);
            ShowVisualForChoice(Choice.rock, true, false, false);

            Debug.Log("Visual test complete, check inspector to verify");
        }

        #endregion

        #region Event Handlers

        private void OnPlayerChoiceButtonClicked(Choice choice, bool isPlayer)
        {
            HandlePlayerChoice(choice, state => { Debug.Log("[Game] Player choice handled, state: " + state); });
        }

        #endregion

        #region Utility Methods

        private void TransitionToState(GameState newState)
        {
            _currentState = newState;
            Debug.Log("[Game] Transition to state: " + newState);
        }

        // New robust choice parsing method
        private static Choice ParseChoice(string choiceString)
        {
            // Log the raw input for debugging
            Debug.Log($"[Parse] Parsing choice string: '{choiceString}'");

            // Normalize the input: trim whitespace, convert to lowercase
            string normalized = choiceString?.Trim().ToLower();

            // Log normalized version
            Debug.Log($"[Parse] Normalized string: '{normalized}'");

            // Handle null or empty
            if (string.IsNullOrEmpty(normalized))
            {
                Debug.LogError("[Parse] Choice string is null or empty");
                return Choice.rock;
            }

            if (Enum.TryParse<Choice>(normalized, true, out Choice result))
            {
                Debug.Log($"[Parse] Successfully parsed as: {result}");
                return result;
            }

            switch (normalized)
            {
                case "rock":
                case "r":
                case "stone":
                    Debug.Log("[Parse] Matched as rock");
                    return Choice.rock;

                case "paper":
                case "p":
                    Debug.Log("[Parse] Matched as paper");
                    return Choice.paper;

                case "scissors":
                case "s":
                case "scissor":
                    Debug.Log("[Parse] Matched as scissors");
                    return Choice.scissors;

                default:
                    Debug.LogError($"[Parse] Could not parse '{choiceString}' as a valid choice");
                    return Choice.rock; // Default to rock if parsing fails
            }
        }

        private static GameResult DetermineWinner(Choice player, Choice bot)
        {
            if (player == bot)
            {
                return GameResult.Draw;
            }

            return (player, bot) switch
            {
                (Choice.rock, Choice.scissors) => GameResult.Win,
                (Choice.paper, Choice.rock) => GameResult.Win,
                (Choice.scissors, Choice.paper) => GameResult.Win,
                _ => GameResult.Lose
            };
        }

        // Utility method to wait for a delay and then execute an action
        private void WaitForSeconds(float seconds, Action onComplete)
        {
            StartCoroutine(WaitForSecondsCoroutine(seconds, onComplete));
        }

        private IEnumerator WaitForSecondsCoroutine(float seconds, Action onComplete)
        {
            yield return new WaitForSeconds(seconds);
            onComplete?.Invoke();
        }

        #endregion

        #region Enums

        public enum Choice
        {
            rock,
            paper,
            scissors
        }

        public enum GameState
        {
            PlayerPickFirst,
            PlayerPickSecond,
            PlayerRemoveHand,
            EndGame
        }

        #endregion
    }
}