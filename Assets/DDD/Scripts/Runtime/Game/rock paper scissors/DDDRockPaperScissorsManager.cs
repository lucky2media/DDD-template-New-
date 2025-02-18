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

namespace DDD.Scripts.Game.RockPaperScissors
{
    public class DDDRockPaperScissorsManager : DDDMonoBehaviour
    {
        #region Serialized Fields

        [Header("Player")]
        [SerializeField] private Sprite rockVisualPlayer;
        [SerializeField] private Sprite paperVisualPlayer;
        [SerializeField] private Sprite scissorsVisualPlayer;
        [SerializeField] private Image firstPickPlayer;
        [SerializeField] private Image secondPickPlayer;
        
        [Header("Bot")]
        [SerializeField] private Sprite rockVisualBot;
        [SerializeField] private Sprite paperVisualBot;
        [SerializeField] private Sprite scissorsVisualBot;
        [SerializeField] private Image firstPickBot;
        [SerializeField] private Image secondPickBot;
        
        [Header("UI")]
        [SerializeField] private EndOfGamePopup endOfGamePopup;
        [SerializeField] private PlayerChoiceButton[] playerChoiceButtons;

        #endregion

        #region Private Fields

        private GameState currentState;
        private readonly List<Choice> playerChoices = new();
        private List<Choice> botChoices = new();
        private readonly System.Random random = new();
        private DDDNetworkManagerRockPeper networkManager;
        private string sessionId;

        #endregion

        #region Public Events

        public Action OnResetGame;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Acquire network manager instance.
            if (DDDNetworkManager.Instance is DDDNetworkManagerRockPeper netMgr)
            {
                networkManager = netMgr;
            }
            else
            {
                DDDDebug.LogError("[Network] Network manager instance is missing or of incorrect type.");
            }
            
            InitializeGame();

            // Subscribe to player choice button clicks.
            foreach (var button in playerChoiceButtons)
            {
                button.click += OnPlayerChoiceButtonClicked;
            }

            // Initialize session with the backend.
            networkManager.CallInitRequest("sweeps", (initResponse) =>
            {
                if (initResponse != null)
                {
                    sessionId = initResponse.sessionId;
                    DDDDebug.Log($"[Network] Session initialized: {sessionId}");
                }
                else
                {
                    DDDDebug.LogError("[Network] Init request failed. Falling back to offline mode.");
                }
            });
        }

        #endregion

        #region Player Button Callback

        /// <summary>
        /// Called when a player choice button is clicked.
        /// </summary>
        private void OnPlayerChoiceButtonClicked(Choice choice, bool isPlayer)
        {
            // Hide the visual of the clicked choice.
            if (isPlayer)
            {
                HidePlayerChoiceVisual(choice);
            }
            // (For bot choices, you could add separate behavior if needed.)
        }

        #endregion

        #region Game Flow Methods

        /// <summary>
        /// Resets the game state and UI.
        /// </summary>
        public void ResetGame()
        {
            InitializeGame();
            OnResetGame?.Invoke();
        }


        private void InitializeGame()
        {
            currentState = GameState.PlayerPickFirst;
            playerChoices.Clear();
            botChoices.Clear();
            HideAllVisuals();
        }


        public void HandlePlayerChoice(Choice choice, Action<GameState> callback)
        {
            if (!IsValidPlayerMove(choice))
                return;

            switch (currentState)
            {
                case GameState.PlayerPickFirst:
                    HandleFirstPlayerTurn(choice, callback);
                    break;
                case GameState.PlayerPickSecond:
                    HandleSecondPlayerTurn(choice, callback);
                    break;
                default:
                    DDDDebug.LogWarning("[Game] Invalid game state for player choice.");
                    break;
            }
        }


        private bool IsValidPlayerMove(Choice choice)
        {
            return currentState switch
            {
                GameState.PlayerPickFirst => !playerChoices.Contains(choice) && playerChoices.Count < 2,
                GameState.PlayerPickSecond => playerChoices.Contains(choice),
                _ => false,
            };
        }

        private void HandleFirstPlayerTurn(Choice choice, Action<GameState> callback)
        {
            playerChoices.Add(choice);
            callback?.Invoke(currentState);

            if (playerChoices.Count == 1)
            {
                ShowVisualForChoice(choice, true, isPlayer: true, isFirstPick: true);
            }
            else if (playerChoices.Count == 2)
            {
                ShowVisualForChoice(choice, true, isPlayer: true, isFirstPick: false);
                TransitionToState(GameState.BotPickFirst);
                StartCoroutine(ExecuteBotFirstTurn());
            }
        }

 
        private void HandleSecondPlayerTurn(Choice choice, Action<GameState> callback)
        {
            callback?.Invoke(currentState);

            networkManager.CallRemoveHandRequest(
                choice.ToString().ToLower(), sessionId, (response) =>
                {
                    if (response != null)
                    {
                        ProcessNetworkRemoveHandResponse(response);
                        return;
                    }
                    else
                    {
                        DDDDebug.LogWarning("[Network] Remove-hand request failed, switching to offline mode.");
                        
                    }
                });
            OfflineRemoveHand(choice);
        }


        private void ProcessNetworkRemoveHandResponse(RPSMinusOneRemoveHandResponse response)
        {
            if (Enum.TryParse(FirstCharToUpper(response.pcHandToRemove), out Choice botRemoved))
            {
              
                if (botChoices.Count > 0 && botChoices[0] == botRemoved)
                {
                    ShowVisualForChoice(botRemoved, false, isPlayer: false, isFirstPick: true);
                }
                else if (botChoices.Count > 1 && botChoices[1] == botRemoved)
                {
                    ShowVisualForChoice(botRemoved, false, isPlayer: false, isFirstPick: false);
                }
            }
            bool playerWins = response.winAmount > 0;
            GameResult result = GameResult.draw;
            if (playerWins)
            {
                if (response.winAmount == BetManager.instance.betAmount)
                {
                    result = GameResult.draw;
                }
                else
                {
                    result = GameResult.Win;
                }
            }
            else
            {
                result = GameResult.Lose;
            }
            endOfGamePopup.Show(result);
            TransitionToState(GameState.EndGame);
        }


        private IEnumerator ExecuteBotFirstTurn()
        {
            botChoices = GenerateBotChoices(2);
            yield return new WaitForSeconds(1f);

            for (int i = 0; i < botChoices.Count; i++)
            {
                bool isFirstPick = (i == 0);
                ShowVisualForChoice(botChoices[i], true, isPlayer: false, isFirstPick: isFirstPick);
                yield return new WaitForSeconds(1f);
            }
            TransitionToState(GameState.PlayerPickSecond);
        }

   
        private List<Choice> GenerateBotChoices(int amount)
        {
            return Enum.GetValues(typeof(Choice))
                .Cast<Choice>()
                .OrderBy(_ => random.Next())
                .Take(amount)
                .ToList();
        }


        private void OfflineRemoveHand(Choice playerChoiceToRemove)
        {
            Choice playerRemaining = playerChoices.First(x => x != playerChoiceToRemove);
            int index = random.Next(botChoices.Count);
            Choice botRemoved = botChoices[index];
            botChoices.RemoveAt(index);

            // Hide the removed bot choice.
            if (index == 0)
                ShowVisualForChoice(botRemoved, false, isPlayer: false, isFirstPick: true);
            else
                ShowVisualForChoice(botRemoved, false, isPlayer: false, isFirstPick: false);

            var playerWins = DetermineWinner(playerRemaining, botChoices[0]);
            
            endOfGamePopup.Show(playerWins);
            TransitionToState(GameState.EndGame);
        }

        #endregion

        #region Visual Methods


        private void ShowVisualForChoice(Choice choice, bool show, bool isPlayer, bool isFirstPick)
        {
            Image targetImage = isPlayer
                ? (isFirstPick ? firstPickPlayer : secondPickPlayer)
                : (isFirstPick ? firstPickBot : secondPickBot);

            Sprite sprite = isPlayer ? GetPlayerSprite(choice) : GetBotSprite(choice);

            if (targetImage != null)
            {
                targetImage.sprite = sprite;
                targetImage.gameObject.SetActive(show);
            }
        }


        private void HideAllVisuals()
        {
            firstPickPlayer.gameObject.SetActive(false);
            secondPickPlayer.gameObject.SetActive(false);
            firstPickBot.gameObject.SetActive(false);
            secondPickBot.gameObject.SetActive(false);
        }


        private void HidePlayerChoiceVisual(Choice choice)
        {
            if (firstPickPlayer.gameObject.activeSelf && firstPickPlayer.sprite == GetPlayerSprite(choice))
            {
                firstPickPlayer.gameObject.SetActive(false);
            }
            else if (secondPickPlayer.gameObject.activeSelf && secondPickPlayer.sprite == GetPlayerSprite(choice))
            {
                secondPickPlayer.gameObject.SetActive(false);
            }
        }


        private void HideBotChoiceVisual(Choice choice)
        {
            if (firstPickBot.gameObject.activeSelf && firstPickBot.sprite == GetBotSprite(choice))
            {
                firstPickBot.gameObject.SetActive(false);
            }
            else if (secondPickBot.gameObject.activeSelf && secondPickBot.sprite == GetBotSprite(choice))
            {
                secondPickBot.gameObject.SetActive(false);
            }
        }

        private Sprite GetPlayerSprite(Choice choice)
        {
            return choice switch
            {
                Choice.Rock => rockVisualPlayer,
                Choice.Paper => paperVisualPlayer,
                Choice.Scissors => scissorsVisualPlayer,
                _ => throw new ArgumentOutOfRangeException(nameof(choice))
            };
        }


        private Sprite GetBotSprite(Choice choice)
        {
            return choice switch
            {
                Choice.Rock => rockVisualBot,
                Choice.Paper => paperVisualBot,
                Choice.Scissors => scissorsVisualBot,
                _ => throw new ArgumentOutOfRangeException(nameof(choice))
            };
        }

        #endregion

        #region Utility Methods

 
        private void TransitionToState(GameState newState)
        {
            currentState = newState;
        }


        private static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            return char.ToUpper(input[0]) + input.Substring(1);
        }


        private static GameResult DetermineWinner(Choice player, Choice bot)
        {
            if (player == bot) return  GameResult.draw;
            return (player, bot) switch
            {
                (Choice.Rock, Choice.Scissors) => GameResult.Win,
                (Choice.Paper, Choice.Rock) => GameResult.Win,
                (Choice.Scissors, Choice.Paper) => GameResult.Win,
                _ => GameResult.Lose
            };
        }

        #endregion

        #region Enumerations

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
