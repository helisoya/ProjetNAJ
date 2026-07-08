using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Remove Character Function can be used to remove a character from the scene
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "removeCharacter",
        functionAutoComplete: new string[] {
            "removeCharacter(Name)"
        },
        functionDesc: "Removes a character")]
    public class RemoveCharacterFunction : ANSLFunction
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
                manager.GetWorld().GetComponent<ANF.Scene.CharacterManager>(out ANF.Scene.CharacterManager characterManager))
            {
                if (!characterManager.RemoveSceneObject(name))
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

