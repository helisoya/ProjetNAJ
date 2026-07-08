using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ANF.GUI
{
    /// <summary>
	/// Represents a quest button in the quests menu
	/// </summary>
    public class QuestsMenuUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Locals.LocalizedText label;

        private QuestsMenuUI questsMenuUI;
        private int id;
        private KeyValuePair<Persistent.QuestInfo, int> data;

        /// <summary>
        /// Initialize the button
        /// </summary>
        /// <param name="id">The button's id</param>
        /// <param name="questsMenuUI">The quests menu</param>
        /// <param name="data">The quest data</param>
        public void Initialize(int id, QuestsMenuUI questsMenuUI, KeyValuePair<Persistent.QuestInfo, int> data)
        {
            this.id = id;
            this.questsMenuUI = questsMenuUI;
            this.data = data;

            root.localScale = Vector2.one * 0.8f;

            label.SetNewKey(data.Key.GetNameKey());
            label.GetText().fontStyle = data.Value >= 100 ? TMPro.FontStyles.Strikethrough : TMPro.FontStyles.Normal;
        }

        /// <summary>
        /// Gets the button's data
        /// </summary>
        /// <returns>Its data</returns>
        public KeyValuePair<Persistent.QuestInfo, int> GetData()
        {
            return data;
        }

        public void OnEnter()
        {
            if (data.Value < 100)
                label.GetText().fontStyle = FontStyles.Underline;

            root.DOScale(Vector2.one * 1.0f, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnExit()
        {
            if (data.Value < 100)
                label.GetText().fontStyle = FontStyles.Normal;
            root.DOScale(Vector2.one * 0.8f, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                questsMenuUI.ShowQuest(data);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            questsMenuUI.SetCurrentButton(id);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //OnExit();
        }
    }
}
