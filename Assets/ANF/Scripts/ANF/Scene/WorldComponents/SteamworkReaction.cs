using ANF.GUI;
using ANF.Persistent;
using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ANF.Scene
{
    /// <summary>
    /// Handles the game's reaction to steamwork events
    /// </summary>
    [System.Serializable]
    public class SteamworkReaction : WorldComponent
    {
        [SerializeField] private SteamOverlayOpenReaction overlayOpenReaction = SteamOverlayOpenReaction.OpenPauseMenu;


        private ANFSteamworkContainer steamworkContainer;
        private PauseMenuUI pauseMenuUI;

        public enum SteamOverlayOpenReaction
        {
            None,
            OpenPauseMenu
        }

        public override WorldComponent CloneComponent()
        {
            return new SteamworkReaction();
        }

        public override void OnInitialize()
        {
        }

        public override void OnStart()
        {
            PersistentDataManager.instance.GetPlayerData().GetComponent(out steamworkContainer);
            manager.GetGUIManager().GetComponent(out pauseMenuUI);
        }

        public override void OnUpdate()
        {
            if (steamworkContainer == null)
                return;

            if (steamworkContainer.overlayOn &&
                overlayOpenReaction == SteamOverlayOpenReaction.OpenPauseMenu &&
                !pauseMenuUI.isEnabled)
                pauseMenuUI.SetEnabled(true);
        }

        public override void OnPaused()
        {
        }

        public override void OnUnPaused()
        {

        }

        public override void OnEnabled()
        {

        }

        public override void OnDisabled()
        {

        }

        public override void OnSave(JSON json)
        {

        }

        public override bool OnLoad(JSON json)
        {
            return true;
        }

        public override void OnRegisterInputs()
        {

        }

        public override void OnUnRegisterInputs()
        {

        }

        public override bool OnChangeScene()
        {
            pauseMenuUI = null;
            steamworkContainer = null;
            return true;
        }

        public override bool IsLoadingOrCleaningUp(bool updateComponent)
        {
            return false;
        }
    }
}

