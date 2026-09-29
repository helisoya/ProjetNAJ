using ANF.GUI;
using ANF.Persistent;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace ANF.Scene
{
    /// <summary>
    /// Handles persistent data and other Manager
    /// </summary>
    public class ANFManager : MonoBehaviour, Jsonable
    {
        /// <summary>
        /// Represents the state for the scene
        /// </summary>
        private enum GameState
        {
            WaitingForCleanUp,
            WaitingForLoad,
            InGame,
            WaitingForChangeScene
        }

        [Header("General")]
        [SerializeField] private RectTransform uiRoot;
        [SerializeField] private ANFSceneData sceneData;

        private bool waitingForCleanup = false;
        private bool initializedCleanup;
        private string nextSceneToLoad = null;

        private GameState currentGameState;
        private AsyncOperation cleanupOperation;

        private World world;
        private GUIManager guiManager;

        /// <summary>
		/// Gets the GUI Manager
		/// </summary>
		/// <returns>The GUI Manager</returns>
        public GUIManager GetGUIManager()
        {
            return guiManager;
        }

        /// <summary>
		/// Gets the world
		/// </summary>
		/// <returns>The world</returns>
        public World GetWorld()
        {
            return world;
        }

        void Update()
        {
            switch (currentGameState)
            {
                case GameState.WaitingForCleanUp:
                    {
                        if (!cleanupOperation.isDone)
                            return;

                        currentGameState = GameState.InGame;

                        if (sceneData.changeSceneUseFading && guiManager.GetComponent<GUI.Fade>(sceneData.changeSceneFadingName, out GUI.Fade fade))
                        {
                            fade.FadeAlphaTo(0);
                        }

                        OnStartComponents();
                    }
                    break;

                case GameState.WaitingForChangeScene:
                    {
                        if (sceneData.changeSceneUseFading &&
                            guiManager.GetComponent<GUI.Fade>(sceneData.changeSceneFadingName, out GUI.Fade fade))
                        {
                            fade.OnUpdate();
                            if (fade.fadingAlpha)
                                return;
                        }

                        if (!initializedCleanup)
                        {
                            initializedCleanup = true;

                            waitingForCleanup = guiManager.OnChangeScene();

                            if (!world.OnChangeScene())
                                waitingForCleanup = false;

                            if (waitingForCleanup)
                                return;
                        }
                        else if (waitingForCleanup)
                        {
                            if (world.IsLoadingOrCleaningUp() ||
                               guiManager.IsLoadingOrCleaningUp())
                                return;

                            waitingForCleanup = false;
                        }

                        SceneManager.LoadScene(nextSceneToLoad);
                    }
                    break;

                case GameState.WaitingForLoad:
                    {
                        if (world.IsLoadingOrCleaningUp() ||
                        guiManager.IsLoadingOrCleaningUp())
                            return;

                        currentGameState = GameState.InGame;
                    }
                    break;

                case GameState.InGame:
                    {
                        world.OnUpdate();
                        guiManager.OnUpdate();
                    }
                    break;

            }
        }

        void Start()
        {
            if (!PersistentDataManager.instance.hasLoaded)
                PersistentDataManager.instance.Load();

            InitializeComponents();

            if (sceneData.changeSceneUseFading && guiManager.GetComponent<GUI.Fade>(sceneData.changeSceneFadingName, out GUI.Fade fade))
            {
                fade.FadeAlphaTo(1, true);
            }

            cleanupOperation = Resources.UnloadUnusedAssets();
            currentGameState = GameState.WaitingForCleanUp;
        }

        /// <summary>
		/// Calls the On Start callback on all components
		/// </summary>
        private void OnStartComponents()
        {
            world.OnStart();
            guiManager.OnStart();
        }

        /// <summary>
        /// Initialize the various components (GUI & World Components)
        /// </summary>
        private void InitializeComponents()
        {
            guiManager = new GUIManager(this, uiRoot, sceneData.registeredGUIComponents);
            world = new World(this, sceneData.registeredWorldComponents);
        }

        /// <summary>
        /// Changes the current Unity Scene
        /// </summary>
        /// <param name="nextScene">The next scene</param>
        public void ChangeScene(string nextScene)
        {
            DOTween.KillAll(false);

            if (sceneData.changeSceneUseFading && guiManager.GetComponent<GUI.Fade>(sceneData.changeSceneFadingName, out GUI.Fade fade))
            {
                fade.SetEnabled(true);
                fade.SetPaused(false);
                fade.FadeAlphaTo(1);
            }

            currentGameState = GameState.WaitingForChangeScene;
            nextSceneToLoad = nextScene;
        }

        public void Save(JSON json)
        {
            JSON individualDataJson = new JSON();
            world.Save(individualDataJson);
            json.Add("world", individualDataJson);

            individualDataJson = new JSON();
            guiManager.Save(individualDataJson);
            json.Add("gui", individualDataJson);
        }

        public bool Load(JSON json)
        {
            bool immediate = true;
            if (json.ContainsKey("gui"))
                if (!guiManager.Load(json.GetJSON("gui")))
                    immediate = false;

            if (json.ContainsKey("world"))
                if (!world.Load(json.GetJSON("world")))
                    immediate = false;

            if (!immediate)
                currentGameState = GameState.WaitingForLoad;

            return immediate;
        }
    }
}
