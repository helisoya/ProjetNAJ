using ANF.Scene;
using Leguar.TotalJSON;

namespace ANF.ANSL
{
    /// <summary>
    /// The Set Interaction Script can be used to change an interactable object's linked script
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setInteractionScript",
        functionAutoComplete: new string[] {
            "setInteractionScript(Id;Script)"
        },
        functionDesc: "Changes an interactable object's linked script")]
    public class SetInteractionScriptFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING }
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string id) &&
                parameters.GetParameter(1, out string script) &&
                manager.GetWorld().GetComponent<InteractionMode>(out InteractionMode interactionMode))
            {
                interactionMode.SetInteractableObjectNextScript(id, script);
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

