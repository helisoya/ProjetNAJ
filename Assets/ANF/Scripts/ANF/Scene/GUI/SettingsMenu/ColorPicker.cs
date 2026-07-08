using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents a HSV color picker
    /// </summary>
    public class ColorPicker : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private RectTransform root;
        [SerializeField] private Selectable defaultSelectedObject;
        [SerializeField] private Image previewImage;
        [SerializeField] private Slider hSlider;
        [SerializeField] private Slider sSlider;
        [SerializeField] private Slider vSlider;
        [SerializeField] private RectTransform confirmButton;
        [SerializeField] private RectTransform cancelButton;

        private Action<Color> endCallback;
        private Selectable lastSelectable;
        public bool IsOpen { private set; get; } = false;

        void Awake()
        {
            root.localScale = Vector3.zero;
        }

        /// <summary>
        /// Opens the color picker
        /// </summary>
        /// <param name="startColor">The start color</param>
        /// <param name="initiator">The selectable that initiated the color picking</param>
        /// <param name="callback">The end callback</param>
        public void Open(Color startColor, Selectable initiator, Action<Color> callback)
        {
            IsOpen = true;
            lastSelectable = initiator;
            endCallback = callback;

            confirmButton.localScale = Vector3.one;
            cancelButton.localScale = Vector3.one;

            EventSystem.current.SetSelectedGameObject(defaultSelectedObject.gameObject);

            float h, s, v;
            Color.RGBToHSV(startColor, out h, out s, out v);

            hSlider.SetValueWithoutNotify(h);
            sSlider.SetValueWithoutNotify(s);
            vSlider.SetValueWithoutNotify(v);
            RefreshPreview();

            root.DOScale(1.0f, 0.5f).SetEase(Ease.OutBounce);
        }


        /// <summary>
        /// Event for when using a slider
        /// </summary>
        public void OnSlider()
        {
            RefreshPreview();
        }

        /// <summary>
        /// Refreshs the preview
        /// </summary>
        public void RefreshPreview()
        {
            previewImage.color = Color.HSVToRGB(hSlider.value, sSlider.value, vSlider.value);
        }

        /// <summary>
        /// Applies the color
        /// </summary>
        public void Apply()
        {
            Color endColor = Color.HSVToRGB(hSlider.value, sSlider.value, vSlider.value);
            endColor.a = 1.0f;
            endCallback.Invoke(endColor);
            Close();
        }

        /// <summary>
        /// Cancels the action
        /// </summary>
        public void Cancel()
        {
            Close();
        }

        /// <summary>
        /// Closes the picker
        /// </summary>
        /// <param name="selectLastSelectable">True if the last selectable should be selected again (In this case, the color picker button)</param>
        public void Close(bool selectLastSelectable = true)
        {
            IsOpen = false;
            root.DOScale(0.0f, 0.5f).SetEase(Ease.OutBack);
            EventSystem.current.SetSelectedGameObject(lastSelectable.gameObject);
        }
    }

}
