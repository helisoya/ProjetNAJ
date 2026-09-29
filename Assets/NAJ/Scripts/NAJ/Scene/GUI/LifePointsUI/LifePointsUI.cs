using ANF.GUI;
using ANF.Locals;
using ANF.Persistent;
using DG.Tweening;
using Leguar.TotalJSON;
using NAJ.Persistent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NAJ.GUI
{
    /// <summary>
    /// Handles the rendering of the life points on screen
    /// </summary>
    public class LifePointsUI : GUIComponent
    {
        [Header("Life Bars")]
        [SerializeField] private Image lifebar;
        [SerializeField] private Image previewbar;
        [SerializeField] private Color damageColor;
        [SerializeField] private Color regainColor;
        [SerializeField] private string lifePointsVariableName = "LifePoints";
        [SerializeField] private string maxLifePointsVariableName = "MaxLifePoints";

        private int currentLifePoints;
        private int maxLifePoints;
        private int currentPreviewRange;
        private float currentPreviewTarget;
        private float currentLifeTarget;
        private PlayerVariableContainer playerVariableContainer;

        public override void OnInitialize()
        {
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetPlayerData().GetComponent(out playerVariableContainer);
        }

        public override void OnUpdate()
        {

        }

        public override void OnEnabled()
        {
            currentPreviewRange = 0;
            Refresh(true, true);
        }

        /// <summary>
		/// Refreshs the values
		/// </summary>
		/// <param name="immediate">True if the change should be immediate</param>
		/// <param name="refreshValues">True if the value's should be refreshed</param>
        public void Refresh(bool immediate, bool refreshValues)
        {
            if (playerVariableContainer != null && refreshValues)
            {
                playerVariableContainer.GetVariable(lifePointsVariableName, out currentLifePoints);
                playerVariableContainer.GetVariable(maxLifePointsVariableName, out maxLifePoints);
            }

            if (maxLifePoints == 0)
                maxLifePoints = 1;

            currentPreviewTarget = currentLifePoints / (float)maxLifePoints;
            currentLifeTarget = Mathf.Max(0, currentLifePoints - currentPreviewRange) / (float)maxLifePoints;

            bool losingPreviewIsDamage = currentPreviewTarget > currentLifeTarget;
            Color selectedColor = losingPreviewIsDamage ? damageColor : regainColor;

            previewbar.DOKill();
            lifebar.DOKill();

            if (immediate)
            {
                previewbar.fillAmount = currentPreviewTarget;
                previewbar.color = selectedColor;
                lifebar.fillAmount = currentLifeTarget;
            }
            else
            {
                previewbar.DOFillAmount(currentPreviewTarget, losingPreviewIsDamage ? 0.5f : 0.1f).SetEase(Ease.OutQuad);
                previewbar.DOColor(selectedColor, losingPreviewIsDamage ? 0.5f : 0.1f).SetEase(Ease.OutQuad);
                lifebar.DOFillAmount(currentLifeTarget, losingPreviewIsDamage ? 0.1f : 0.75f).SetEase(Ease.OutQuad);
            }
        }

        /// <summary>
		/// Sets the preview range for the health bar 
		/// </summary>
		/// <param name="previewRange">The range</param>
		/// <param name="immediate">True if the change should be immediate</param>
        /// <param name="refresh">True if a refresh should be immediately triggered</param>
        public void SetPreviewRange(int previewRange, bool immediate, bool refresh)
        {
            currentPreviewRange = previewRange;
            if (refresh)
                Refresh(immediate, false);
        }

        public override void OnDisabled()
        {
        }

        public override void OnPaused()
        {

        }

        public override void OnUnPaused()
        {

        }

        public override void OnRegisterInputs()
        {
        }

        public override void OnUnRegisterInputs()
        {
        }

        public override bool OnChangeScene()
        {
            previewbar.DOKill();
            lifebar.DOKill();
            playerVariableContainer = null;
            return true;
        }

        public override bool IsLoadingOrCleaningUp()
        {
            return false;
        }

        public override void OnSave(JSON json)
        {
            json.Add("currentLifePoints", currentLifePoints);
            json.Add("maxLifePoints", maxLifePoints);
            json.Add("currentPreviewRange", currentPreviewRange);
            json.Add("currentPreviewTarget", currentPreviewTarget);
            json.Add("currentLifeTarget", currentLifeTarget);
        }

        public override bool OnLoad(JSON json)
        {
            if (json.ContainsKey("currentLifePoints"))
                currentLifePoints = json.GetInt("currentLifePoints");
            if (json.ContainsKey("maxLifePoints"))
                maxLifePoints = json.GetInt("maxLifePoints");
            if (json.ContainsKey("currentPreviewRange"))
                currentPreviewRange = json.GetInt("currentPreviewRange");
            if (json.ContainsKey("currentPreviewTarget"))
                currentPreviewTarget = json.GetFloat("currentPreviewTarget");
            if (json.ContainsKey("currentLifeTarget"))
                currentLifeTarget = json.GetFloat("currentLifeTarget");

            Refresh(true, false);

            return true;
        }
    }
}

