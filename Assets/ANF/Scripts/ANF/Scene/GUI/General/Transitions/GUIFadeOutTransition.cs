using DG.Tweening;
using UnityEngine;

namespace ANF.GUI
{
    /// <summary>
    /// A transition that consists in fading the entire panel
    /// </summary>
    [System.Serializable]
    public class GUIFadeInOutTransition : GUIInOutTransition
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private bool blockRaycastsIfInactive = false;
        [SerializeField] private Ease inEase = Ease.OutQuad;
        [SerializeField] private Ease outEase = Ease.OutQuad;

        public override void OnStart()
        {
        }

        public override void TransitionIn(bool immediate = false)
        {
            canvasGroup.blocksRaycasts = true;

            if (immediate)
                canvasGroup.alpha = 1.0f;
            else
                canvasGroup.DOFade(1.0f, transitionDuration).SetEase(inEase);
        }

        public override void TransitionOut(bool immediate = false)
        {
            canvasGroup.blocksRaycasts = blockRaycastsIfInactive;

            if (immediate)
                canvasGroup.alpha = 0.0f;
            else
                canvasGroup.DOFade(0.0f, transitionDuration).SetEase(outEase);
        }
    }
}

