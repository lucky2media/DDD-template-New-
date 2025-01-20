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
        [SerializeField] private Sprite winSprite;
        [SerializeField] private Sprite loseSprite;
        [SerializeField] private Button playAgain;
        [SerializeField] private DDDRockPaperScissorsManager manager;
        public void Show(bool win)
        {
            screen.SetActive(true);
            if (win)
            {
                banner.sprite = winSprite;
            }
            else
            {
                banner.sprite = loseSprite;
            }
            playAgain.interactable = true;
            playAgain.onClick.RemoveAllListeners();
            playAgain.onClick.AddListener((() =>
            {
               manager.ResetGame();
               screen.SetActive(false);
            }));
        }
    }
}