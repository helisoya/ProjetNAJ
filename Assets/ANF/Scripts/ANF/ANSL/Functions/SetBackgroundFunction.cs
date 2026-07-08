using Leguar.TotalJSON;
using ANF.Scene;

namespace ANF.ANSL
{
    /// <summary>
    /// The Set Background function can be used to change the current background
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setBackground",
        functionAutoComplete: new string[] {
            "setBackground(Background)",
            "setBackground(Background;UseDefaultData)"
        },
        functionDesc: "Changes the current background. Background can be null")]
    public class SetBackgroundFunction : ANSLFunction
    {
        private bool waitingForLoading = false;
        private BackgroundManager backgroundManager;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING },
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.BOOL }
            };
        }

        protected override void OnStartProcess()
        {
            waitingForLoading = false;
            if (parameters.GetParameter(0, out string background) &&
                manager.GetWorld().GetComponent<BackgroundManager>(out backgroundManager))
            {
                bool useDefaultData = false;

                if (parameters.GetTemplateId() == 1)
                    if (!parameters.GetParameter(1, out useDefaultData))
                        useDefaultData = false;

                backgroundManager.SetBackground(background, useDefaultData);

                if (!backgroundManager.unloadingBackground && !backgroundManager.loadingBackground)
                    EndProcess();
                else waitingForLoading = true;
            }
            else
            {
                EndProcess();
            }

        }

        protected override void OnUpdate()
        {
            if (backgroundManager == null)
                manager.GetWorld().GetComponent<BackgroundManager>(out backgroundManager);

            if (backgroundManager != null && (backgroundManager.loadingBackground || backgroundManager.loadingBackground))
                return;

            waitingForLoading = false;

            EndProcess();
        }

        protected override void OnCleanup()
        {
            backgroundManager = null;
        }

        protected override void OnSave(JSON json)
        {
            json.Add("waitingForLoading", waitingForLoading);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitingForLoading"))
                waitingForLoading = json.GetBool("waitingForLoading");
            else
                waitingForLoading = false;
        }
    }
}

