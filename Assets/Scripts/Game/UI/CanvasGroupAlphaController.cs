using System;
using System.Collections;
using UnityEngine;

namespace Game.UI
{
    public class CanvasGroupAlphaController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        private Coroutine _appearanceCoroutine;

        public void Show(bool val)
        {
            if (_appearanceCoroutine != null) 
                StopCoroutine(_appearanceCoroutine);
            _appearanceCoroutine = StartCoroutine(ShowCo(val));
        }

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        IEnumerator ShowCo(bool val)
        {
            float targetA = val ? 1f : 0;
            float startA = 1f - targetA;
            float percent = Mathf.InverseLerp(startA, targetA, canvasGroup.alpha);
            float duration = 0.5f;
            while (percent < 1f)
            {
                yield return 0f;
                percent += Time.deltaTime / duration;
                canvasGroup.alpha = Mathf.Lerp(startA, targetA, percent);
            }
        }
    }
}