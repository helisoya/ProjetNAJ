using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Color Bg function can be used to change the background's fade color
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setBgColor",
        functionAutoComplete: new string[] { "setBgColor(R;G;B;A)" },
        functionDesc: "Changes the color of the background fade immediately")]
    public class SetBgColorFunction : ANSLFunction
    {

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.FLOAT, FunctionParameterType.FLOAT, FunctionParameterType.FLOAT, FunctionParameterType.FLOAT}
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out float r) &&
                parameters.GetParameter(1, out float g) &&
                parameters.GetParameter(2, out float b) &&
                parameters.GetParameter(3, out float a) &&
                manager.GetGUIManager().GetComponent<GUI.Fade>("fadeBg", out GUI.Fade currentFade))
            {
                currentFade.FadeColorTo(new Color(r, g, b, a), true);
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

