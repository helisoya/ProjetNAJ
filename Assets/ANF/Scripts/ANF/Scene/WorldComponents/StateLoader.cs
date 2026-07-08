using ANF.ANSL;
using ANF.GUI;
using ANF.Persistent;
using ANF.Utils;
using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.Scene
{
    /// <summary>
	/// The state loader component is responsible for loading the state of the game
	/// </summary>
    [System.Serializable]
    public class StateLoader : WorldComponent
    {
        [SerializeField] private string fadeAllName = "fadeAll";

#if UNITY_EDITOR
        [Header("Debug Mode")]
        [SerializeField] private bool startInDebugMode;
        [SerializeField] private string debugModeScript;
        [SerializeField] private bool autoLoadSaveFile;
        [SerializeField] private string saveFileToAutoLoad = "autosave";
#endif


        public override WorldComponent CloneComponent()
        {
            return new StateLoader()
            {
                canBeSaved = canBeSaved,
                enabledByDefault = enabledByDefault,
                fadeAllName = fadeAllName,
#if UNITY_EDITOR
                startInDebugMode = startInDebugMode,
                debugModeScript = debugModeScript,
                autoLoadSaveFile = autoLoadSaveFile,
                saveFileToAutoLoad = saveFileToAutoLoad
#endif
            };
        }



        public override void OnInitialize()
        {
        }

        public override void OnStart()
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent<LoadStateContainer>(out LoadStateContainer container))
            {
                ANSLManager anslManager = null;
                manager.GetWorld().GetComponent<ANSLManager>(out anslManager);

                if (container.loadingASaveFile)
                {
                    SaveUtils.LoadPlayerData(PersistentDataManager.instance.GetPlayerData(), manager, container.GetSaveFileToLoad());
                }
                else
                {
                    if (manager.GetGUIManager().GetComponent<GUI.Fade>(fadeAllName, out GUI.Fade fade))
                    {
                        fade.FadeAlphaTo(1, true);
                        fade.FadeAlphaTo(0, false, 1f);
                    }


#if UNITY_EDITOR
                    if (autoLoadSaveFile)
                    {
                        string savePath = Utils.FileManager.savPath + PersistentDataManager.instance.GetANFSettings().saveFolder
                            + saveFileToAutoLoad + ".json";
                        SaveUtils.LoadPlayerData(PersistentDataManager.instance.GetPlayerData(), manager, savePath);
                        return;
                    }
                    else if (startInDebugMode)
                    {
                        string resolvedScript = ANSLUtils.ResolveFilePath(null, debugModeScript);

                        if (anslManager != null && resolvedScript != null)
                            anslManager.StartNewContext(resolvedScript);
                        return;
                    }
#endif
                    if (anslManager != null)
                    {
                        string resolvedScript = ANSLUtils.ResolveFilePath(null, container.GetScriptToLoad());

                        if (anslManager != null && resolvedScript != null)
                            anslManager.StartNewContext(resolvedScript);
                    }
                        
                }
            }
        }

        public override void OnUpdate()
        {
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

        public override void OnLoad(JSON json)
        {
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
    }

}
