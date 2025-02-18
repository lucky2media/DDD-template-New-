using DDD.Scripts.Core;
using DDD.Scripts.Game.RockPaperScissors;
using DDD.Scripts.Lobby;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DDD.Scripts.Game.rock_paper_scissors
{
    public class EndOfGamePopup : DDDMonoBehaviour
    {
        [SerializeField] GameObject screen;
        [SerializeField] Image banner;
        [SerializeField] private GameObject winSprite;
        [SerializeField] private GameObject loseSprite;
        [SerializeField] private GameObject drawSprite;
        [SerializeField] private Button playAgain;
        [SerializeField] private DDDRockPaperScissorsManager manager;
        public void Show(GameResult result)
        {
            HideAllHeadlines();
            screen.SetActive(true);
            playAgain.gameObject.SetActive(true);
            if (result == GameResult.Win)
            {
                winSprite.SetActive(true);
            }
            else if (result == GameResult.Lose)
            {
              loseSprite.SetActive(true);
            }
            else
            {
                drawSprite.SetActive(true);
            }
            playAgain.interactable = true;
            playAgain.onClick.RemoveAllListeners();
            playAgain.onClick.AddListener((() =>
            {
               manager.ResetGame();
               screen.SetActive(false);
            }));
        }

        public void HideAllHeadlines()
        {
            winSprite.SetActive(false);
            loseSprite.SetActive(false);
            drawSprite.SetActive(false);
        }

        public void Hide()
        {
            playAgain.gameObject.SetActive(false);
        }
    }

    public enum GameResult
    {
        Win = 0,
        Lose = 1,
        draw = 2
        
    }
}