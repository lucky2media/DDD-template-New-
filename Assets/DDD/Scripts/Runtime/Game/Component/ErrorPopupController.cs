using System.Collections;
using UnityEngine;
using TMPro;
using DDD.Scripts.Core;
using UnityEngine.UI;

namespace DDD.Scripts.Game.UI
{
    public class ErrorPopupController : DDDMonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform popupRect;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private Image backgroundImage;

        [Header("Animation Settings")]
        [SerializeField] private float appearDuration = 0.3f;
        [SerializeField] private float displayDuration = 2.0f;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float bounceHeight = 30f;
        [SerializeField] private AnimationCurve bounceCurve;
        [SerializeField] private Color errorColor = new Color(0.9f, 0.2f, 0.2f, 0.9f);

        [Header("Shake Animation")]
        [SerializeField] private float shakeAmount = 5f;
        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private int shakeVibrato = 10;

        private Vector2 originalPosition;
        private Coroutine currentAnimation;
        public static ErrorPopupController instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            
            originalPosition = popupRect.anchoredPosition;
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

       
        public void ShowError(string message)
        {
            // Stop any existing animation
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }

            // Enable the game object
            gameObject.SetActive(true);

            // Set the message
            errorText.text = message;

            // Start the animation sequence
            currentAnimation = StartCoroutine(AnimateError());
        }

        private IEnumerator AnimateError()
        {
            popupRect.anchoredPosition = originalPosition - new Vector2(0, 50f);
            backgroundImage.color = errorColor;

            float elapsedTime = 0f;
            while (elapsedTime < appearDuration)
            {
                float normalizedTime = elapsedTime / appearDuration;
                canvasGroup.alpha = normalizedTime;
                
                float bounce = bounceCurve.Evaluate(normalizedTime) * bounceHeight;
                popupRect.anchoredPosition = originalPosition + new Vector2(0, bounce);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure fully visible
            canvasGroup.alpha = 1f;
            popupRect.anchoredPosition = originalPosition;

            // Shake animation
            yield return StartCoroutine(ShakeAnimation());

            // Wait for display duration
            yield return new WaitForSeconds(displayDuration);

            // Fade out
            elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                canvasGroup.alpha = 1f - (elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private IEnumerator ShakeAnimation()
        {
            Vector2 startPos = popupRect.anchoredPosition;
            float elapsed = 0f;
            
            while (elapsed < shakeDuration)
            {
                float normalizedTime = elapsed / shakeDuration;
                float strength = shakeAmount * (1f - normalizedTime);
                
                // Apply vibrato-based shake
                float x = startPos.x + Random.Range(-strength, strength);
                float y = startPos.y + Random.Range(-strength, strength);
                
                popupRect.anchoredPosition = new Vector2(x, y);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Reset to original position
            popupRect.anchoredPosition = startPos;
        }
    }
}