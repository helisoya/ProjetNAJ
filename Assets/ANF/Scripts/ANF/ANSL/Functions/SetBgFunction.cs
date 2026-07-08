namespace ANF.ANSL
{
    /// <summary>
    /// The Set Bg function sets immediately the Background's fade alpha without transition
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setBg",
        functionAutoComplete: new string[] {
            "setBg(Alpha)" },
        functionDesc: "Sets the Background's alpha to a specific value")]
    public class SetBgFunction : ANSLFunction
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
                manager.GetGUIManager().GetComponent<GUI.Fade>("fadeBg", out GUI.Fade currentFade))
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

