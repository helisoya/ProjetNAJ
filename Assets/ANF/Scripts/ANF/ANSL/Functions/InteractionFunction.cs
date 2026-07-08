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
            "interaction()"
        },
        functionDesc: "Starts the interaction mode")]
    public class InteractionFunction : ANSLFunction
    {
        private InteractionMode interactionMode;
        private bool waitingForInteraction;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{}
            };
        }

        protected override void OnStartProcess()
        {
            if (manager.GetGUIManager().GetComponent<DialogUI>(out DialogUI dialogUI))
                dialogUI.SetEnabled(false);

            if (manager.GetWorld().GetComponent<InteractionMode>(out interactionMode))
            {
                interactionMode.StartInteractionMode();

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

                string selectedScript = interactionMode.selectedScript;
                string resolvedScript = ANSLUtils.ResolveFilePath(context.GetCurrentFilepath(), selectedScript);

                if(resolvedScript != null)
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

