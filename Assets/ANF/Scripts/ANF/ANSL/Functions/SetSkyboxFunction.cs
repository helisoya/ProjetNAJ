using Leguar.TotalJSON;
using ANF.Scene;

namespace ANF.ANSL
{
    /// <summary>
    /// The Set Skybox function can be used to change the skybox
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setSkybox",
        functionAutoComplete: new string[] {
            "setSkybox(Skybox)",
            "setSkybox(Skybox;Duration;WaitForEnd)"
        },
        functionDesc: "Changes the current background's skybox")]
    public class SetSkyboxFunction : ANSLFunction
    {
        private bool waitForEnd;
        private BackgroundManager backgroundManager;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING },
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.FLOAT, FunctionParameterType.BOOL }
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string skybox) &&
                manager.GetWorld().GetComponent<BackgroundManager>(out backgroundManager))
            {
                bool immediate = parameters.GetTemplateId() == 0;
                float duration = 2.0f;

                if(!immediate)
                {
                    if (!parameters.GetParameter(1, out duration))
                        duration = 2.0f;

                    parameters.GetParameter(2, out waitForEnd);
                }

                backgroundManager.SetSkybox(skybox,immediate,duration);
            }

            if(!waitForEnd)
                EndProcess();
        }

        protected override void OnUpdate()
        {
            if (backgroundManager == null)
                manager.GetWorld().GetComponent<BackgroundManager>(out backgroundManager);

            if(backgroundManager != null && waitForEnd)
            {
                waitForEnd = backgroundManager.lerpingSkybox;
            }

            if (!waitForEnd)
                EndProcess();
        }

        protected override void OnCleanup()
        {
            backgroundManager = null;
        }

        protected override void OnSave(JSON json)
        {
            json.Add("waitForEnd", waitForEnd);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitForEnd"))
                waitForEnd = json.GetBool("waitForEnd");
        }
    }
}

