using ANF.Scene;
using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Screenshake function can be used to do screenshakes
    /// </summary>
    [ANSLFunctionAttribute(
        functionBody: "screenshake",
        functionAutoComplete: new string[] {
            "screenshake(Strength;Duration)",
            "screenshake(Strength;Duration;ShakeCamera;ShakeUI)"
        },
        functionDesc: "Starts a screen shake")]
    public class ScreenshakeFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.FLOAT, FunctionParameterType.FLOAT },
                new FunctionParameterType[]{FunctionParameterType.FLOAT, FunctionParameterType.FLOAT, FunctionParameterType.BOOL, FunctionParameterType.BOOL },
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out float strength) &&
                parameters.GetParameter(1, out float duration) &&
                manager.GetWorld().GetComponent(out MainCameraController cameraController))
            {
                if (parameters.GetTemplateId() == 1 &&
                    parameters.GetParameter(2, out bool shakeCamera) &&
                    parameters.GetParameter(3, out bool shakeUI))
                    cameraController.ShakeScreen(strength, duration, shakeCamera, shakeUI);
                else
                    cameraController.ShakeScreen(strength, duration);
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

