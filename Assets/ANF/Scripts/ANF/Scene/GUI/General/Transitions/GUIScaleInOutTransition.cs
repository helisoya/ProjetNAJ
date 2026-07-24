using DG.Tweening;
using UnityEngine;

namespace ANF.GUI
{
    /// <summary>
    /// A scale in/out transition for the UI
    /// </summary>
    [System.Serializable]
    public class GUIScaleInOutTransition : GUIInOutTransition
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private Ease inEase = Ease.OutQuad;
        [SerializeField] private Ease outEase = Ease.OutQuad;

        public override void OnStart()
        {
        }

        public override void TransitionIn(bool immediate = false)
        {
            if (immediate)
                panel.localScale = Vector3.one;
            else
                panel.DOScale(1.0f, transitionDuration).SetEase(inEase);
        }

        public override void TransitionOut(bool immediate = false)
        {
            if (immediate)
                panel.localScale = Vector3.zero;
            else
                panel.DOScale(0.0f, transitionDuration).SetEase(outEase);
        }
    }
}