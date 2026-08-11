using UnityEngine;
using DG.Tweening;

namespace HR.UI{
// Generic reusable blink loop for any CanvasGroup (e.g. a blinking
// "Press Any Key" prompt) - alpha yoyos between minAlpha and maxAlpha
// forever while this component is enabled.
public class CanvasGroupBlink : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float duration = 0.8f;
    [SerializeField] float minAlpha = 0f;
    [SerializeField] float maxAlpha = 1f;
    [SerializeField] Ease ease = Ease.InOutSine;

    void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }
    void OnEnable()
    {
        canvasGroup.alpha = minAlpha;
        canvasGroup.DOFade(maxAlpha, duration)
            .SetEase(ease)
            .SetLoops(-1, LoopType.Yoyo);
    }
    void OnDisable()
    {
        canvasGroup.DOKill();
    }
}

}
