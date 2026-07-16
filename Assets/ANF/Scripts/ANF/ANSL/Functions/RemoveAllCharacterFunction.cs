using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Remove All Characters Function can be used to remove all characters from the scene
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "removeAllCharacters",
        functionAutoComplete: new string[] {
            "removeAllCharacters()"
        },
        functionDesc: "Removes all characters")]
    public class RemoveAllCharacterFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{}
            };
        }

        protected override void OnStartProcess()
        {

            if (manager.GetWorld().GetComponent<ANF.Scene.CharacterManager>(out ANF.Scene.CharacterManager characterManager))
            {
                characterManager.RemoveAllSceneObjects();
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

