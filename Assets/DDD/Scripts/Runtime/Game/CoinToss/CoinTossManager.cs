using System;
using DDD.Scripts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace DDD.Game.CoinToss
{
    public class CoinTossManager : DDDMonoBehaviour
    {
        #region Events

        public event Action<bool,long,string> OnCoinFlipCompleted;
        public event Action<bool> OnPlayerWinStateChanged;

        #endregion

        #region Serialized Fields
        


        [FormerlySerializedAs("networkManager")] [Header("Dependencies")] [SerializeField]
        private DDDNetworkManager dddNetworkManager;

        #endregion

        #region Properties

        public int CurrentScore => currentScore;
        public bool IsGameInProgress => isFlipping;

        #endregion

        #region Private Fields

        private bool playerCoinSideChoiceHeads;
        private int currentScore;
        private bool isFlipping;

        #endregion


        #region Public Methods

        public void HandleHeadsSelection()
        {
            ProcessPlayerChoice(true);
        }

        public void HandleTailsSelection()
        {
            ProcessPlayerChoice(false);
        }

        public string GetGameState()
        {
            return $"Current Score: {currentScore}\nReady for next flip: {!isFlipping}";
        }

        #endregion

        #region Private Methods

        private void ProcessPlayerChoice(bool isHeads)
        {
            if (isFlipping) return;

            int currentBet = BetManager.instance.betAmount;
            dddNetworkManager.SetBet(currentBet, HandleBetResponse);
            playerCoinSideChoiceHeads = isHeads;

            Debug.Log($"Player selected: {(isHeads ? "Heads" : "Tails")}");
        }


        private void HandleBetResponse(BetResponse response)
        {
            if (!response.success)
            {
                HandleBetError(response.success.ToString());
                return;
            }

            ProcessGameOutcome(response.winAmount);
            StartCoinFlip();
        }

        private void ProcessGameOutcome(long winAmount)
        {
            if (winAmount > 0)
            {
                HandleWin(winAmount);
            }
            else
            {
                HandleLoss(winAmount);
            }
        }

        private void HandleWin(long amount)
        {
            Debug.Log($"Win amount: {amount}");
            OnCoinFlipCompleted.Invoke(playerCoinSideChoiceHeads,amount,"Win");
        }

        private void HandleLoss(long amount)
        {
            Debug.Log($"Loss amount: {amount}");
            OnCoinFlipCompleted.Invoke(playerCoinSideChoiceHeads,amount,"Lose");
        }

    

        private void HandleBetError(string error)
        {
            Debug.LogError($"Bet failed: {error}");
        }

        private void StartCoinFlip()
        {
            Manager.EventsManager.InvokeEvent(DDDEventNames.OnUserBalanceChanged, 2);
            return;
        }

        #endregion
    }
}