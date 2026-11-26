using System.Collections.Generic;
using ANF.GUI;
using ANF.Locals;
using ANF.Persistent;
using DG.Tweening;
using Leguar.TotalJSON;
using NAJ.Persistent;
using Radishmouse;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NAJ.GUI
{
	/// <summary>
	/// Represents a visual node in the deductions UI
	/// </summary>
	class DeductionsUINode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
	{
		public enum Status
		{
			Shown,
			NotActiveLink,
			Hidden
		}

		[SerializeField] private RectTransform root;
		[SerializeField] private RectTransform visualTransform;
		[SerializeField] private Image visualImage;
		[SerializeField] private LocalizedText label;
		[SerializeField] private CanvasGroup labelCanvasGroup;
		private DeductionNode.LabelPosition currentLabelPosition = DeductionNode.LabelPosition.Left;
		public Status status { get; private set; }
		private DeductionNode data;
		private UILineRenderer[] prerequistesLinkImages;
		private float currentFillAmount = 0.0f;
		private Tweener currentFillAmountTweener;
		private Material material;
		private DeductionsUI deductionsUI;
		public int id { get; private set; }
		private int linkCount = 0;

		/// <summary>
		/// Cleans up the component before shut down
		/// </summary>
		public void Cleanup()
		{
			data = null;
			if (currentFillAmountTweener != null && !currentFillAmountTweener.IsActive())
				currentFillAmountTweener.Kill();

			if (prerequistesLinkImages != null)
				foreach (UILineRenderer line in prerequistesLinkImages)
					line.SetMaterial(null);

			Destroy(material);
			material = null;
		}

		/// <summary>
		/// Initializes the component
		/// </summary>
		/// <param name="data">The node's data</param>
		/// <param name="Status">The node's status</param>
		/// <param name="baseSize">The base node size</param>
		/// <param name="prerequistesLinkImages">The linked line renderers</param>
		/// <param name="material">The linked material</param>
		/// <param name="deductionsUI">The parent UI</param>
		/// <param name="id">The component's id</param>
		public void Init(DeductionNode data, Status status, float baseSize,
			UILineRenderer[] prerequistesLinkImages, Material material, DeductionsUI deductionsUI, int id)
		{
			linkCount = 0;
			this.deductionsUI = deductionsUI;
			this.id = id;
			this.data = data;
			this.prerequistesLinkImages = prerequistesLinkImages;
			this.material = material;
			if (prerequistesLinkImages != null)
				foreach (UILineRenderer line in prerequistesLinkImages)
					line.SetMaterial(material);

			SetNodeSize(baseSize * data.uiSizeFactor, false);
			SetLabelKey(data.GetName(), false);
			SetLabelPosition(data.labelPosition);
			SetStatus(status);
		}

		/// <summary>
		/// Resets the link count
		/// </summary>
		public void ResetLinkCount()
		{
			linkCount = 0;
		}

		/// <summary>
		/// Increments the link count
		/// </summary>
		public void IncrementLinkCount()
		{
			linkCount++;
		}

		/// <summary>
		/// Updates the visual part linked to the link count
		/// </summary>
		/// <param name="immediate"></param>
		public void UpdateLinkCountVisual(bool immediate)
		{
			if (data.isEndNode || !string.IsNullOrEmpty(data.linkedNode) || status != Status.Shown)
				return;

			Color colorTarget = linkCount == 0 ? Color.gray : Color.yellow;
			if (immediate)
				visualImage.color = colorTarget;
			else
				visualImage.DOColor(colorTarget, 0.5f).SetEase(Ease.OutQuad);
		}

		/// <summary>
		/// Gets the component's data
		/// </summary>
		/// <returns>The component's data</returns>
		public DeductionNode GetData()
		{
			return data;
		}

		/// <summary>
		/// Changes the visual's status
		/// </summary>
		/// <param name="status">True if active</param>
		/// <param name="immediate">True if immediate</param>
		public void SetStatus(Status status, bool immediate = true)
		{
			float targetSize = status == Status.Hidden ? 0.0f : 1.0f;
			if (immediate)
				root.localScale = new Vector3(targetSize, targetSize, targetSize);
			else
				root.DOScale(targetSize, 0.5f).SetEase(Ease.OutQuad);

			Color colorTarget = status == Status.NotActiveLink ? Color.gray : Color.yellow;
			if (immediate)
				visualImage.color = colorTarget;
			else
				visualImage.DOColor(colorTarget, 0.5f).SetEase(Ease.OutQuad);

			float notActiveSize = status == Status.NotActiveLink ? 0.8f : 1.0f;
			if (immediate)
				visualTransform.localScale = new Vector3(notActiveSize, notActiveSize);
			else
				visualTransform.DOScale(notActiveSize, 0.5f).SetEase(Ease.OutQuad);

			if (status == Status.NotActiveLink)
			{
				transform.SetAsFirstSibling();
				transform.DOComplete();
			}
			else if (this.status == Status.NotActiveLink)
			{
				transform.SetAsLastSibling();
				transform.DOComplete();
			}


			float labelAlphaTarget = status == Status.Shown ? 1.0f : 0.0f;
			if (immediate)
				labelCanvasGroup.alpha = labelAlphaTarget;
			else
				labelCanvasGroup.DOFade(labelAlphaTarget, 0.5f).SetEase(Ease.OutQuad);

			if (prerequistesLinkImages != null)
			{
				if (currentFillAmountTweener != null && !currentFillAmountTweener.IsActive())
					currentFillAmountTweener.Kill();

				if (immediate)
				{
					currentFillAmount = labelAlphaTarget;
					if (material)
						material.SetFloat("_FillAmount", labelAlphaTarget);
				}
				else
				{
					currentFillAmountTweener = DOVirtual.Float(currentFillAmount, labelAlphaTarget, 0.5f, (float newValue) =>
					{
						currentFillAmount = newValue;
						if (material)
							material.SetFloat("_FillAmount", currentFillAmount);
					});
				}
			}

			this.status = status;
		}

		/// <summary>
		/// Changes the node's size
		/// </summary>
		/// <param name="size">The new size</param>
		/// <param name="immediateUpdate">True if the label update should happen immediately</param>
		public void SetNodeSize(float size, bool immediateUpdate = true)
		{
			visualTransform.sizeDelta = new Vector2(size, size);

			if (immediateUpdate)
				UpdateLabelPosition();
		}

		/// <summary>
		/// Changes the node's text label
		/// </summary>
		/// <param name="labelKey">The new label's key</param>
		/// <param name="immediateUpdate">True if the label update should happen immediately</param>
		public void SetLabelKey(string labelKey, bool immediateUpdate = true)
		{
			label.SetNewKey(labelKey);
			label.GetText().ForceMeshUpdate(true, true);

			if (immediateUpdate)
				UpdateLabelPosition();
		}

		/// <summary>
		/// Changes the label's anchor
		/// </summary>
		/// <param name="labelPosition">The label's anchoring position</param>
		/// <param name="immediateUpdate">True if the label update should happen immediately</param>
		public void SetLabelPosition(DeductionNode.LabelPosition labelPosition, bool immediateUpdate = true)
		{
			currentLabelPosition = labelPosition;

			if (immediateUpdate)
				UpdateLabelPosition();
		}

		/// <summary>
		/// Updates the label's position
		/// </summary>
		public void UpdateLabelPosition()
		{
			RectTransform rectTransform = label.GetText().GetComponent<RectTransform>();
			switch (currentLabelPosition)
			{
				case DeductionNode.LabelPosition.Top:
					label.GetText().horizontalAlignment = TMPro.HorizontalAlignmentOptions.Center;
					label.GetText().verticalAlignment = TMPro.VerticalAlignmentOptions.Geometry;
					rectTransform.anchoredPosition = new Vector2(0, visualTransform.sizeDelta.x / 2.0f);
					break;
				case DeductionNode.LabelPosition.Bottom:
					label.GetText().horizontalAlignment = TMPro.HorizontalAlignmentOptions.Center;
					label.GetText().verticalAlignment = TMPro.VerticalAlignmentOptions.Geometry;
					rectTransform.anchoredPosition = new Vector2(0, -visualTransform.sizeDelta.x / 2.0f);
					break;
				case DeductionNode.LabelPosition.Left:
					label.GetText().horizontalAlignment = TMPro.HorizontalAlignmentOptions.Right;
					label.GetText().verticalAlignment = TMPro.VerticalAlignmentOptions.Geometry;
					rectTransform.anchoredPosition = new Vector2(-visualTransform.sizeDelta.x / 2.0f - rectTransform.sizeDelta.x / 2.0f, 0);
					break;
				case DeductionNode.LabelPosition.Right:
					label.GetText().horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
					label.GetText().verticalAlignment = TMPro.VerticalAlignmentOptions.Geometry;
					rectTransform.anchoredPosition = new Vector2(visualTransform.sizeDelta.x / 2.0f + rectTransform.sizeDelta.x / 2.0f, 0);
					break;
			}
		}

		/// <summary>
		/// Highlights the node
		/// </summary>
		public void Highlight()
		{
			Color colorTarget =
				(status == Status.NotActiveLink || (linkCount == 0 && !data.isEndNode && string.IsNullOrEmpty(data.linkedNode))) ?
				new Color(0.6f, 0.6f, 0.6f) : new Color(1.0f, 1.0f, 0.116f);
			visualImage.DOColor(colorTarget, 0.5f).SetEase(Ease.OutQuad);

			float targetSize = status == Status.Hidden ? 0.0f : 1.1f;
			root.DOScale(targetSize, 0.5f).SetEase(Ease.OutQuad);
		}

		/// <summary>
		/// Unhighlights the node
		/// </summary>
		public void UnHighlight()
		{
			Color colorTarget =
				(status == Status.NotActiveLink || (linkCount == 0 && !data.isEndNode && string.IsNullOrEmpty(data.linkedNode))) ?
				Color.gray : Color.yellow;
			visualImage.DOColor(colorTarget, 0.5f).SetEase(Ease.OutQuad);

			float targetSize = status == Status.Hidden ? 0.0f : 1.0f;
			root.DOScale(targetSize, 0.5f).SetEase(Ease.OutQuad);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (status != Status.Hidden && deductionsUI.currentMode == DeductionsUI.UIMode.Default)
				deductionsUI.SetCurrentButton(id);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			//OnExit();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (status != Status.Hidden)
			{
				deductionsUI.SelectCurrentButton();
			}
		}
	}
}