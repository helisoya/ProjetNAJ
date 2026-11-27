using ANF.GUI;
using ANF.Scene;
using ANF.Utils;
using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Interaction Function can be used to start an interaction sequence
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "interaction",
        functionAutoComplete: new string[] {
            "interaction()",
            "interaction(AngleMin;AngleMax)",
            "interaction(AngleMin;AngleMax;UpdateFocalPoint)",
            "interaction(AngleMin;AngleMax;UpdateFocalPoint;SkipCameraRotation)"
        },
        functionDesc: "Starts the interaction mode")]
    public class InteractionFunction : ANSLFunction
    {
        private InteractionMode interactionMode;
        private bool waitingForInteraction;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{},
                new FunctionParameterType[]{ FunctionParameterType.FLOAT, FunctionParameterType.FLOAT},
                new FunctionParameterType[]{ FunctionParameterType.FLOAT, FunctionParameterType.FLOAT, FunctionParameterType.BOOL},
                new FunctionParameterType[]{ FunctionParameterType.FLOAT, FunctionParameterType.FLOAT, FunctionParameterType.BOOL, FunctionParameterType.BOOL},
            };
        }

        protected override void OnStartProcess()
        {
            if (manager.GetGUIManager().GetComponent<DialogUI>(out DialogUI dialogUI))
                dialogUI.SetEnabled(false);

            if (parameters.GetTemplateId() != 0 &&
                parameters.GetParameter(0, out float angleMin) &&
                parameters.GetParameter(1, out float angleMax) &&
                manager.GetWorld().GetComponent(out MainCameraController cameraController))
            {
                bool skipLerps = true;
                if (!parameters.GetParameter(3, out skipLerps))
                    skipLerps = true;

                bool updateFocalPoint = true;
                if (!parameters.GetParameter(2, out updateFocalPoint))
                    updateFocalPoint = true;

                cameraController.SetAllowedCameraRotation(angleMin, angleMax);
                cameraController.SetCanManualRotate(true, skipLerps, updateFocalPoint);

                if (manager.GetGUIManager().GetComponent(out CameraManualPanUI panUI))
                    panUI.SetEnabled(true);
            }

            if (manager.GetWorld().GetComponent<InteractionMode>(out interactionMode))
            {
                interactionMode.StartInteractionMode(parameters.GetTemplateId() != 0);

                waitingForInteraction = true;
            }

            if (!waitingForInteraction)
                EndProcess();
        }

        protected override void OnUpdate()
        {
            if (interactionMode == null)
                manager.GetWorld().GetComponent<InteractionMode>(out interactionMode);

            if (interactionMode != null && waitingForInteraction &&
                !interactionMode.inInteractionMode)
            {
                waitingForInteraction = false;

                EndProcess();

                if (parameters.GetTemplateId() != 0)
                {
                    if (manager.GetWorld().GetComponent(out MainCameraController cameraController))
                        cameraController.SetCanManualRotate(false);

                    if (manager.GetGUIManager().GetComponent(out CameraManualPanUI panUI))
                        panUI.SetEnabled(false);
                }

                string selectedScript = interactionMode.selectedScript;
                string resolvedScript = ANSLUtils.ResolveFilePath(context.GetCurrentFilepath(), selectedScript);

                if (resolvedScript != null)
                    context.LoadScript(resolvedScript);
                interactionMode = null;
            }
        }

        protected override void OnCleanup()
        {

        }

        protected override void OnSave(JSON json)
        {
            if (waitingForInteraction)
                json.Add("waitingForInteraction", waitingForInteraction);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitingForInteraction"))
                waitingForInteraction = json.GetBool("waitingForInteraction");
        }
    }
}

