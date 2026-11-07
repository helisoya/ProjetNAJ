using ANF.GUI;
using Leguar.TotalJSON;

namespace ANF.ANSL
{
    /// <summary>
    /// The Stop UI Animation Function can be used to stop UI Animations
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "stopUIAnimation",
        functionAutoComplete: new string[]
        {
            "stopUIAnimation(Id)"
        },
        functionDesc: "Stops a UI Animation")]
    public class StopUIAnimationFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{ FunctionParameterType.STRING}
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string id) &&
                manager.GetGUIManager().GetComponent(out AnimationUI animationUI))
            {
                animationUI.StopAnimation(id);
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