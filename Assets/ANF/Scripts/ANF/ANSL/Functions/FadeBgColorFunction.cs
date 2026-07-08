using Leguar.TotalJSON;
using UnityEngine;
using ANF.GUI;


namespace ANF.ANSL
{
    /// <summary>
    /// The Fade Color Bg function can be used to change the background's fade color over time
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "fadeBgColor",
        functionAutoComplete: new string[] {
            "fadeBgColor(R;G;B;A;WaitForEnd)",
            "fadeBgColor(R;G;B;A;WaitForEnd;Duration)"
        },
        functionDesc: "Changes the color of the background fade over time")]
    public class FadeBgColorFunction : ANSLFunction
    {
        private bool waitingForFading;
        private Fade currentFade;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.FLOAT, FunctionParameterType.FLOAT, FunctionParameterType.FLOAT,
                    FunctionParameterType.FLOAT, FunctionParameterType.BOOL},
                new FunctionParameterType[]{FunctionParameterType.FLOAT, FunctionParameterType.FLOAT, FunctionParameterType.FLOAT,
                    FunctionParameterType.FLOAT, FunctionParameterType.BOOL, FunctionParameterType.FLOAT }
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out float r) &&
                parameters.GetParameter(1, out float g) &&
                parameters.GetParameter(2, out float b) &&
                parameters.GetParameter(3, out float a) &&
                parameters.GetParameter(4, out waitingForFading) &&
                manager.GetGUIManager().GetComponent<Fade>("fadeBg", out currentFade))
            {
                float duration;
                if (!parameters.GetParameter(5, out duration))
                    duration = 1.0f;

                currentFade.FadeColorTo(new Color(r, g, b, a), false, duration);
            }

            if (!waitingForFading || !currentFade)
                EndProcess();
        }

        protected override void OnUpdate()
        {
            if (currentFade == null)
                manager.GetGUIManager().GetComponent<Fade>("fadeBg", out currentFade);

            if (currentFade != null && !currentFade.fadingColor)
            {
                waitingForFading = false;
                EndProcess();
            }
        }

        protected override void OnCleanup()
        {
            currentFade = null;
        }

        protected override void OnSave(JSON json)
        {
            json.Add("waitingForFading", waitingForFading);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitingForFading"))
                waitingForFading = json.GetBool("waitingForFading");
        }
    }
}

