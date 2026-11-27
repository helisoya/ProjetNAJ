using ANF.Persistent;
using ANF.Scene;
using DG.Tweening;
using Leguar.TotalJSON;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace ANF.GUI
{
    /// <summary>
    /// Represents the camera manual pan UI
    /// </summary>
    public class CameraManualPanUI : GUIComponent
    {
        [Header("Components")]
        [SerializeField] private Scrollbar scrollbar;
        private MainCameraController mainCameraController;

        public override void OnInitialize()
        {
        }

        public override void OnStart()
        {
            manager.GetWorld().GetComponent(out mainCameraController);
        }

        public override void OnUpdate()
        {
            if (mainCameraController != null)
                scrollbar.value = mainCameraController.GetManualPanValue();
        }


        public override void OnEnabled()
        {
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

        private void OnPauseInput(InputAction.CallbackContext context)
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
            return true;
        }

        public override bool IsLoadingOrCleaningUp(bool updateComponent)
        {
            return false;
        }

        public override void OnSave(JSON json)
        {

        }

        public override bool OnLoad(JSON json)
        {
            return true;
        }
    }
}

