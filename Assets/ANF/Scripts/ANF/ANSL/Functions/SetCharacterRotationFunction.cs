using Leguar.TotalJSON;
using UnityEngine;
using ANF.Scene;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Character rotation Function can be used to rotate a character
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setCharacterRotation",
        functionAutoComplete: new string[] {
            "setCharacterRotation(Name;X;Y;Z)",
            "setCharacterRotation(Name;X;Y;Z;Duration;WaitForEnd)",
            "setCharacterRotation(Name;Marker)",
            "setCharacterRotation(Name;Marker;Duration;WaitForEnd)"
        },
        functionDesc: "Rotates a character")]
    public class SetCharacterRotationFunction : ANSLFunction
    {
        private bool waitingForObject = false;
        private string currentObjectName;
        private Character currentObject;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING,
                    FunctionParameterType.FLOAT, FunctionParameterType.FLOAT, FunctionParameterType.FLOAT },
                new FunctionParameterType[]{FunctionParameterType.STRING,
                    FunctionParameterType.FLOAT, FunctionParameterType.FLOAT, FunctionParameterType.FLOAT,
                    FunctionParameterType.FLOAT, FunctionParameterType.BOOL },
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING },
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING,
                    FunctionParameterType.FLOAT, FunctionParameterType.BOOL},
            };
        }

        protected override void OnStartProcess()
        {
            bool endProcess = true;
            if (parameters.GetParameter(0, out currentObjectName) &&
                manager.GetWorld().GetComponent<CharacterManager>(out CharacterManager characterManager))
            {
                if (characterManager.GetSceneObject(currentObjectName, out currentObject))
                {
                    Vector3 position = Vector3.zero;
                    float duration = 1.0f;
                    bool waitForEnd = false;
                    bool immediate = parameters.GetTemplateId() == 0 || parameters.GetTemplateId() == 2;

                    if (parameters.GetTemplateId() <= 1)
                    {
                        // Explicit
                        if (parameters.GetParameter(1, out float x) &&
                            parameters.GetParameter(2, out float y) &&
                            parameters.GetParameter(3, out float z))
                        {
                            position = new Vector3(x, y, z);
                        }

                        if (parameters.GetTemplateId() == 1)
                        {
                            if (!parameters.GetParameter(5, out waitForEnd))
                                waitForEnd = false;

                            if (!parameters.GetParameter(4, out duration))
                                duration = 1.0f;
                        }
                    }
                    else
                    {
                        // Marker
                        if (manager.GetWorld().GetComponent<BackgroundManager>(out BackgroundManager backgroundManager) &&
                            parameters.GetParameter(1, out string marker))
                        {
                            Background currentBackground = backgroundManager.GetBackground();

                            if (currentBackground != null && marker != null)
                            {
                                position = currentBackground.GetMarkerPosition(marker);
                            }

                            if (parameters.GetTemplateId() == 3)
                            {
                                if (!parameters.GetParameter(3, out waitForEnd))
                                    waitForEnd = false;

                                if (!parameters.GetParameter(2, out duration))
                                    duration = 1.0f;
                            }
                        }
                    }

                    currentObject.SetRotation(position, immediate, duration);
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
                if (!currentObject.Rotating)
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

