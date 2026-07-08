using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Character Trigger Function can be used to set a character's animator trigger
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setCharacterTrigger",
        functionAutoComplete: new string[] {
            "setCharacterTrigger(Name;Trigger)"
        },
        functionDesc: "Sets a character's animator trigger")]
    public class SetCharacterTriggerFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING }
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string characterName) &&
                parameters.GetParameter(1, out string triggerName) &&
                manager.GetWorld().GetComponent<ANF.Scene.CharacterManager>(out ANF.Scene.CharacterManager characterManager))
            {
                if (characterManager.GetSceneObject(characterName, out ANF.Scene.Character character))
                {
                    character.SetTrigger(triggerName);
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

