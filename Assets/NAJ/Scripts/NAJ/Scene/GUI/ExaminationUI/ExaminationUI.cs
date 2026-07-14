using System.Collections.Generic;
using ANF.GUI;
using ANF.Locals;
using ANF.Persistent;
using ANF.Scene;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using NAJ.ANSL;
using NAJ.Persistent;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace NAJ.GUI
{
    /// <summary>
    /// Represents the examination UI (The arrows & the little icons at the bottom of the screen)
    /// </summary>
    public class ExaminationUI : GUIComponent
    {
        [Header("Items")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private ExaminationUIArrow leftArrow;
        [SerializeField] private ExaminationUIArrow rightArrow;
        [SerializeField] private RectTransform iconsRoot;
        [SerializeField] private ExaminationUIPartIcon prefabIcon;
        [SerializeField] private float transitionDuration = 0.5f;
        private List<string> pressedIds = new List<string>();
        private string[] currentIds;
        private uint[] currentIdsLines;
        private ExaminationUIPartIcon[] partIcons;
        private string currentId;
        private int currentIdIndex;
        private bool canBeInteractedWith = false;

        public UnityEvent onPressPart;
        public UnityEvent onGoLeft;
        public UnityEvent onGoRight;
        public UnityEvent<uint> onChangePart;

        private AudioManager audioManager;


        public override void OnInitialize()
        {
            onPressPart = new UnityEvent();
            onGoLeft = new UnityEvent();
            onGoRight = new UnityEvent();
            onChangePart = new UnityEvent<uint>();

            canBeInteractedWith = false;
            canvasGroup.alpha = 0.0f;
            canvasGroup.blocksRaycasts = false;
            pressedIds = new List<string>();

            ClearData();
        }

        /// <summary>
        /// Changes if the menu is interactable or not
        /// </summary>
        /// <param name="canBeInteractedWith">True if interactable</param>
        public void SetCanBeInteractedWith(bool canBeInteractedWith)
        {
            this.canBeInteractedWith = canBeInteractedWith;
            leftArrow.SetCanBeInteractedWith(canBeInteractedWith);
            rightArrow.SetCanBeInteractedWith(canBeInteractedWith);
            if (partIcons != null)
            {
                foreach (ExaminationUIPartIcon icon in partIcons)
                {
                    icon.SetCanBeInteractedWith(canBeInteractedWith);
                }
            }
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);
        }

        public override void OnUpdate()
        {

        }

        /// <summary>
		/// Clears the examination data
		/// </summary>
        public void ClearData()
        {
            foreach (Transform child in iconsRoot)
                Destroy(child.gameObject);

            partIcons = null;
            currentId = null;
            currentIdIndex = 0;
            currentIds = null;
            currentIdsLines = null;
            pressedIds.Clear();
        }

        /// <summary>
		/// Try settings the current visual parts for this examination
		/// </summary>
		/// <param name="ids">The list of ids</param>
		/// <param name="idLines">The list of lines linked to the ids</param>
        public void SetIds(string[] ids, uint[] idLines)
        {
            bool shouldChange = currentIds == null || ids.Length != currentIds.Length;

            if (!shouldChange)
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    if (!ids[i].Equals(currentIds[i]))
                    {
                        shouldChange = true;
                        break;
                    }
                }
            }

            if (shouldChange)
            {
                currentIds = ids;
                currentIdsLines = idLines;

                foreach (Transform child in iconsRoot)
                    Destroy(child.gameObject);

                partIcons = new ExaminationUIPartIcon[currentIds.Length];
                for (int i = 0; i < currentIds.Length; i++)
                {
                    partIcons[i] = Instantiate(prefabIcon, iconsRoot);
                    partIcons[i].Initialize(i, this);
                    partIcons[i].SetCanBeInteractedWith(canBeInteractedWith);
                    partIcons[i].SetIsActiveTab(currentIds[i].Equals(currentId));
                }
            }
        }

        /// <summary>
		/// Changes the current id visualy
		/// </summary>
		/// <param name="id">The new id</param>
        public void SetId(string id)
        {
            if (currentIds == null)
                return;

            for (int i = 0; i < currentIds.Length; i++)
            {
                if (currentIds[i].Equals(id))
                {
                    SetId(i);
                    return;
                }
            }

            // You could still bind an id that doesn't exist (yet), but the active line will not be changed
            currentId = id;
        }

        /// <summary>
		/// Changes the current id visualy
		/// </summary>
		/// <param name="idIndex">The new id's index</param>
        public void SetId(int idIndex)
        {
            partIcons[currentIdIndex].SetIsActiveTab(false);

            currentIdIndex = idIndex;
            currentId = currentIds[idIndex];
            partIcons[idIndex].SetIsActiveTab(true);
            leftArrow.gameObject.SetActive(idIndex != 0);
        }

        /// <summary>
		/// Registers a new id as pressed
		/// </summary>
		/// <param name="id">The id</param>
        public void RegisterIdAsPressed(string id)
        {
            if (!pressedIds.Contains(id))
            {
                pressedIds.Add(id);
            }
        }

        /// <summary>
		/// Gets the list of pressed ids
		/// </summary>
		/// <returns>The pressed ids</returns>
        public List<string> GetPressedIds()
        {
            return pressedIds;
        }

        /// <summary>
		/// Gets the current part's id
		/// </summary>
		/// <returns>The current Id</returns>
        public string GetCurrentId()
        {
            return currentId;
        }

        /// <summary>
		/// Try submitting a press action
		/// </summary>
        public void TryPress()
        {
            if (canBeInteractedWith)
            {
                onPressPart.Invoke();
            }
        }

        /// <summary>
		/// Try submitting a change part action
		/// </summary>
        public void TryChangePart(bool goLeft)
        {
            if (canBeInteractedWith)
            {
                if (goLeft)
                    onGoLeft.Invoke();
                else
                    onGoRight.Invoke();
            }
        }

        /// <summary>
        /// Try submitting a change part action
        /// </summary>
        public void TryChangePart(int partIndex)
        {
            if (canBeInteractedWith)
            {
                onChangePart.Invoke(currentIdsLines[partIndex]);
            }
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
            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOFade(0.0f, transitionDuration).SetEase(Ease.OutQuad);
        }

        public override void OnUnPaused()
        {
            canvasGroup.DOFade(1.0f, transitionDuration).SetEase(Ease.OutQuad);
            canvasGroup.blocksRaycasts = true;
        }

        public override void OnRegisterInputs()
        {
        }

        public override void OnUnRegisterInputs()
        {
        }

        public override void OnChangeScene()
        {
        }

        public override void OnSave(JSON json)
        {
            json.Add("pressedIds", pressedIds.ToArray());
            json.Add("canBeInteractedWith", canBeInteractedWith);
            if (currentIdsLines != null)
                json.Add("currentIdsLines", currentIdsLines);
            if (currentIds != null)
                json.Add("currentIds", currentIds);
            if (currentId != null)
                json.Add("currentId", currentId);
            json.Add("currentIdIndex", currentIdIndex);
        }

        public override void OnLoad(JSON json)
        {
            if (json.ContainsKey("pressedIds"))
            {
                pressedIds.Clear();
                pressedIds.AddRange(json.GetJArray("pressedIds").AsStringArray());
            }

            if (json.ContainsKey("currentIds") && json.ContainsKey("currentIdsLines"))
                SetIds(json.GetJArray("currentIds").AsStringArray(), json.GetJArray("currentIdsLines").AsUIntArray());

            if (json.ContainsKey("currentId"))
                SetId(json.GetString("currentId"));

            if (json.ContainsKey("canBeInteractedWith"))
                SetCanBeInteractedWith(json.GetBool("canBeInteractedWith"));
        }
    }
}

