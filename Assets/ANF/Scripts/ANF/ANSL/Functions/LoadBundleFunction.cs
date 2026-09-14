using Leguar.TotalJSON;
using ANF.Persistent;
using UnityEngine;

namespace ANF.ANSL
{
    /// <summary>
    /// The Load Bundle function can be used to load bundle in memory
    /// </summary>
    [ANSLFunctionAttribute(
        functionBody: "loadBundle",
        functionAutoComplete: new string[] {
            "loadBundle(Bundle;DataType)",
        },
        functionDesc: "Loads a bundle in memory (AudioClip / TextAsset / Sprite / Texture2D / GameObject)")]
    public class LoadBundleFunction : ANSLFunction
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
                        resourceManager.LoadBundle(bundle, ResourceManager.BundleType.AudioClip);
                        break;
                    case "textasset":
                        resourceManager.LoadBundle(bundle, ResourceManager.BundleType.TextAsset);
                        break;
                    case "sprite":
                        resourceManager.LoadBundle(bundle, ResourceManager.BundleType.Sprite);
                        break;
                    case "texture2d":
                        resourceManager.LoadBundle(bundle, ResourceManager.BundleType.Texture2D);
                        break;
                    case "gameobject":
                        resourceManager.LoadBundle(bundle, ResourceManager.BundleType.GameObject);
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

