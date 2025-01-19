using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DDD.Scripts.Core;
using UnityEngine;
using Random = System.Random;


namespace DDD.Scripts.Game.rock_paper_scissors
{
    public class DDDRockPaperScissorsManager : DDDMonoBehaviour
    {
        [SerializeField] List<PlayerChoice> playerChoices = new List<PlayerChoice>();
        [SerializeField] RockPaperScissorsState gameState;
        [SerializeField]  List<PlayerChoice> botChoice = new List<PlayerChoice>();
        [SerializeField] GameObject rockVis, paperVis, scissorsVis;

        public void SelectItem(PlayerChoice choice, Action<RockPaperScissorsState> callback)
        {
            Debug.Log(choice);
            if (gameState == RockPaperScissorsState.PlayerPickFirst)
            {
                Debug.Log($"try to select {choice}");
                if (playerChoices.Contains(choice)) return;
                if (playerChoices.Count < 2)
                {
                    playerChoices.Add(choice);
                    Debug.Log($" select {choice}");
                    callback?.Invoke(gameState);
                }

                if (playerChoices.Count == 2)
                {
                    Debug.Log($" end first turn");
                    ChangeState(RockPaperScissorsState.BotPickFirst);
                    StartCoroutine(BotFirstMove());
                }
            }

            if (gameState == RockPaperScissorsState.PlayerPickSecond)
            {
                Debug.Log(RockPaperScissorsState.BotPickFirst.ToString());
                Debug.Log($"try to select {choice}");
                if (playerChoices.Contains(choice))
                {
                    playerChoices.Remove(choice);
                    callback?.Invoke(gameState);
                    StartCoroutine(BotSecondMove());
                }
            }
        }

        private IEnumerator BotFirstMove()
        {
            botChoice = BotChoices();
            yield return new WaitForSeconds(1);
            for (int i = 0; i < botChoice.Count; i++)
            {
                switch (botChoice[i])
                {
                    case PlayerChoice.Rock:
                        rockVis.SetActive(true);
                        break;
                    case PlayerChoice.Paper:
                        paperVis.SetActive(true);
                        break;
                    case PlayerChoice.Scissors:
                        scissorsVis.SetActive(true);
                        break;
                }
                yield return new WaitForSeconds(1f);
            }

            ChangeState(RockPaperScissorsState.PlayerPickSecond);
        }

        public List<PlayerChoice> BotChoices()
        {
            Random random = new Random();
            var uniqueChoices = Enum.GetValues(typeof(PlayerChoice))
                .Cast<PlayerChoice>()
                .OrderBy(x => random.Next())
                .Distinct()
                .Take(2)
                .ToList();
            return uniqueChoices;
        }
        public List<PlayerChoice> BotChoices(int amount)
        {
            Random random = new Random();
            var uniqueChoices = Enum.GetValues(typeof(PlayerChoice))
                .Cast<PlayerChoice>()
                .OrderBy(x => random.Next())
                .Distinct()
                .Take(amount)
                .ToList();
            return uniqueChoices;
        }

        private IEnumerator BotSecondMove()
        {
            var ran =  botChoice.GetRandom();
            yield return new WaitForSeconds(1);
            switch (ran)
            {
                case PlayerChoice.Rock:
                    rockVis.SetActive(false);
                    break;
                case PlayerChoice.Paper:
                    paperVis.SetActive(false);
                    break;
                case PlayerChoice.Scissors:
                    scissorsVis.SetActive(false);
                    break;
               
            }
            botChoice.Remove(ran);
            
            yield return new WaitForSeconds(1);
           var t= DetermineWinner(playerChoices.First(),botChoice.First());
           Debug.Log(t);
        }

        void ChangeState(RockPaperScissorsState state)
        {
            gameState = state;
        }
        
        private string DetermineWinner(PlayerChoice player1, PlayerChoice player2)
        {
            if (player1 == player2)
                return "It's a tie!";

            bool player1Wins = (player1 == PlayerChoice.Rock && player2 == PlayerChoice.Scissors) ||
                               (player1 == PlayerChoice.Paper && player2 == PlayerChoice.Rock) ||
                               (player1 == PlayerChoice.Scissors && player2 == PlayerChoice.Paper);

            return player1Wins ? "Player 1 Wins!" : "Player 2 Wins!";
        }
    }

    public enum PlayerChoice
    {
        Rock,
        Paper,
        Scissors
    }
    
    

    public enum RockPaperScissorsState
    {
        PlayerPickFirst,
        BotPickFirst,
        PlayerPickSecond,
        BotPickSecond,
        EndGame
    }
}