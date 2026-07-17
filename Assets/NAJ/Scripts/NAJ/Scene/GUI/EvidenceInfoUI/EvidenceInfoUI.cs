using ANF.GUI;
using ANF.Locals;
using ANF.Persistent;
using DG.Tweening;
using Leguar.TotalJSON;
using NAJ.Persistent;
using UnityEngine;
using UnityEngine.UI;


namespace NAJ.GUI
{
    /// <summary>
    /// Represents the examination UI (The arrows & the little icons at the bottom of the screen)
    /// </summary>
    public class EvidenceInfoUI : GUIComponent
    {
        [Header("All")]
        [SerializeField] private CanvasGroup canvasGroup;


        [Header("Left")]
        [SerializeField] private RectTransform leftRoot;
        [SerializeField] private Image leftImage;

        [Header("Left")]
        [SerializeField] private RectTransform rightRoot;
        [SerializeField] private Image rightImage;

        [Header("Full")]
        [SerializeField] private RectTransform fullRoot;
        [SerializeField] private Image fullImage;
        [SerializeField] private LocalizedText fullName;
        [SerializeField] private LocalizedText fullDesc;

        public enum Status
        {
            Hidden,
            ShowLeft,
            ShowRight,
            ShowFull
        }

        private Status currentStatus;
        private string currentItem;
        private bool isEvidence;

        public override void OnInitialize()
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 1.0f;
            leftRoot.anchoredPosition = new Vector2(-100, leftRoot.anchoredPosition.y);
            rightRoot.anchoredPosition = new Vector2(100, rightRoot.anchoredPosition.y);
            fullRoot.localScale = new Vector2(0, 0);
        }

        public override void OnStart()
        {

        }

        public override void OnUpdate()
        {

        }

        public override void OnEnabled()
        {
            OnUnPaused();
        }

        public override void OnDisabled()
        {
            OnPaused();
        }

        public override void OnPaused()
        {
            canvasGroup.DOFade(0.0f, 0.5f).SetEase(Ease.OutQuad);
        }

        public override void OnUnPaused()
        {
            canvasGroup.DOFade(1.0f, 0.5f).SetEase(Ease.OutQuad);
        }

        public override void OnRegisterInputs()
        {
        }

        public override void OnUnRegisterInputs()
        {
        }

        public override bool OnChangeScene()
        {
            return true;
        }

        public override bool IsCleaningUpForSceneChange()
        {
            return false;
        }

        /// <summary>
		/// Shows an evidence's info on screen
		/// </summary>
		/// <param name="status">Where to put the infos</param>
		/// <param name="itemName">The item name</param>
		/// <param name="isEvidence">True if the item is evidence. False if it is a profile</param>
        public void SetInfos(Status status, string itemName, bool isEvidence)
        {
            currentItem = itemName;
            this.isEvidence = isEvidence;

            if (currentStatus != status)
            {
                if (currentStatus == Status.ShowLeft)
                    leftRoot.DOAnchorPosX(-100, 0.5f).SetEase(Ease.OutQuad);
                else if (currentStatus == Status.ShowRight)
                    rightRoot.DOAnchorPosX(100, 0.5f).SetEase(Ease.OutQuad);
                else if (currentStatus == Status.ShowFull)
                    fullRoot.DOScale(0.0f, 0.5f).SetEase(Ease.OutQuad);

                currentStatus = status;

                if (currentStatus == Status.ShowLeft)
                    leftRoot.DOAnchorPosX(100, 0.5f).SetEase(Ease.OutQuad);
                else if (currentStatus == Status.ShowRight)
                    rightRoot.DOAnchorPosX(-100, 0.5f).SetEase(Ease.OutQuad);
                else if (currentStatus == Status.ShowFull)
                    fullRoot.DOScale(1.0f, 0.5f).SetEase(Ease.OutQuad);
            }

            if (currentItem != null &&
            PersistentDataManager.instance.GetPlayerData().GetComponent(out NAJCaseInventoryContainer inventoryContainer))
            {
                Sprite sprite = null;
                string nameKey = "";
                string descKey = "";
                if (isEvidence && inventoryContainer.GetAllEvidence().TryGetValue(currentItem, out NAJCaseEvidence evidence))
                {
                    nameKey = evidence.GetNameKey();
                    descKey = evidence.GetDescKey();
                    sprite = evidence.LoadIcon();
                }
                else if (!isEvidence && inventoryContainer.GetAllProfiles().TryGetValue(currentItem, out NAJCaseProfile profile))
                {
                    nameKey = profile.GetNameKey();
                    descKey = profile.GetDescKey();
                    sprite = profile.LoadIcon();
                }

                if (currentStatus == Status.ShowLeft)
                    leftImage.sprite = sprite;
                else if (currentStatus == Status.ShowRight)
                    rightImage.sprite = sprite;
                else if (currentStatus == Status.ShowFull)
                {
                    fullImage.sprite = sprite;
                    fullName.SetNewKey(nameKey);
                    fullDesc.SetNewKey(descKey);
                }
            }
        }

        /// <summary>
		/// Hides the current info
		/// </summary>
        public void HideInfos()
        {
            SetInfos(Status.Hidden, null, false);
        }

        public override void OnSave(JSON json)
        {
            json.Add("currentStatus", (int)currentStatus);
            json.Add("isEvidence", isEvidence);
            if (currentItem != null)
                json.Add("currentItem", currentItem);
        }

        public override void OnLoad(JSON json)
        {

            if (json.ContainsKey("currentStatus") &&
                json.ContainsKey("isEvidence") &&
                json.ContainsKey("currentItem"))
            {
                SetInfos((Status)json.GetInt("currentStatus"), json.GetString("currentItem"), json.GetBool("isEvidence"));
            }
        }
    }
}

