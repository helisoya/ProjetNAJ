using Leguar.TotalJSON;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Light Direction function can be used to change the current background's light direction
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setLightDirection",
        functionAutoComplete: new string[] {
            "setLightDirection(X;Y;Z)"
        },
        functionDesc: "Changes the current background's light direction")]
    public class SetLightDirectionFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.FLOAT, FunctionParameterType.FLOAT, FunctionParameterType.FLOAT }
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out float x) &&
                parameters.GetParameter(1, out float y) &&
                parameters.GetParameter(2, out float z) &&
                manager.GetWorld().GetComponent<ANF.Scene.BackgroundManager>(out ANF.Scene.BackgroundManager backgroundManager))
            {
                backgroundManager.SetLightDirection(new Vector3(x, y, z));
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

