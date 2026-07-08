using ANF.Persistent;
using ANF.Utils;
using UnityEngine.SceneManagement;


namespace ANF.ANSL
{
    /// <summary>
    /// The autosave function makes an automatic save. The autosave.json file is the default save file, but regular save files can also be autosaved to.
    /// </summary>
    [ANSLFunctionAttribute(
        functionBody: "autosave",
        functionAutoComplete: new string[] {
            "autosave()",
            "autosave(SaveIndex)" },
        functionDesc: "Starts an autosave")]
    public class AutoSaveFunction : ANSLFunction
    {

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{},
                new FunctionParameterType[]{FunctionParameterType.UINT},
            };
        }

        protected override void OnStartProcess()
        {
            string savePath = Utils.FileManager.savPath + PersistentDataManager.instance.GetANFSettings().saveFolder + "autosave.json";
            SaveUtils.SavePlayerData(PersistentDataManager.instance.GetPlayerData(), manager, SceneManager.GetActiveScene().name, savePath);

            EndProcess();
        }

        protected override void OnUpdate()
        {
            // Unused
        }

        protected override void OnCleanup()
        {
            // Unused
        }
    }
}

