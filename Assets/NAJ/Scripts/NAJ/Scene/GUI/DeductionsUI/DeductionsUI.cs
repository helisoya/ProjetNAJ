using System.Collections.Generic;
using ANF.ANSL;
using ANF.GUI;
using ANF.Locals;
using ANF.Persistent;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using NAJ.Persistent;
using NUnit.Framework.Interfaces;
using Radishmouse;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NAJ.GUI
{
    /// <summary>
    /// Handles the Deductions menu
    /// </summary>
    public class DeductionsUI : GUIComponent
    {
        /// <summary>
		/// The UI Modes of the Deductions menu
		/// </summary>
        public enum UIMode
        {
            Default,
            InfoPopup,
            LinkPopup,
            EndPopup
        }

        [Header("Popups")]
        [SerializeField] private RectTransform defaultPopup;
        [SerializeField] private LocalizedText defaultPopupLabel;
        [SerializeField] private RectTransform linkPopup;
        [SerializeField] private RectTransform linkPopupLeft;
        [SerializeField] private RectTransform linkPopupRight;
        [SerializeField] private CanvasGroup linkPopupLeftActiveObj;
        [SerializeField] private CanvasGroup linkPopupRightActiveObj;
        [SerializeField] private LocalizedText linkPopupLabelLeft;
        [SerializeField] private LocalizedText linkPopupLabelRight;
        [SerializeField] private RectTransform endPopup;
        [SerializeField] private LocalizedText endPopupLabel;
        [SerializeField] private CanvasGroup endErrorGroup;


        [Header("Nodes / Lines")]
        [SerializeField] private Vector2 gridSize = new Vector2(8.0f, 4.5f);
        [SerializeField] private float baseNodeSize = 25f;
        [SerializeField] private int lineSegments = 7;
        [SerializeField] private float segmentShakeAmount = 3.0f;
        [SerializeField] private UILineRenderer prefabLine;
        [SerializeField] private Material lineMaterial;
        [SerializeField] private DeductionsUINode prefabNode;
        [SerializeField] private RectTransform linesRoot;
        [SerializeField] private RectTransform nodesRoot;
        private DeductionsUINode[] nodes;
        private int currentNodeIdx;
        private Vector2Int currentButtonInputSide;
        private float cooldownToNextButtonIncrement = 0;
        private float cursorMoveCooldown = 0.25f;
        private AudioManager audioManager;
        public UIMode currentMode { get; private set; }
        private bool linkPopupOnLeftSide = false;
        private Dictionary<string, DeductionsUINode.Status> statusCache;
        private int endNodeId;
        private bool canSelectEndNode;


        [Header("Background")]
        [SerializeField] private string[] guiToPauseOnEnable;
        private bool repauseNextFrame = false;

        [Header("Input Reminders")]
        [SerializeField] private string[] inputRemindersToDisable;
        private bool[] cachedPreviousReminders;


        /// <summary>
		/// Changes if an end node can be select
		/// </summary>
		/// <param name="canSelectEndNode">True if an end node can be selected</param>       
        public void SetCanSelectEndNode(bool canSelectEndNode)
        {
            this.canSelectEndNode = canSelectEndNode;
        }

        public override void OnInitialize()
        {
            canSelectEndNode = false;
            statusCache = new Dictionary<string, DeductionsUINode.Status>();
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Deductions").performed += OnDeductionsInput;
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetGlobalData().GetComponent(out audioManager);
        }

        public override void OnUpdate()
        {
            if (repauseNextFrame)
            {
                repauseNextFrame = true;
                manager.GetWorld().SetPausedAll(true);
                gui.SetComponentsPaused(guiToPauseOnEnable, true);
            }

            if (currentButtonInputSide.x != 0 || currentButtonInputSide.y != 0)
            {
                cooldownToNextButtonIncrement -= Time.deltaTime;
                if (cooldownToNextButtonIncrement <= 0)
                {
                    if (currentMode == UIMode.Default)
                        IncrementButtonWithInput();
                    else if (currentMode == UIMode.LinkPopup)
                    {
                        int linkedIdx = FindNodeIndex(nodes[currentNodeIdx].GetData().linkedNode);

                        if (statusCache[nodes[currentNodeIdx].GetData().id] != DeductionsUINode.Status.Hidden &&
                            statusCache[nodes[linkedIdx].GetData().id] != DeductionsUINode.Status.Hidden)
                            SwitchLinkPopup(!linkPopupOnLeftSide);
                    }

                    cooldownToNextButtonIncrement = cursorMoveCooldown;
                }
            }
        }

        private void OnNext(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                if (currentMode == UIMode.Default)
                    SelectCurrentButton();
                else if (currentMode == UIMode.LinkPopup)
                    SelectCurrentSwitch();
                else if (currentMode == UIMode.EndPopup)
                    SelectCurrentEndNode();
            }
        }

        private void OnPresent(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused && context.ReadValueAsButton())
            {
                if (currentMode == UIMode.Default && endNodeId != -1)
                    OpenCurrentEndNode();
            }
        }

        private void OnDeductionsInput(InputAction.CallbackContext context)
        {
            if (context.ReadValueAsButton())
            {
                if (gui.GetComponent(out PauseMenuUI pauseMenu) &&
                    pauseMenu.isEnabled)
                    return;



                TrySetEnabled(!isEnabled);
            }
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            if (isEnabled && !isPaused)
            {
                Vector2 value = context.ReadValue<Vector2>();

                bool noMovement = true;

                if (Mathf.Abs(value.x) >= 0.9f)
                {
                    noMovement = false;
                    if (currentButtonInputSide.x == 0)
                    {
                        cooldownToNextButtonIncrement = cursorMoveCooldown;
                        currentButtonInputSide.x = value.x > 0 ? 1 : -1;

                        if (currentMode == UIMode.Default)
                            IncrementButtonWithInput();
                        else if (currentMode == UIMode.LinkPopup)
                        {
                            int linkedIdx = FindNodeIndex(nodes[currentNodeIdx].GetData().linkedNode);

                            if (statusCache[nodes[currentNodeIdx].GetData().id] != DeductionsUINode.Status.Hidden &&
                                statusCache[nodes[linkedIdx].GetData().id] != DeductionsUINode.Status.Hidden)
                                SwitchLinkPopup(!linkPopupOnLeftSide);
                        }
                    }
                }

                if (Mathf.Abs(value.y) >= 0.9f)
                {
                    noMovement = false;
                    if (currentButtonInputSide.y == 0)
                    {
                        cooldownToNextButtonIncrement = cursorMoveCooldown;
                        currentButtonInputSide.y = value.y > 0 ? 1 : -1;

                        if (currentMode == UIMode.Default)
                            IncrementButtonWithInput();
                        else if (currentMode == UIMode.LinkPopup)
                        {
                            int linkedIdx = FindNodeIndex(nodes[currentNodeIdx].GetData().linkedNode);

                            if (statusCache[nodes[currentNodeIdx].GetData().id] != DeductionsUINode.Status.Hidden &&
                                statusCache[nodes[linkedIdx].GetData().id] != DeductionsUINode.Status.Hidden)
                                SwitchLinkPopup(!linkPopupOnLeftSide);
                        }
                    }
                }


                if (noMovement)
                {
                    cooldownToNextButtonIncrement = 0.0f;
                    currentButtonInputSide.x = 0;
                    currentButtonInputSide.y = 0;
                }
            }
        }


        public override void OnPaused()
        {

        }

        public override void OnUnPaused()
        {
            repauseNextFrame = true;
        }

        /// <summary>
		/// Tries to close/open the menu (or the relevant popup)
		/// </summary>
		/// <param name="enabled">True if the menu should be open</param>
        /// <param name="playSound">True if SFX can be played</param>
        public void TrySetEnabled(bool enabled, bool playSound = true)
        {
            if (currentMode == UIMode.EndPopup)
            {
                if (audioManager != null && playSound)
                    audioManager.PlayUICursorCancelSFX();
                currentMode = UIMode.Default;
                endPopup.DOScale(0, 0.5f).SetEase(Ease.OutQuad);
                if (gui.GetComponent(out InputReminderUI inputReminder))
                    inputReminder.SetReminderEnabled("deductionsConfirm", endNodeId != -1 && canSelectEndNode);
            }
            else if (currentMode == UIMode.InfoPopup)
            {
                if (audioManager != null && playSound)
                    audioManager.PlayUICursorCancelSFX();
                currentMode = UIMode.Default;
                defaultPopup.DOScale(0, 0.5f).SetEase(Ease.OutQuad);
                if (gui.GetComponent(out InputReminderUI inputReminder))
                    inputReminder.SetReminderEnabled("deductionsConfirm", endNodeId != -1 && canSelectEndNode);
            }
            else if (currentMode == UIMode.LinkPopup)
            {
                if (audioManager != null && playSound)
                    audioManager.PlayUICursorCancelSFX();
                currentMode = UIMode.Default;
                linkPopup.DOScale(0, 0.5f).SetEase(Ease.OutQuad);
            }
            else
            {
                if (audioManager != null && isEnabled && playSound)
                    audioManager.PlaySFX("CursorUnPause", 1.0f);
                if (audioManager != null && !isEnabled && playSound)
                    audioManager.PlaySFX("CursorPause", 1.0f);

                SetEnabled(enabled);
            }
        }

        public override void OnDisabled()
        {
            manager.GetWorld().SetPausedAll(false);
            gui.SetComponentsPaused(guiToPauseOnEnable, false);

            if (cachedPreviousReminders != null && gui.GetComponent(out InputReminderUI inputReminder))
            {
                inputReminder.SetRemindersState(cachedPreviousReminders);
            }
        }

        public override void OnEnabled()
        {
            if (PersistentDataManager.instance.GetPlayerData().GetComponent(out DeductionsContainer container))
            {
                currentNodeIdx = 0;
                currentMode = UIMode.Default;
                defaultPopup.localScale = Vector3.zero;
                linkPopup.localScale = Vector3.zero;
                endPopup.localScale = Vector3.zero;

                manager.GetWorld().SetPausedAll(true);
                gui.SetComponentsPaused(guiToPauseOnEnable, true);

                foreach (Transform child in linesRoot)
                    Destroy(child.gameObject);
                foreach (Transform child in nodesRoot)
                    Destroy(child.gameObject);

                Dictionary<string, DeductionNode> existingNodes = container.GetNodes();
                nodes = new DeductionsUINode[existingNodes.Count];
                List<string> idsDone = new List<string>();

                int idx = 0;
                foreach (DeductionNode deductionNode in existingNodes.Values)
                {
                    DeductionsUINode uiNode = Instantiate(prefabNode, nodesRoot);
                    UILineRenderer[] images = new UILineRenderer[deductionNode.prerequistes != null ? deductionNode.prerequistes.Length : 0];
                    for (int i = 0; i < images.Length; i++)
                        images[i] = GenerateLineBetweenNodes(existingNodes[deductionNode.prerequistes[i]], deductionNode);

                    uiNode.Init(deductionNode, DeductionsUINode.Status.Shown, baseNodeSize,
                        images, images != null && images.Length > 0 ? new Material(lineMaterial) : null,
                        this, idx);

                    RectTransform nodeTransform = uiNode.GetComponent<RectTransform>();
                    nodeTransform.anchoredPosition = GetCanvasPosition(deductionNode.nodePosition);
                    if (!string.IsNullOrEmpty(deductionNode.linkedNode))
                    {
                        float amount = (baseNodeSize * deductionNode.uiSizeFactor) / 4.0f;
                        if (idsDone.Contains(deductionNode.linkedNode))
                        {
                            nodeTransform.anchoredPosition += new Vector2(-amount, amount);
                        }

                        else
                        {
                            nodeTransform.anchoredPosition += new Vector2(amount, -amount);
                        }

                    }
                    nodes[idx] = uiNode;
                    idx++;
                    idsDone.Add(deductionNode.id);
                }

                if (gui.GetComponent(out InputReminderUI inputReminder))
                {
                    cachedPreviousReminders = inputReminder.GetRemindersState();
                    foreach (string reminder in inputRemindersToDisable)
                    {
                        inputReminder.SetReminderEnabled(reminder, false);
                    }

                    inputReminder.SetReminderEnabled("deductionsBack", true);
                }
                else
                {
                    cachedPreviousReminders = null;
                }

                RecheckNodeVisibility(true);
                SetCurrentButton(0, true);
            }
            else
            {
                SetEnabled(false);
            }
        }

        /// <summary>
		/// Finds the node's index
		/// </summary>
		/// <param name="id">Its id</param>
		/// <returns>Its index</returns>
        private int FindNodeIndex(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                foreach (DeductionsUINode node in nodes)
                {
                    if (node.GetData().id.Equals(id))
                    {
                        return node.id;
                    }
                }
            }
            return -1;
        }

        public void SetCurrentButton(int id, bool force = false)
        {
            string linkedNode = nodes[id].GetData().linkedNode;
            int linkedIdx = -1;

            if (!string.IsNullOrEmpty(linkedNode))
            {
                linkedIdx = FindNodeIndex(linkedNode);
            }


            bool good = force;
            if (!good)
            {
                if (linkedIdx == -1)
                    good = id != currentNodeIdx;
                else
                    good = currentNodeIdx != nodes[id].id && currentNodeIdx != nodes[linkedIdx].id;
            }

            if (good)
            {
                string linkedNodePrevious = nodes[currentNodeIdx].GetData().linkedNode;
                int linkedIdxPrevious = -1;

                if (!string.IsNullOrEmpty(linkedNodePrevious))
                {
                    linkedIdxPrevious = FindNodeIndex(linkedNodePrevious);
                }


                nodes[currentNodeIdx].UnHighlight();
                if (linkedIdxPrevious != -1)
                    nodes[linkedIdxPrevious].UnHighlight();
                currentNodeIdx = id;
                nodes[currentNodeIdx].Highlight();
                if (linkedIdx != -1)
                    nodes[linkedIdx].Highlight();
            }
        }

        /// <summary>
		/// Increments the button with the inputs
		/// </summary>
        private void IncrementButtonWithInput()
        {
            int minScore = 9999;
            int minIdx = -1;

            Vector2Int currentPosition = nodes[currentNodeIdx].GetData().nodePosition;
            string currentLink = nodes[currentNodeIdx].GetData().linkedNode;

            foreach (DeductionsUINode uiNode in nodes)
            {
                if (uiNode.id == currentNodeIdx || statusCache[uiNode.GetData().id] != DeductionsUINode.Status.Shown ||
                    (!string.IsNullOrEmpty(currentLink) && uiNode.id.Equals(currentLink)))
                    continue; // Skip self, linked node & non "shown" nodes

                Vector2Int nodePos = uiNode.GetData().nodePosition;
                Vector2Int dist = nodePos - currentPosition;
                int score = Mathf.Abs(dist.x) + Mathf.Abs(dist.y);

                if (
                   (currentButtonInputSide.x > 0 && dist.x > 0 && score < minScore) ||
                   (currentButtonInputSide.x < 0 && dist.x < 0 && score < minScore) ||
                   (currentButtonInputSide.y > 0 && dist.y > 0 && score < minScore) ||
                   (currentButtonInputSide.y < 0 && dist.y < 0 && score < minScore))
                {
                    minScore = score;
                    minIdx = uiNode.id;
                }
            }

            if (minIdx != -1)
                SetCurrentButton(minIdx);
        }

        /// <summary>
		/// Selects the current button
		/// </summary>
        public void SelectCurrentButton()
        {
            if (currentMode != UIMode.Default)
                return;

            if (audioManager != null)
                audioManager.PlayUICursorConfirmSFX();

            if (gui.GetComponent(out InputReminderUI inputReminder))
                inputReminder.SetReminderEnabled("deductionsConfirm", false);

            if (nodes[currentNodeIdx].GetData().isEndNode)
            {
                // End Popup
                currentMode = UIMode.EndPopup;
                endPopupLabel.SetNewKey(nodes[currentNodeIdx].GetData().GetDesc());
                endPopup.DOScale(1.0f, 0.5f).SetEase(Ease.OutQuad);
            }
            else if (!string.IsNullOrEmpty(nodes[currentNodeIdx].GetData().linkedNode))
            {
                // Link Popup
                currentMode = UIMode.LinkPopup;
                linkPopup.DOScale(1.0f, 0.5f).SetEase(Ease.OutQuad);

                int linkedIdx = FindNodeIndex(nodes[currentNodeIdx].GetData().linkedNode);
                bool leftVisible = statusCache[nodes[Mathf.Max(linkedIdx, currentNodeIdx)].GetData().id] != DeductionsUINode.Status.Hidden;
                bool rightVisible = statusCache[nodes[Mathf.Min(linkedIdx, currentNodeIdx)].GetData().id] != DeductionsUINode.Status.Hidden;
                linkPopupLeft.localScale = leftVisible ? Vector2.one : Vector2.zero;
                linkPopupRight.localScale = rightVisible ? Vector2.zero : Vector2.one;

                if (leftVisible)
                    linkPopupLabelLeft.SetNewKey(nodes[Mathf.Max(linkedIdx, currentNodeIdx)].GetData().GetDesc());
                if (rightVisible)
                    linkPopupLabelRight.SetNewKey(nodes[Mathf.Min(linkedIdx, currentNodeIdx)].GetData().GetDesc());

                SwitchLinkPopup(
                    nodes[Mathf.Max(linkedIdx, currentNodeIdx)].GetData().isActiveLink
                );
            }
            else
            {
                // Normal Popup
                currentMode = UIMode.InfoPopup;
                defaultPopupLabel.SetNewKey(nodes[currentNodeIdx].GetData().GetDesc());
                defaultPopup.DOScale(1.0f, 0.5f).SetEase(Ease.OutQuad);
            }
        }

        /// <summary>
		/// Selects the current end node
		/// </summary>
        public void SelectCurrentEndNode()
        {
            if (!canSelectEndNode)
            {
                // Can't do it now
                audioManager.PlaySFX("CursorError", 1.0f);
                endErrorGroup.DOKill(false);
                endErrorGroup.DOFade(1.0f, 0.25f).SetEase(Ease.OutBounce).OnComplete(() =>
                {
                    endErrorGroup.DOFade(0.0f, 0.25f).SetEase(Ease.InBounce).SetDelay(1.0f);
                });
            }
            else
            {
                string selectedEndScript = nodes[currentNodeIdx].GetData().endScript;
                TrySetEnabled(false, false);
                TrySetEnabled(false, false);

                if (manager.GetWorld().GetComponent(out ANSLManager anslManager))
                {
                    ANSLContext context = anslManager.GetContext(0);
                    selectedEndScript = ANSLUtils.ResolveFilePath(context.GetCurrentFilepath(), selectedEndScript);
                    anslManager.GetContext(0).LoadScript(selectedEndScript);
                }
            }
        }

        /// <summary>
		/// Select the current switch link
		/// </summary>
        public void SelectCurrentSwitch()
        {
            if (audioManager != null)
                audioManager.PlayUICursorConfirmSFX();
            TrySetEnabled(false, false);
            int linkedIdx = FindNodeIndex(nodes[currentNodeIdx].GetData().linkedNode);
            int leftIdx = Mathf.Max(linkedIdx, currentNodeIdx);
            int rightIdx = Mathf.Min(linkedIdx, currentNodeIdx);

            if (linkPopupOnLeftSide && !nodes[leftIdx].GetData().isActiveLink)
            {
                nodes[leftIdx].GetData().SetIsActiveLink(true);
                nodes[rightIdx].GetData().SetIsActiveLink(false);
                RecheckNodeVisibility(false);
            }
            else if (!linkPopupOnLeftSide && !nodes[rightIdx].GetData().isActiveLink)
            {
                nodes[leftIdx].GetData().SetIsActiveLink(false);
                nodes[rightIdx].GetData().SetIsActiveLink(true);
                RecheckNodeVisibility(false);
            }
        }

        /// <summary>
        /// Opens the current end node if possible
        /// </summary>
        public void OpenCurrentEndNode()
        {
            if (endNodeId != -1 && currentMode == UIMode.Default)
            {
                SetCurrentButton(endNodeId);
                SelectCurrentButton();
            }
        }


        /// <summary>
		/// Switch the current side for the link popup
		/// </summary>
        public void SwitchLinkPopup(bool onLeftSide)
        {
            linkPopupOnLeftSide = onLeftSide;

            if (onLeftSide)
            {
                linkPopupLeft.SetAsLastSibling();
                linkPopupLeft.DOScale(1.1f, 0.5f).SetEase(Ease.OutQuad);
                linkPopupRight.DOScale(1.0f, 0.5f).SetEase(Ease.OutQuad);
                linkPopupLeftActiveObj.DOFade(1.0f, 0.25f).SetEase(Ease.OutQuad);
                linkPopupRightActiveObj.DOFade(0.0f, 0.25f).SetEase(Ease.OutQuad);
                linkPopupLeftActiveObj.blocksRaycasts = true;
                linkPopupRightActiveObj.blocksRaycasts = false;
            }
            else
            {
                linkPopupLeft.SetAsFirstSibling();
                linkPopupLeft.DOScale(1.0f, 0.5f).SetEase(Ease.OutQuad);
                linkPopupRight.DOScale(1.1f, 0.5f).SetEase(Ease.OutQuad);
                linkPopupLeftActiveObj.DOFade(0.0f, 0.25f).SetEase(Ease.OutQuad);
                linkPopupRightActiveObj.DOFade(1.0f, 0.25f).SetEase(Ease.OutQuad);
                linkPopupLeftActiveObj.blocksRaycasts = false;
                linkPopupRightActiveObj.blocksRaycasts = true;
            }
        }

        /// <summary>
		/// Generates a line between two nodes
		/// </summary>
		/// <param name="node1">The first node</param>
		/// <param name="node2">The second node</param>
		/// <returns>The line</returns>
        private UILineRenderer GenerateLineBetweenNodes(DeductionNode node1, DeductionNode node2)
        {
            Vector2 pos1 = GetCanvasPosition(node1.nodePosition);
            Vector2 pos2 = GetCanvasPosition(node2.nodePosition);
            Vector2 directionVector = pos2 - pos1;

            UILineRenderer instance = Instantiate(prefabLine, linesRoot);

            Vector2[] points = new Vector2[lineSegments + 2];
            points[0] = pos1;
            points[lineSegments + 1] = pos2;

            for (int i = 0; i < lineSegments; i++)
            {
                Vector2 point = pos1 + directionVector * ((i + 1.0f) / (lineSegments + 1.0f));
                point += Random.insideUnitCircle * segmentShakeAmount;
                points[i + 1] = point;
            }

            instance.SetPoints(points);
            return instance;
        }

        /// <summary>
		/// Gets the canvas position
		/// </summary>
		/// <param name="nodePosition">The node's "grid" position</param>
		/// <returns>The node's canvas position</returns>
        private Vector2 GetCanvasPosition(Vector2Int nodePosition)
        {
            Vector2 cellSize = new Vector2(800.0f / gridSize.x, 600.0f / gridSize.y);
            return new Vector2(
                (nodePosition.x * cellSize.x + (cellSize.x / 2.0f)),
                (nodePosition.y * cellSize.y + (cellSize.y / 2.0f))
            );
        }

        /// <summary>
		/// Recheck nodes visibility
		/// </summary>
		/// <param name="immediate">True if activation should be immediate</param>
        private void RecheckNodeVisibility(bool immediate)
        {
            statusCache.Clear();
            endNodeId = -1;

            foreach (DeductionsUINode uINode in nodes)
            {
                RecheckNodeVisibilityRecursive(immediate, uINode);
            }

            if (gui.GetComponent(out InputReminderUI inputReminder))
                inputReminder.SetReminderEnabled("deductionsConfirm", endNodeId != -1 && canSelectEndNode);
        }

        /// <summary>
		/// Recheck nodes visibility (recursive inner function)
		/// </summary>
		/// <param name="immediate">True if activation should be immediate</param>
        /// <param name="uINode">The current UINode</param>
        /// <returns>True if this node should be visible</returns>
        private DeductionsUINode.Status RecheckNodeVisibilityRecursive(bool immediate, DeductionsUINode uINode)
        {
            DeductionNode node = uINode.GetData();
            if (statusCache.TryGetValue(node.id, out DeductionsUINode.Status result))
                return result;

            bool computed = node.IsActiveAndVisible();
            bool prerequisteWrong = false;
            if (node.prerequistes != null)
            {
                foreach (string prerequiste in node.prerequistes)
                {
                    foreach (DeductionsUINode prerequisteNode in nodes)
                    {
                        if (prerequisteNode.GetData().id.Equals(prerequiste))
                        {
                            if (RecheckNodeVisibilityRecursive(immediate, prerequisteNode) != DeductionsUINode.Status.Shown)
                            {
                                prerequisteWrong = true;
                                break;
                            }
                        }
                    }
                }
            }

            DeductionsUINode.Status status;
            if (computed && !prerequisteWrong)
                status = DeductionsUINode.Status.Shown;
            else if (!prerequisteWrong && node.isVisible && !node.isActiveLink)
                status = DeductionsUINode.Status.NotActiveLink;
            else
                status = DeductionsUINode.Status.Hidden;

            if (status != uINode.status)
            {
                uINode.SetStatus(status, immediate);
            }

            if (status == DeductionsUINode.Status.Shown && node.isEndNode)
                endNodeId = uINode.id;

            statusCache.Add(node.id, status);
            return status;
        }

        public override void OnRegisterInputs()
        {
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Next").performed += OnNext;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Back").performed += OnDeductionsInput;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").performed += OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").canceled += OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Present").performed += OnPresent;
        }

        public override void OnUnRegisterInputs()
        {
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Next").performed += OnNext;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Back").performed -= OnDeductionsInput;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").performed -= OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").canceled -= OnMove;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Present").performed -= OnPresent;
        }



        public override void OnSave(JSON json)
        {
            if (cachedPreviousReminders != null && cachedPreviousReminders.Length != 0)
            {
                json.Add("cachedPreviousReminders", new JArray(cachedPreviousReminders));
            }
        }

        public override bool OnLoad(JSON json)
        {
            if (json.ContainsKey("cachedPreviousReminders"))
            {
                JArray array = json.GetJArray("cachedPreviousReminders");
                cachedPreviousReminders = array.AsBoolArray();
            }

            return true;
        }


        public override bool IsLoadingOrCleaningUp(bool updateComponent)
        {
            return false;
        }

        public override bool OnChangeScene()
        {
            audioManager = null;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Deductions").performed -= OnDeductionsInput;
            foreach (DeductionsUINode uINode in nodes)
            {
                uINode.Cleanup();
            }
            return true;
        }
    }
}

