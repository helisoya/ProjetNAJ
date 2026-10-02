using Leguar.TotalJSON;
using ANF.Scene;

namespace ANF.ANSL
{
    /// <summary>
    /// The Set Volume Weight Function allows you to change a post process volume's weight
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "setVolumeWeight",
        functionAutoComplete: new string[] {
            "setVolumeWeight(Name;Weight)",
            "setVolumeWeight(Name;Weight;Duration;WaitForEnd)"
        },
        functionDesc: "Sets a post process volume's weight")]
    public class SetVolumeWeightFunction : ANSLFunction
    {
        private bool waitingForObject = false;
        private string currentVolumeName;
        private PostProcessManager postProcessManager;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.FLOAT },
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.FLOAT,
                    FunctionParameterType.FLOAT, FunctionParameterType.BOOL},
            };
        }

        protected override void OnStartProcess()
        {
            bool endProcess = true;
            if (parameters.GetParameter(0, out currentVolumeName) &&
                parameters.GetParameter(1, out float weight) &&
                manager.GetWorld().GetComponent(out postProcessManager))
            {
                float duration = 1.0f;
                bool waitForEnd = false;
                bool immediate = parameters.GetTemplateId() == 0;

                if (parameters.GetTemplateId() == 1)
                {
                    if (parameters.GetTemplateId() == 1)
                    {
                        if (!parameters.GetParameter(3, out waitForEnd))
                            waitForEnd = false;

                        if (!parameters.GetParameter(2, out duration))
                            duration = 1.0f;
                    }
                }

                postProcessManager.SetVolume(currentVolumeName, weight, immediate, duration);
                waitingForObject = !immediate && waitForEnd;
                endProcess = !waitingForObject;
            }

            if (endProcess)
                EndProcess();
        }

        protected override void OnUpdate()
        {
            if (postProcessManager == null)
            {
                if (!manager.GetWorld().GetComponent(out postProcessManager))
                    return;
            }

            if (postProcessManager != null && waitingForObject)
            {
                if (!postProcessManager.IsLerpingVolume(currentVolumeName))
                {
                    waitingForObject = false;
                    currentVolumeName = null;
                    postProcessManager = null;
                    EndProcess();
                }
            }
        }

        protected override void OnCleanup()
        {
            postProcessManager = null;
        }

        protected override void OnSave(JSON json)
        {
            json.Add("waitingForObject", waitingForObject);
            json.Add("currentVolumeName", currentVolumeName);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitingForObject"))
                waitingForObject = json.GetBool("waitingForObject");

            if (json.ContainsKey("currentVolumeName"))
                currentVolumeName = json.GetString("currentVolumeName");
        }
    }
}

