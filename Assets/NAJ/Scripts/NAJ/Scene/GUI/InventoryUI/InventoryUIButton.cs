using ANF.Persistent;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NAJ.GUI
{
    /// <summary>
	/// Represents a slot button in the inventory / case record
	/// </summary>
    public class InventoryUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image icon;
        [SerializeField] private Image selectionOutline;

        private InventoryUI menu;
        private InventoryUIButtonData data;
        private int id;
        private AudioManager audioManager;

        /// <summary>
        /// Destroys the button
        /// </summary>
        public void Destroy()
        {
            audioManager = null;
            icon.sprite = null;
            selectionOutline.sprite = null;
            menu = null;
            data = null;
            Destroy(gameObject);
        }

        /// <summary>
        /// Initialize the button
        /// </summary>
        /// <param name="id">The button's id</param>
        /// <param name="saveMenu">The save menu</param>
        /// <param name="data">The button's data</param>
        public void Initialize(int id, InventoryUI menu, InventoryUIButtonData data)
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);

            this.id = id;
            this.data = data;
            this.menu = menu;

            if (data != null)
            {
                icon.sprite = data.icon;
            }

            if (!data.isValid)
                selectionOutline.gameObject.SetActive(false);

            selectionOutline.pixelsPerUnitMultiplier = 2.0f;
        }

        /// <summary>
		/// Gets the linked data
		/// </summary>
		/// <returns>The linked data</returns>
        public InventoryUIButtonData GetData()
        {
            return data;
        }

        public void OnEnter()
        {
            selectionOutline.DOColor(Color.green, 0.25f).SetEase(Ease.OutQuad);
        }

        public void OnExit()
        {
            selectionOutline.DOColor(Color.white, 0.25f).SetEase(Ease.OutQuad);
        }

        public void OnSelect()
        {
            if (data.isValid)
            {
                menu.ShowDetails(data);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (data != null && data.isValid)
                menu.SetCurrentButton(id);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //OnExit();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (data.isValid && eventData.button == PointerEventData.InputButton.Left)
            {
                menu.TryPresentCurrentEvidence();
            }
        }
    }

    /// <summary>
	/// Represents an inventory button's data
	/// </summary>
    public class InventoryUIButtonData
    {
        public Sprite icon;
        public string id;
        public InventoryUI.InventoryTabType type;
        public bool isValid;
    }
}
