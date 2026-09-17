using ANF.Scene;
using Leguar.TotalJSON;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Fog Density Function can be used to change the fog's density
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "setFogDensity",
        functionAutoComplete: new string[] {
            "setFogDensity(Density)",
            "setFogDensity(Density;Duration;WaitForEnd)",
        },
        functionDesc: "Changes the fog's density")]
    public class SetFogDensityFunction : ANSLFunction
    {
        private bool waitingForObject = false;
        private FogController fogController;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.FLOAT },
                new FunctionParameterType[]{FunctionParameterType.FLOAT, FunctionParameterType.FLOAT, FunctionParameterType.BOOL},
            };
        }

        protected override void OnStartProcess()
        {
            bool endProcess = true;
            if (manager.GetWorld().GetComponent(out fogController) &&
                parameters.GetParameter(0, out float density))
            {
                float duration = 1.0f;
                bool waitForEnd = false;
                bool immediate = parameters.GetTemplateId() == 0;

                if (parameters.GetTemplateId() == 1)
                {
                    if (!parameters.GetParameter(4, out waitForEnd))
                        waitForEnd = false;

                    if (!parameters.GetParameter(3, out duration))
                        duration = 1.0f;
                }

                fogController.SetFogDensity(density, immediate, duration);
                waitingForObject = !immediate && waitForEnd;
                endProcess = !waitingForObject;
            }

            if (endProcess)
                EndProcess();
        }

        protected override void OnUpdate()
        {
            if (fogController == null)
            {
                if (!manager.GetWorld().GetComponent(out fogController))
                    return;
            }

            if (fogController != null && waitingForObject)
            {
                if (!fogController.LerpingColor)
                {
                    waitingForObject = false;
                    fogController = null;
                    EndProcess();
                }
            }
        }

        protected override void OnCleanup()
        {
            fogController = null;
        }

        protected override void OnSave(JSON json)
        {
            json.Add("waitingForObject", waitingForObject);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitingForObject"))
                waitingForObject = json.GetBool("waitingForObject");
        }
    }
}

