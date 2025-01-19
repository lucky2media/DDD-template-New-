using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DDD.Scripts.Core;
using DDD.Scripts.Game.rock_paper_scissors;
using UnityEngine;

namespace DDD.Scripts.Game.RockPaperScissors
{
    public class DDDRockPaperScissorsManager : DDDMonoBehaviour
    {
        [SerializeField] private GameObject rockVisual;
        [SerializeField] private GameObject paperVisual;
        [SerializeField] private GameObject scissorsVisual;
        [SerializeField] private EndOfGamePopup endOfGamePopup;

        private GameState currentState;
        private readonly List<Choice> playerChoices = new();
        private List<Choice> botChoices = new();
        private readonly System.Random random = new();

        private void Start()
        {
            InitializeGame();
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
            if (!IsValidPlayerMove(choice)) return;

            switch (currentState)
            {
                case GameState.PlayerPickFirst:
                    HandleFirstPlayerTurn(choice, callback);
                    break;
                case GameState.PlayerPickSecond:
                    HandleSecondPlayerTurn(choice, callback);
                    break;
            }
        }

        private bool IsValidPlayerMove(Choice choice)
        {
            if (currentState == GameState.PlayerPickFirst)
            {
                return !playerChoices.Contains(choice) && playerChoices.Count < 2;
            }
            
            if (currentState == GameState.PlayerPickSecond)
            {
                return playerChoices.Contains(choice);
            }

            return false;
        }

        private void HandleFirstPlayerTurn(Choice choice, Action<GameState> callback)
        {
            playerChoices.Add(choice);
            callback?.Invoke(currentState);

            if (playerChoices.Count == 2)
            {
                TransitionToState(GameState.BotPickFirst);
                StartCoroutine(ExecuteBotFirstTurn());
            }
        }

        private void HandleSecondPlayerTurn(Choice choice, Action<GameState> callback)
        {
            playerChoices.Remove(choice);
            callback?.Invoke(currentState);
            StartCoroutine(ExecuteBotSecondTurn());
        }

        private IEnumerator ExecuteBotFirstTurn()
        {
            botChoices = GenerateBotChoices(2);
            yield return new WaitForSeconds(1f);

            foreach (var choice in botChoices)
            {
                ShowVisualForChoice(choice, true);
                yield return new WaitForSeconds(1f);
            }

            TransitionToState(GameState.PlayerPickSecond);
        }

        private IEnumerator ExecuteBotSecondTurn()
        {
            var selectedChoice = botChoices[random.Next(botChoices.Count)];
            ShowVisualForChoice(selectedChoice, false);
            botChoices.Remove(selectedChoice);
            
            yield return new WaitForSeconds(1f);
            
            bool playerWins = DetermineWinner(playerChoices[0], botChoices[0]);
            endOfGamePopup.Show(playerWins);
            TransitionToState(GameState.EndGame);
        }

        private List<Choice> GenerateBotChoices(int amount)
        {
            return Enum.GetValues(typeof(Choice))
                .Cast<Choice>()
                .OrderBy(_ => random.Next())
                .Take(amount)
                .ToList();
        }

        private void ShowVisualForChoice(Choice choice, bool show)
        {
            GameObject visual = choice switch
            {
                Choice.Rock => rockVisual,
                Choice.Paper => paperVisual,
                Choice.Scissors => scissorsVisual,
                _ => throw new ArgumentOutOfRangeException(nameof(choice))
            };
            
            visual.SetActive(show);
        }

        private void HideAllVisuals()
        {
            rockVisual.SetActive(false);
            paperVisual.SetActive(false);
            scissorsVisual.SetActive(false);
        }

        private static bool DetermineWinner(Choice player, Choice bot)
        {
            if (player == bot) return false;

            return (player, bot) switch
            {
                (Choice.Rock, Choice.Scissors) => true,
                (Choice.Paper, Choice.Rock) => true,
                (Choice.Scissors, Choice.Paper) => true,
                _ => false
            };
        }

        private void TransitionToState(GameState newState)
        {
            currentState = newState;
        }
    }

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
        BotPickSecond,
        EndGame
    }
}