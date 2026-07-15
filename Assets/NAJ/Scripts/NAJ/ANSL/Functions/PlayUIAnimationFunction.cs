using ANF.ANSL;
using Leguar.TotalJSON;
using NAJ.GUI;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Play UI Animation Function can be used to play UI Animations
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "playUIAnimation",
        functionAutoComplete: new string[]
        {
            "playUIAnimation(Id;WaitForEnd)"
        },
        functionDesc: "Plays a UI Animation")]
    public class PlayUIAnimationFunction : ANSLFunction
    {
        private bool waitForEnd;
        private string id;
        private AnimationUI animationUI;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{ FunctionParameterType.STRING, FunctionParameterType.BOOL}
            };
        }

        protected override void OnStartProcess()
        {
            waitForEnd = false;

            if (parameters.GetParameter(0, out id) &&
                parameters.GetParameter(1, out waitForEnd) &&
                manager.GetGUIManager().GetComponent(out animationUI))
            {
                animationUI.PlayAnimation(id);
            }

            if (!waitForEnd)
                EndProcess();
        }

        protected override void OnUpdate()
        {
            if (!animationUI && !manager.GetGUIManager().GetComponent(out animationUI))
            {
                EndProcess();
                return;
            }

            if (!animationUI.IsAnimationInProgress(id))
            {
                EndProcess();
            }
        }

        protected override void OnCleanup()
        {
            animationUI = null;
        }

        protected override void OnSave(JSON json)
        {
            json.Add("waitForEnd", waitForEnd);
            json.Add("id", id);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitForEnd"))
                waitForEnd = json.GetBool("waitForEnd");
            if (json.ContainsKey("id"))
                id = json.GetString("id");
        }
    }
}