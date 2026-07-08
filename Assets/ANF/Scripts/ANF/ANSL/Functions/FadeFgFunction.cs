using Leguar.TotalJSON;
using ANF.GUI;

namespace ANF.ANSL
{
    /// <summary>
    /// The Fade Fg function can be used to fade the foreground
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "fadeFg",
        functionAutoComplete: new string[] {
            "fadeFg(Alpha;WaitForEnd)",
            "fadeFg(Alpha;WaitForEnd;Duration)" },
        functionDesc: "Fades the Foreground to a specific value")]
    public class FadeFgFunction : ANSLFunction
    {
        private bool waitingForFading;
        private Fade currentFade;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.FLOAT, FunctionParameterType.BOOL},
                new FunctionParameterType[]{FunctionParameterType.FLOAT, FunctionParameterType.BOOL, FunctionParameterType.FLOAT},
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out float target) &&
                parameters.GetParameter(1, out waitingForFading))
            {
                float duration;

                if (!parameters.GetParameter(2, out duration))
                    duration = 1.0f;

                manager.GetGUIManager().GetComponent<Fade>("fadeFg", out currentFade);

                if (currentFade != null)
                    currentFade.FadeAlphaTo(target, false, duration);

                if (!waitingForFading || currentFade == null)
                    EndProcess();
            }
        }

        protected override void OnUpdate()
        {
            if (currentFade == null)
                manager.GetGUIManager().GetComponent<Fade>("fadeFg", out currentFade);

            if (currentFade != null && !currentFade.fadingAlpha)
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

