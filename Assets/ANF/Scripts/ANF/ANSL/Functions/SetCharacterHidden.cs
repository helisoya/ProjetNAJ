using Leguar.TotalJSON;
using ANF.Scene;

namespace ANF.ANSL
{
    /// <summary>
    /// The Set Character Hidden Function can be used to disable character's renderers
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "setCharacterHidden",
        functionAutoComplete: new string[] {
            "setCharacterHidden(Name;Hidden)"
        },
        functionDesc: "Sets if a character is hidden or not")]
    public class SetCharacterHiddenFunction : ANSLFunction
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
                manager.GetWorld().GetComponent<CharacterManager>(out CharacterManager characterManager))
            {
                if (characterManager.GetSceneObject(currentObjectName, out Character currentObject))
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

