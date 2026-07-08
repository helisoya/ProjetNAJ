using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ANF.Utils
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectAutoScroll : MonoBehaviour
    {
        [Tooltip("The UI/Navigate action used by controller and keyboard to use menus")]
        [SerializeField] InputActionReference navigateAction;
        /// <summary>
        /// The scrollRect attached to this component
        /// </summary>
        ScrollRect _scrollRect;
        /// <summary>
        /// All selectable items inside the scrollrect
        /// </summary>
        Selectable[] _selectables;

        public void Start()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _selectables = GetComponentsInChildren<Selectable>();
            ScrollToSelection();
        }

        void ScrollToSelection()
        {
            GameObject selection = EventSystem.current.currentSelectedGameObject;
            if (!selection) return;
            if (!selection.transform.IsChildOf(transform)) return;
            if (!selection.TryGetComponent(out RectTransform rectTransform)) return;
            ScrollToCenter(_scrollRect, rectTransform);
            int index = Array.IndexOf(_selectables, rectTransform);
            if (index < 0) return;
            float target = 1f - (float)index / Mathf.Max(_selectables.Length - 2, 1);
            _scrollRect.verticalNormalizedPosition = target;
        }

        public void Update()
        {
            if (navigateAction.action.inProgress)
                ScrollToSelection();
        }

        // Shared array used to receive result of RectTransform.GetWorldCorners
        static readonly Vector3[] Corners = new Vector3[4];

        /// <summary>
        /// Transform the bounds of the current rect transform to the space of another transform.
        /// </summary>
        /// <param name="source">The rect to transform</param>
        /// <param name="target">The target space to transform to</param>
        /// <returns>The transformed bounds</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public static Bounds TransformBoundsTo(RectTransform source, Transform target)
        {
            // Based on code in ScrollRect internal GetBounds and InternalGetBounds methods
            Bounds bounds = new Bounds();
            if (source != null)
            {
                source.GetWorldCorners(Corners);

                Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                Matrix4x4 matrix = target.worldToLocalMatrix;
                for (int j = 0; j < 4; j++)
                {
                    Vector3 v = matrix.MultiplyPoint3x4(Corners[j]);
                    vMin = Vector3.Min(v, vMin);
                    vMax = Vector3.Max(v, vMax);
                }

                bounds = new Bounds(vMin, Vector3.zero);
                bounds.Encapsulate(vMax);
            }
            return bounds;
        }

        /// <summary>
        /// Normalize a distance to be used in verticalNormalizedPosition or horizontalNormalizedPosition.
        /// </summary>
        /// <param name="scrollRect">The target scrollRect</param>
        /// <param name="axis">Scroll axis, 0 = horizontal, 1 = vertical</param>
        /// <param name="distance">The distance in the scroll rect view coordinate space</param>
        /// <returns>The normalized scroll distance</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public static float NormalizeScrollDistance(ScrollRect scrollRect, int axis, float distance)
        {
            // Based on code in ScrollRect internal SetNormalizedPosition method
            RectTransform viewport = scrollRect.viewport;
            RectTransform viewRect = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();
            Rect rect = viewRect.rect;
            Bounds viewBounds = new Bounds(rect.center, rect.size);

            RectTransform content = scrollRect.content;
            Bounds contentBounds = content != null ? TransformBoundsTo(content, viewRect) : new Bounds();

            float hiddenLength = contentBounds.size[axis] - viewBounds.size[axis];
            return distance / hiddenLength;
        }

        /// <summary>
        /// Scroll the target element to the vertical center of the scroll rect viewport.
        /// Assumes the target element is part of the scroll rect contents.
        /// </summary>
        /// <param name="scrollRect">Scroll rect to scroll</param>
        /// <param name="target">Element of the scroll rect content to center vertically</param>
        public static void ScrollToCenter(ScrollRect scrollRect, RectTransform target)
        {
            // The scroll rect view space is used to calculate scroll position
            RectTransform view = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();

            // Calculate the scroll offset in the view's space
            Rect viewRect = view.rect;
            Bounds elementBounds = TransformBoundsTo(target, view);
            float offset = viewRect.center.y - elementBounds.center.y;

            // Normalize and apply the calculated offset
            float scrollPos = scrollRect.verticalNormalizedPosition - NormalizeScrollDistance(scrollRect, 1, offset);
            scrollRect.verticalNormalizedPosition = Mathf.Clamp(scrollPos, 0f, 1f);
        }
    }
}