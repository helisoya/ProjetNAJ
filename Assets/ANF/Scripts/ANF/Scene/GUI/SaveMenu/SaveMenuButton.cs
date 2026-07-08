using ANF.Persistent;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
	/// Represents a slot button in the save menu
	/// </summary>
    public class SaveMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Locals.LocalizedText iconText;
        [SerializeField] private Locals.LocalizedText labelText;
        [SerializeField] private Image backgroundImage;

        private SaveMenuUI saveMenuUI;
        private SaveMenuButtonData data;
        private int id;
        private Persistent.AudioManager audioManager;

        /// <summary>
        /// Initialize the button
        /// </summary>
        /// <param name="id">The button's id</param>
        /// <param name="saveMenu">The save menu</param>
        /// <param name="data">The button's data</param>
        public void Initialize(int id, SaveMenuUI saveMenu, SaveMenuButtonData data)
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);

            this.id = id;
            this.data = data;
            this.saveMenuUI = saveMenu;

            if (data != null)
            {
                iconText.GetText().text = data.saveFileIcon;
                labelText.GetText().text = data.label;
                backgroundImage.sprite = data.bgSprite;
            }
        }


        /// <summary>
        /// Updates the button's infos
        /// </summary>
        /// <param name="label">The new label</param>
        /// <param name="bgSprite">The new Background sprite</param>
        public void UpdateInfos(string label, Sprite bgSprite)
        {
            data.label = label;
            data.bgSprite = bgSprite;

            labelText.GetText().text = label;
            backgroundImage.sprite = bgSprite;
        }

        public void OnEnter()
        {
            backgroundImage.DOColor(Color.lightGray, 0.5f).SetEase(Ease.OutQuad);
            root.DOScale(Vector2.one * 0.9f, 0.5f).SetEase(Ease.OutQuad);
            root.DOShakeRotation(0.5f, new Vector3(0, 0, 5f)).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                root.DORotate(Vector3.zero, 0.1f).SetEase(Ease.OutQuad);
            });
        }

        public void OnExit()
        {
            backgroundImage.DOColor(Color.white, 0.5f).SetEase(Ease.OutQuad);
            root.DOScale(Vector2.one, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnClick()
        {
            if (data.interactable)
            {
                if (audioManager != null)
                    audioManager.PlayUICursorConfirmSFX();
                saveMenuUI.OpenConfirmPopup(data);
            }
            else
            {
                if (audioManager != null)
                    audioManager.PlayUICursorCancelSFX();
                root.DOShakeRotation(0.5f, new Vector3(0, 0, 10f)).SetEase(Ease.InOutElastic).OnComplete(() =>
                {
                    root.DORotate(Vector3.zero, 0.1f).SetEase(Ease.OutQuad);
                });

                backgroundImage.DOColor(Color.red, 0.25f).SetEase(Ease.Flash).onComplete += () =>
                {
                    backgroundImage.DOColor(Color.lightGray, 0.25f).SetEase(Ease.Flash);
                };
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                OnClick();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            saveMenuUI.SetCurrentButton(id);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //OnExit();
        }
    }

    /// <summary>
	/// Represents a save menu button's data
	/// </summary>
    public class SaveMenuButtonData
    {
        public bool interactable;
        public bool saveFileExists;
        public string saveFileName;
        public string saveFileIcon;
        public string label;
        public Sprite bgSprite;
    }
}
