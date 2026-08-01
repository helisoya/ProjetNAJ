using ANF.Scene;
using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Remove All Statics Function can be used to remove a static object to the scene
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "removeAllStatics",
        functionAutoComplete: new string[] {
            "removeAllStatics()",
            "removeAllStatics(RemoveEverything)",
        },
        functionDesc: "Removes all statics object")]
    public class RemoveAllStaticFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{},
                new FunctionParameterType[]{FunctionParameterType.BOOL}
            };
        }

        protected override void OnStartProcess()
        {

            if (manager.GetWorld().GetComponent<StaticObjectManager>(out StaticObjectManager staticObjectManager))
            {
                if (parameters.GetTemplateId() == 1 &&
                    parameters.GetParameter(0, out bool deleteEverything))
                    staticObjectManager.RemoveAllSceneObjects(!deleteEverything);
                else
                    staticObjectManager.RemoveAllSceneObjects();
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

