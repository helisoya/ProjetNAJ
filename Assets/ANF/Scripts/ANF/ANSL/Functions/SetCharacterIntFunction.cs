using Leguar.TotalJSON;
using ANF.Scene;

namespace ANF.ANSL
{
    /// <summary>
    /// The Set Character Int Function can be used to set a character's animator int
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setCharacterInt",
        functionAutoComplete: new string[] {
            "setCharacterInt(Name;ParameterName;Value)"
        },
        functionDesc: "Sets a character's animator int")]
    public class SetCharacterIntFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.INT }
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string characterName) &&
                parameters.GetParameter(1, out string parameterName) &&
                parameters.GetParameter(2, out int value) &&
                manager.GetWorld().GetComponent<CharacterManager>(out CharacterManager characterManager))
            {
                if (characterManager.GetSceneObject(characterName, out ANF.Scene.Character character))
                {
                    character.SetInteger(parameterName,value);
                }
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

        protected override void OnSave(JSON json)
        {
            // Unused
        }

        protected override void OnLoad(JSON json)
        {
            // Unused
        }
    }
}

