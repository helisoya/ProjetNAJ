using Leguar.TotalJSON;
using ANF.Scene;

namespace ANF.ANSL
{
    /// <summary>
    /// The Set Static Hidden Function can be used to disable static object's renderers
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "setStaticHidden",
        functionAutoComplete: new string[] {
            "setStaticHidden(Name;Hidden)"
        },
        functionDesc: "Sets if a static object is hidden or not")]
    public class SetStaticHiddenFunction : ANSLFunction
    {

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.BOOL }
            };
        }

        protected override void OnStartProcess()
        {
            bool endProcess = true;
            if (parameters.GetParameter(0, out string currentObjectName) &&
                parameters.GetParameter(1, out bool hidden) &&
                manager.GetWorld().GetComponent(out StaticObjectManager staticObjectManager))
            {
                if (staticObjectManager.GetSceneObject(currentObjectName, out StaticObject currentObject))
                {
                    currentObject.SetHidden(hidden);
                }
            }

            if (endProcess)
                EndProcess();
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnCleanup()
        {
        }

        protected override void OnSave(JSON json)
        {
        }

        protected override void OnLoad(JSON json)
        {
        }
    }
}

