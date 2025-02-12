using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DDD.Game.CoinToss
{
    public class CoinFlipAnimation : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image headsSideImage;
        [SerializeField] private Image tailsSideImage;
    
        [Header("Animation Settings")]
        [SerializeField] private float flipDuration = 1f;
        [SerializeField] private float rotations = 5f; // Number of full rotations
        [SerializeField] private float startScale = 1f;
        [SerializeField] private float midScale = 1.5f; // Scale at the peak of the flip
    
        private bool isAnimating = false;

        public void StartFlip(bool landOnHeads, System.Action onComplete = null)
        {
            if (isAnimating) return;
        
            isAnimating = true;
            StartCoroutine(FlipCoinRoutine(landOnHeads, onComplete));
        }

        private IEnumerator FlipCoinRoutine(bool landOnHeads, System.Action onComplete)
        {
            headsSideImage.gameObject.SetActive(true);
            tailsSideImage.gameObject.SetActive(true);
        
            headsSideImage.transform.localScale = Vector3.one * startScale;
            tailsSideImage.transform.localScale = Vector3.one * startScale;
            headsSideImage.transform.rotation = Quaternion.identity;
            tailsSideImage.transform.rotation = Quaternion.identity;

            Sequence flipSequence = DOTween.Sequence();

            flipSequence.Join(headsSideImage.transform.DOScale(midScale, flipDuration / 2f))
                .Join(tailsSideImage.transform.DOScale(midScale, flipDuration / 2f))
                .SetEase(Ease.OutQuad);
        
            flipSequence.Append(headsSideImage.transform.DOScale(startScale, flipDuration / 2f))
                .Join(tailsSideImage.transform.DOScale(startScale, flipDuration / 2f))
                .SetEase(Ease.InQuad);

            float targetRotation = 360f * rotations + (landOnHeads ? 0f : 180f);

            flipSequence.Join(headsSideImage.transform.DORotate(new Vector3(0, targetRotation, 0), flipDuration, RotateMode.FastBeyond360))
                .Join(tailsSideImage.transform.DORotate(new Vector3(0, targetRotation, 0), flipDuration, RotateMode.FastBeyond360))
                .SetEase(Ease.InOutQuad);

            flipSequence.Play();

            yield return new WaitForSeconds(flipDuration);

            headsSideImage.gameObject.SetActive(landOnHeads);
            tailsSideImage.gameObject.SetActive(!landOnHeads);

            isAnimating = false;
            onComplete?.Invoke();
        }

        private void OnDestroy()
        {
            DOTween.Kill(headsSideImage.transform);
            DOTween.Kill(tailsSideImage.transform);
        }

        public void TestFlip()
        {
            bool randomResult = Random.value > 0.5f;
            StartFlip(randomResult, () => Debug.Log($"Landed on {(randomResult ? "Heads" : "Tails")}"));
        }
    }
}