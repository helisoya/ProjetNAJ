using ANF.GUI;
using ANF.Utils;
using DG.Tweening;
using Leguar.TotalJSON;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ANF.Scene
{
    /// <summary>
    /// Handles persistent data and other Manager
    /// </summary>
    public class ANFManager : MonoBehaviour, Jsonable
    {
        [Header("General")]
        [SerializeField] private RectTransform uiRoot;
        [SerializeField] private ANFSceneData sceneData;

        private bool isChangingScene = false;
        private string nextSceneToLoad = null;

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
            if (isChangingScene)
            {
                if (sceneData.changeSceneUseFading &&
                    guiManager.GetComponent<GUI.Fade>(sceneData.changeSceneFadingName, out GUI.Fade fade))
                {
                    fade.OnUpdate();
                    if (fade.fadingAlpha)
                        return;
                }

                SceneManager.LoadScene(nextSceneToLoad);
            }
            else
            {
                world.OnUpdate();
                guiManager.OnUpdate();
            }
        }

        void Start()
        {
            InitializeComponents();
            OnStartComponents();
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
            if (sceneData.changeSceneUseFading && guiManager.GetComponent<GUI.Fade>(sceneData.changeSceneFadingName, out GUI.Fade fade))
            {
                DOTween.KillAll(false);
                guiManager.OnChangeScene();
                world.OnChangeScene();

                isChangingScene = true;
                nextSceneToLoad = nextScene;

                fade.SetEnabled(true);
                fade.SetPaused(false);
                fade.FadeAlphaTo(1);
            }
            else
            {
                SceneManager.LoadScene(nextSceneToLoad);
            }
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

        public void Load(JSON json)
        {
            if (json.ContainsKey("gui"))
                guiManager.Load(json.GetJSON("gui"));

            if (json.ContainsKey("world"))
                world.Load(json.GetJSON("world"));
        }
    }
}
