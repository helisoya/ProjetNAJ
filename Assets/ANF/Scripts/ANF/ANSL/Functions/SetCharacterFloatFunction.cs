using ANF.GUI;
using ANF.Scene;
using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Character Float Function can be used to set a character's animator int
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setCharacterFloat",
        functionAutoComplete: new string[] {
            "setCharacterFloat(Name;ParameterName;Value)",
            "setCharacterFloat(Name;ParameterName;Value;TransitionDuration;WaitForEnd)"
        },
        functionDesc: "Sets a character's animator float")]
    public class SetCharacterFloatFunction : ANSLFunction
    {
        private bool waitingForEnd;
        private string characterName;
        private string parameterName;
        private Character currentCharacter;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.FLOAT },
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.FLOAT,
                    FunctionParameterType.FLOAT, FunctionParameterType.BOOL}
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out characterName) &&
                parameters.GetParameter(1, out parameterName) &&
                parameters.GetParameter(2, out float value) &&
                manager.GetWorld().GetComponent<CharacterManager>(out CharacterManager characterManager))
            {
                if (characterManager.GetSceneObject(characterName, out currentCharacter))
                {
                    bool immediate = parameters.GetTemplateId() == 0;
                    float transitionDuration = 0.0f;
                    if (!immediate && !parameters.GetParameter(3, out transitionDuration))
                        transitionDuration = 0.5f;

                    if (!immediate && !parameters.GetParameter(4, out waitingForEnd))
                        waitingForEnd = false;

                    currentCharacter.SetFloat(parameterName,value,immediate,transitionDuration);
                }
            }

            if(!waitingForEnd)
                EndProcess();
        }

        protected override void OnUpdate()
        {
            if (currentCharacter == null)
                if (manager.GetWorld().GetComponent<CharacterManager>(out CharacterManager characterManager))
                    characterManager.GetSceneObject(characterName,out currentCharacter);

            if (currentCharacter != null && !currentCharacter.IsParameterLerping(parameterName))
                EndProcess();
        }

        protected override void OnCleanup()
        {
            currentCharacter = null;
        }

        protected override void OnSave(JSON json)
        {
            json.Add("waitingForEnd", waitingForEnd);

            if (parameterName != null)
                json.Add("parameterName", parameterName);
            if (characterName != null)
                json.Add("characterName", characterName);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitingForEnd"))
                waitingForEnd = json.GetBool("waitingForEnd");
            else
                waitingForEnd = false;

            if (json.ContainsKey("parameterName"))
                parameterName = json.GetString("parameterName");
            if (json.ContainsKey("characterName"))
                characterName = json.GetString("characterName");
        }
    }
}

