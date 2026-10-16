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
            if (hideWhenOut)
                root.SetActive(true);

            DOTween.Kill(panel, TWEENING_ID);

            if (immediate)
            {
                panel.localScale = Vector3.one;
            }
            else
                panel.DOScale(1.0f, transitionDuration).SetEase(inEase).SetId(TWEENING_ID);
        }

        public override void TransitionOut(bool immediate = false)
        {
            DOTween.Kill(panel, TWEENING_ID);

            if (immediate)
            {
                panel.localScale = Vector3.zero;
                if (hideWhenOut)
                    root.SetActive(false);
            }
            else
                panel.DOScale(0.0f, transitionDuration).SetEase(outEase)
                .SetId(TWEENING_ID)
                .OnComplete(() => { if (hideWhenOut) root.SetActive(false); });
        }
    }
}