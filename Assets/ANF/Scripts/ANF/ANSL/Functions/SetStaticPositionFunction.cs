using ANF.Scene;
using Leguar.TotalJSON;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Static Position Function can be used to move a static object
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setStaticPosition",
        functionAutoComplete: new string[] {
            "setStaticPosition(Name;X;Y;Z)",
            "setStaticPosition(Name;X;Y;Z;Duration;WaitForEnd)",
            "setStaticPosition(Name;Marker)",
            "setStaticPosition(Name;Marker;Duration;WaitForEnd)"
        },
        functionDesc: "Moves a static object")]
    public class SetStaticPositionFunction : ANSLFunction
    {
        private bool waitingForObject = false;
        private string currentObjectName;
        private StaticObject currentObject;

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
                manager.GetWorld().GetComponent<StaticObjectManager>(out StaticObjectManager staticObjectManager))
            {
                if (staticObjectManager.GetSceneObject(currentObjectName, out currentObject))
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
                        if (manager.GetWorld().GetComponent<ANF.Scene.BackgroundManager>(out ANF.Scene.BackgroundManager backgroundManager) &&
                            parameters.GetParameter(1, out string marker))
                        {
                            ANF.Scene.Background currentBackground = backgroundManager.GetBackground();

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

                    currentObject.SetPosition(position, immediate, duration);
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
                if (!manager.GetWorld().GetComponent<StaticObjectManager>(out StaticObjectManager staticObjectManager))
                    return;
                if (!staticObjectManager.GetSceneObject(currentObjectName, out currentObject))
                    return;
            }

            if (currentObject != null && waitingForObject)
            {
                if (!currentObject.Moving)
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

