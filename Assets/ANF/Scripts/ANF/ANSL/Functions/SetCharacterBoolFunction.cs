using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Character Bool Function can be used to set a character's animator bool
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setCharacterBool",
        functionAutoComplete: new string[] {
            "setCharacterBool(Name;ParameterName;Value)"
        },
        functionDesc: "Sets a character's animator bool")]
    public class SetCharacterBoolFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.BOOL }
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string characterName) &&
                parameters.GetParameter(1, out string parameterName) &&
                parameters.GetParameter(2, out bool value) &&
                manager.GetWorld().GetComponent<ANF.Scene.CharacterManager>(out ANF.Scene.CharacterManager characterManager))
            {
                if (characterManager.GetSceneObject(characterName, out ANF.Scene.Character character))
                {
                    character.SetBool(parameterName,value);
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

