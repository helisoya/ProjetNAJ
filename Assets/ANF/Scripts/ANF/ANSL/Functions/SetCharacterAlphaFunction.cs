using Leguar.TotalJSON;
using ANF.Scene;

namespace ANF.ANSL
{
    /// <summary>
    /// The Set Character Alpha Function can be used to fade a character
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setCharacterAlpha",
        functionAutoComplete: new string[] {
            "setCharacterAlpha(Name;Alpha)",
            "setCharacterAlpha(Name;Alpha;Duration;WaitForEnd)"
        },
        functionDesc: "Fades a character")]
    public class SetCharacterAlphaFunction : ANSLFunction
    {
        private bool waitingForObject = false;
        private string currentObjectName;
        private Character currentObject;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.FLOAT },
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.FLOAT,
                    FunctionParameterType.FLOAT, FunctionParameterType.BOOL},
            };
        }

        protected override void OnStartProcess()
        {
            bool endProcess = true;
            if (parameters.GetParameter(0, out currentObjectName) &&
                parameters.GetParameter(1, out float alpha) &&
                manager.GetWorld().GetComponent<CharacterManager>(out CharacterManager characterManager))
            {
                if (characterManager.GetSceneObject(currentObjectName, out currentObject))
                {
                    float duration = 1.0f;
                    bool waitForEnd = false;
                    bool immediate = parameters.GetTemplateId() == 0;

                    if (parameters.GetTemplateId() == 1)
                    {
                        if (parameters.GetTemplateId() == 1)
                        {
                            if (!parameters.GetParameter(3, out waitForEnd))
                                waitForEnd = false;

                            if (!parameters.GetParameter(2, out duration))
                                duration = 1.0f;
                        }
                    }

                    currentObject.SetAlpha(alpha, immediate, duration);
                    waitingForObject = !immediate && waitForEnd;
                    endProcess = !waitingForObject;
                }
            }

            if (endProcess)
                EndProcess();
        }

        protected override void OnUpdate()
        {
            if (currentObject == null)
            {
                if (!manager.GetWorld().GetComponent<CharacterManager>(out CharacterManager characterManager))
                    return;
                if (!characterManager.GetSceneObject(currentObjectName, out currentObject))
                    return;
            }

            if (currentObject != null && waitingForObject)
            {
                if (!currentObject.Fading)
                {
                    waitingForObject = false;
                    currentObjectName = null;
                    currentObject = null;
                    EndProcess();
                }
            }
        }

        protected override void OnCleanup()
        {
            currentObject = null;
        }

        protected override void OnSave(JSON json)
        {
            json.Add("waitingForObject", waitingForObject);
            json.Add("currentObjectName", currentObjectName);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitingForObject"))
                waitingForObject = json.GetBool("waitingForObject");

            if (json.ContainsKey("currentObjectName"))
                currentObjectName = json.GetString("currentObjectName");
        }
    }
}

