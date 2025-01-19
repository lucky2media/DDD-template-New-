using DDD.Scripts.Core;
using DDD.Scripts.Game.RockPaperScissors;
using UnityEngine;
using UnityEngine.UI;

namespace DDD.Scripts.Game.rock_paper_scissors
{
    public class PlayerChoiceButton : DDDMonoBehaviour
    {
        public Choice choice;
        public Button me;
        public DDDRockPaperScissorsManager manager;
        public GameObject vis;
        private void Start()
        {
            me.onClick.RemoveAllListeners();
            me.onClick.AddListener((() =>
            {
                manager.HandlePlayerChoice(choice,((state) =>
                {
                    if (state == GameState.PlayerPickFirst)
                    {
                        vis.SetActive(true);     
                    }
                    else if(state == GameState.PlayerPickSecond)
                    {
                        Deselect();
                    }
                }));
               
            }));
        }

        public void Deselect()
        {
            vis.SetActive(false);
        }
    }
}