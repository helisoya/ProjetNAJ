using ANF.GUI;

namespace ANF.ANSL
{
    /// <summary>
    /// The Set Fg function sets immediately the Foreground's fade alpha without transition
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setFg",
        functionAutoComplete: new string[] {
            "setFg(Alpha)" },
        functionDesc: "Sets the Foreground's alpha to a specific value")]
    public class SetFgFunction : ANSLFunction
    {

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.FLOAT},
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out float target) &&
                manager.GetGUIManager().GetComponent<Fade>("fadeFg", out Fade currentFade))
            {
                currentFade.FadeAlphaTo(target, true);
            }
            EndProcess();
        }

        protected override void OnUpdate()
        {
            // Unused
        }

        protected override void OnCleanup()
        {
            // Unused
        }
    }
}

