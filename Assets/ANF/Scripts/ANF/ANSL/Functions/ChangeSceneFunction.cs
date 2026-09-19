using ANF.Scene;
using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Change Scene Function can be used to change the current scene
    /// </summary>
    [ANSLFunctionAttribute(
        functionBody: "changeScene",
        functionAutoComplete: new string[] {
            "changeScene(Name)"
        },
        functionDesc: "Changes the current scene")]
    public class ChangeSceneFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING }
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string name))
            {
                context.ClearStack();
                manager.ChangeScene(name);
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

