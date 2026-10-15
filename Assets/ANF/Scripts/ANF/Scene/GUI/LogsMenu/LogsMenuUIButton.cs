using ANF.Persistent;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ANF.GUI
{
    /// <summary>
	/// Represents a log button in the logs menu
	/// </summary>
    public class LogsMenuUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Locals.LocalizedText label;

        private LogsMenuUI logsMenuUI;
        private int id;
        private string data;
        private bool isUnlocked;

        /// <summary>
        /// Initialize the button
        /// </summary>
        /// <param name="id">The button's id</param>
        /// <param name="logsMenuUI">The log's menu</param>
        /// <param name="data">The log's data (Id)</param>
        /// <param name="isUnlocked">True if the log is unlocked</param>
        public void Initialize(int id, LogsMenuUI logsMenuUI, string data, bool isUnlocked)
        {
            this.id = id;
            this.logsMenuUI = logsMenuUI;
            this.data = data;
            this.isUnlocked = isUnlocked;

            root.localScale = Vector2.one * 0.8f;

            if (isUnlocked)
                label.SetNewKey($"Log_{data}_Name");
            else
                label.SetNewKey("GeneralMenu_Unknown");
        }

        /// <summary>
        /// Gets the button's data
        /// </summary>
        /// <returns>Its data</returns>
        public string GetData()
        {
            return data;
        }

        /// <summary>
        /// Gets if the button's log is unlocked
        /// </summary>
        /// <returns>True if unlocked</returns>
        public bool IsUnlocked()
        {
            return isUnlocked;
        }

        public void OnEnter()
        {
            if (isUnlocked)
                label.GetText().fontStyle = FontStyles.Underline;

            root.DOScale(Vector2.one * 1.0f, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnExit()
        {
            if (isUnlocked)
                label.GetText().fontStyle = FontStyles.Normal;
            root.DOScale(Vector2.one * 0.8f, 0.5f).SetEase(Ease.OutQuad);
        }

        public void ErrorAnimation()
        {
            Color startColor = label.GetText().color;

            root.DOShakeScale(0.5f, 0.1f).SetEase(Ease.InOutElastic).OnComplete(() =>
                {
                    root.DOScale(1.0f, 0.1f).SetEase(Ease.OutQuad);
                });

            label.GetText().DOColor(Color.red, 0.25f).SetEase(Ease.Flash).onComplete += () =>
            {
                label.GetText().DOColor(startColor, 0.25f).SetEase(Ease.Flash);
            };
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && isUnlocked)
                logsMenuUI.ShowLog(data);
            else if (!isUnlocked)
            {
                if (PersistentDataManager.instance.GetGlobalData().GetComponent(out AudioManager audioManager))
                    audioManager.PlaySFX("CursorError", 1.0f);
                ErrorAnimation();
            }

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            logsMenuUI.SetCurrentButton(id);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //OnExit();
        }
    }
}
