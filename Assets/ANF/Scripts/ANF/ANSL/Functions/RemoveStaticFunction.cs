using ANF.Scene;
using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Remove Static Function can be used to remove a static object to the scene
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "removeStatic",
        functionAutoComplete: new string[] {
            "removeStatic(Name)"
        },
        functionDesc: "Removes a static object")]
    public class RemoveStaticFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING }
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string name) &&
                manager.GetWorld().GetComponent<StaticObjectManager>(out StaticObjectManager staticObjectManager))
            {
                if (!staticObjectManager.RemoveSceneObject(name))
                {
                    // Problem
                }
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

