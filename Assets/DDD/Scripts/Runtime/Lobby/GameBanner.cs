using DDD.Scripts.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DDD.Scripts.Lobby
{
    public class GameBanner : DDDMonoBehaviour
    {
        string gameName = "game/coin_flip/play";
        string sceneName = "coin_flip";
        [SerializeField] Image banner;
        [SerializeField] Button button;
        public void Init(GameData data)
        {
            gameName = data.gameName;
            sceneName = data.sceneName;
            banner.sprite = data.banner;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener((() =>
            {
                SceneManager.LoadScene(sceneName);
            }));
        }
        
    }
}