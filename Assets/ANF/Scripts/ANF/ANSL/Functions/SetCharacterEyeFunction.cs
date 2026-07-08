using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Character eye Function can be used to change a character's eye animation
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setCharacterEye",
        functionAutoComplete: new string[] {
            "setCharacterEye(Name;Eye)",
            "setCharacterEye(Name;Eye;TransitionDuration)"
        },
        functionDesc: "Changes a character's eye animation")]
    public class SetCharacterEyeFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING },
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.FLOAT },
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string characterName) &&
                parameters.GetParameter(1, out string bodyName) &&
                manager.GetWorld().GetComponent<ANF.Scene.CharacterManager>(out ANF.Scene.CharacterManager characterManager))
            {
                if (characterManager.GetSceneObject(characterName, out ANF.Scene.Character character))
                {
                    if (parameters.GetTemplateId() == 1 && parameters.GetParameter(2, out float transitionDuration))
                        character.ChangeEyeAnimation(bodyName, false, transitionDuration);
                    else
                        character.ChangeEyeAnimation(bodyName, true);
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

