using System;
using UnityEngine;
using UnityEngine.UI;

public class OpenUrl : MonoBehaviour
{
    [SerializeField] Button _button;
    [SerializeField] string url;

    private void Awake()
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener((() =>
        {
            Application.OpenURL(url);
        }));
    }
}
