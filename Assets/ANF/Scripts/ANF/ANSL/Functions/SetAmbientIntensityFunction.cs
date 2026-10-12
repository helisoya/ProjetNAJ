using ANF.Scene;
using Leguar.TotalJSON;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Ambient Intensity Function can be used to change the ambient light intensity
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "setAmbientIntensity",
        functionAutoComplete: new string[] {
            "setAmbientIntensity(Intensity)",
            "setAmbientIntensity(Intensity;Duration;WaitForEnd)",
        },
        functionDesc: "Changes the ambient lighting's intensity")]
    public class SetAmbientIntensityFunction : ANSLFunction
    {
        private bool waitingForObject = false;
        private SceneRenderController fogController;

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

                fogController.SetAmbientIntensity(density, immediate, duration);
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

