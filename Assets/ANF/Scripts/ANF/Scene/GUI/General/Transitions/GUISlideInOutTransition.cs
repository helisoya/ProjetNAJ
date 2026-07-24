using DG.Tweening;
using UnityEngine;

namespace ANF.GUI
{
    /// <summary>
    /// A transition that consists in sliding a root panel in/out of the screen.
    /// ONLY Works if the anchor is set to the same side as SlideFrom
    /// </summary>
    [System.Serializable]
    public class GUISlideInOutTransition : GUIInOutTransition
    {
        public enum SlideFrom
        {
            Left,
            Right,
            Top,
            Bottom
        }

        [SerializeField] private RectTransform panel;
        [SerializeField] private SlideFrom slideFrom;
        [SerializeField] private Ease inEase = Ease.OutQuad;
        [SerializeField] private Ease outEase = Ease.OutQuad;
        private Vector2 desiredAnchoredPosition;

        public override void OnStart()
        {
            desiredAnchoredPosition = panel.anchoredPosition;
        }

        public override void TransitionIn(bool immediate = false)
        {
            if (immediate)
                panel.anchoredPosition = desiredAnchoredPosition;
            else
                panel.DOAnchorPos(desiredAnchoredPosition, transitionDuration).SetEase(inEase);
        }

        public override void TransitionOut(bool immediate = false)
        {
            Vector2 outPos = desiredAnchoredPosition;
            Vector2 halfSize = panel.sizeDelta / 2.0f;


            if (slideFrom == SlideFrom.Left)
                outPos.x = -halfSize.x;
            else if (slideFrom == SlideFrom.Right)
                outPos.x = halfSize.x;
            else if (slideFrom == SlideFrom.Bottom)
                outPos.y = -halfSize.y;
            else if (slideFrom == SlideFrom.Top)
                outPos.y = halfSize.y;

            if (immediate)
                panel.anchoredPosition = outPos;
            else
                panel.DOAnchorPos(outPos, transitionDuration).SetEase(outEase);
        }
    }
}

