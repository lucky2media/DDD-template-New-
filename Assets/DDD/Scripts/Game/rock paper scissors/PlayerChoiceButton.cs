using DDD.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DDD.Scripts.Game.rock_paper_scissors
{
    public class PlayerChoiceButton : DDDMonoBehaviour
    {
        public PlayerChoice choice;
        public Button me;
        public DDDRockPaperScissorsManager manager;
        public GameObject vis;
        private void Start()
        {
            me.onClick.RemoveAllListeners();
            me.onClick.AddListener((() =>
            {
                manager.SelectItem(choice,((state) =>
                {
                    if (state == RockPaperScissorsState.PlayerPickFirst)
                    {
                        vis.SetActive(true);     
                    }
                    else if(state == RockPaperScissorsState.PlayerPickSecond)
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