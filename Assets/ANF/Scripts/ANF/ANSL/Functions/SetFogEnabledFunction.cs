using ANF.Scene;
using Leguar.TotalJSON;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Fog Enabled Function can be used to change the fog's visibility
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "setFogEnabled",
        functionAutoComplete: new string[] {
            "setFogEnabled(Enabled)"
        },
        functionDesc: "Changes if the fog is visible or not")]
    public class SetFogEnabledFunction : ANSLFunction
    {

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.BOOL },
            };
        }

        protected override void OnStartProcess()
        {
            if (manager.GetWorld().GetComponent(out FogController fogController) &&
                parameters.GetParameter(0, out bool enabled))
            {
                fogController.SetFogEnabled(enabled);
            }

            EndProcess();
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnCleanup()
        {
        }

        protected override void OnSave(JSON json)
        {
        }

        protected override void OnLoad(JSON json)
        {
        }
    }
}

