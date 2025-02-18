using System;
using DDD.Scripts.Core;
using DDD.Scripts.Game.RockPaperScissors;
using UnityEngine;
using UnityEngine.UI;

namespace DDD.Scripts.Game.RockPaperScissors
{
    public class PlayerChoiceButton : DDDMonoBehaviour
    {
        public DDDRockPaperScissorsManager.Choice choice;
        public Button me;
        public DDDRockPaperScissorsManager manager;
        public GameObject vis;
        public Action<DDDRockPaperScissorsManager.Choice,bool> click;
        private void Start()
        {
            me.onClick.RemoveAllListeners();
            me.onClick.AddListener((() =>
            {
                manager.HandlePlayerChoice(choice, ((state) =>
                {
                    if (state == DDDRockPaperScissorsManager.GameState.PlayerPickFirst)
                    {
                        me.image.color = Color.green;
                        vis.SetActive(true);
                    }
                    else if (state == DDDRockPaperScissorsManager.GameState.PlayerPickSecond)
                    {
                        Deselect();
                    }
                }));
            }));
            
            manager.OnResetGame += Deselect;
        }

        public void Deselect()
        {
            me.image.color = Color.white;
            click?.Invoke(choice,true);
            //vis.SetActive(false);
        }
    }
}