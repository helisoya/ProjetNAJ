using Leguar.TotalJSON;
using ANF.Persistent;
using UnityEngine;

namespace ANF.ANSL
{
    /// <summary>
    /// The Unload Bundle function can be used to unload a bundle from memory
    /// </summary>
    [ANSLFunctionAttribute(
        functionBody: "unloadBundle",
        functionAutoComplete: new string[] {
            "unloadBundle(Bundle;DataType)",
        },
        functionDesc: "Unloads a bundle from memory (AudioClip / TextAsset / Sprite / Texture2D / GameObject)")]
    public class UnloadBundleFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING,FunctionParameterType.STRING },
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string bundle) &&
                parameters.GetParameter(1, out string type) &&
                PersistentDataManager.instance.GetPlayerData().GetComponent(out ResourceManager resourceManager))
            {
                type = type.ToLower();
                switch (type)
                {
                    case "audioclip":
                        resourceManager.UnloadBundle(bundle, ResourceManager.BundleType.AudioClip);
                        break;
                    case "textasset":
                        resourceManager.UnloadBundle(bundle, ResourceManager.BundleType.TextAsset);
                        break;
                    case "sprite":
                        resourceManager.UnloadBundle(bundle, ResourceManager.BundleType.Sprite);
                        break;
                    case "texture2d":
                        resourceManager.UnloadBundle(bundle, ResourceManager.BundleType.Texture2D);
                        break;
                    case "gameobject":
                        resourceManager.UnloadBundle(bundle, ResourceManager.BundleType.GameObject);
                        break;
                }

            }

            EndProcess();
        }

        protected override void OnUpdate()
        {

        }

        protected override void OnCleanup()
        {
            // Unused
        }

        protected override void OnSave(JSON json)
        {

        }

        protected override void OnLoad(JSON json)
        {

        }
    }
}

