using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DDD.Game.CoinToss
{
    public class CoinTossVisuals : MonoBehaviour
    {
        private CoinTossManager tossManager;
        [SerializeField] Button headsButton;
        [SerializeField] Button tailsButton;
        [SerializeField] TextMeshProUGUI betText;
        [SerializeField] TextMeshProUGUI resultText;
        public CoinFlipAnimation coinFlipAnimation;
        void Start()
        {
            tossManager = FindObjectOfType<CoinTossManager>();
            if (tossManager != null)
            {
                tossManager.OnCoinFlipCompleted += (HandleFlipResult);
                tossManager.OnPlayerWinStateChanged += (HandleWinResult);
                headsButton.onClick.AddListener(tossManager.HandleHeadsSelection);
                tailsButton.onClick.AddListener(tossManager.HandleTailsSelection);
            }
        }

        

        private void HandleWinResult(bool playerWon)
        {
            Debug.Log($"Visual Update: Player {(playerWon ? "Won!" : "Lost!")}");
        }

        void OnDestroy()
        {
            if (tossManager != null)
            {
                tossManager.OnCoinFlipCompleted -= (HandleFlipResult);
                tossManager.OnPlayerWinStateChanged -= (HandleWinResult);
            }
        }

        private void HandleFlipResult(bool isHeads, long amount, string result)
        {
            var head = false;
            if (amount > 1)
            {
                head = isHeads;
            }
            else
            {
                head = !isHeads;
            }
            coinFlipAnimation.StartFlip(head, () => 
            {
                
                Debug.Log("Flip animation completed!");
            });
            betText.text = $"Bet: {amount}";
            resultText.text = result;
        }
    }
}