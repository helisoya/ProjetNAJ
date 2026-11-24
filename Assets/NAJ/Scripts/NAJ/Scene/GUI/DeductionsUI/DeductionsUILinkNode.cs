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
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NAJ.GUI
{
	/// <summary>
	/// Represents a visual link node info in the deductions UI
	/// </summary>
	class DeductionsUILinkNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
	{
		[SerializeField] private DeductionsUI deductionsUI;
		[SerializeField] private bool isLeft;
		public void OnPointerEnter(PointerEventData eventData)
		{
			deductionsUI.SwitchLinkPopup(isLeft);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			//OnExit();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			deductionsUI.SelectCurrentSwitch();
		}
	}
}