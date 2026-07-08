using ANF.Scene;
using Leguar.TotalJSON;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Camera Position Function can be used to move the camera
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setCameraPosition",
        functionAutoComplete: new string[] {
            "setCameraPosition(X;Y;Z)",
            "setCameraPosition(X;Y;Z;Duration;WaitForEnd)",
            "setCameraPosition(Marker)",
            "setCameraPosition(Marker;Duration;WaitForEnd)",
            "setCameraPosition(default)",
            "setCameraPosition(default;Duration;WaitForEnd)"
        },
        functionDesc: "Moves the camera")]
    public class SetCameraPositionFunction : ANSLFunction
    {
        private bool waitingForObject = false;
        private MainCameraController cameraController;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{ FunctionParameterType.FLOAT, FunctionParameterType.FLOAT, FunctionParameterType.FLOAT },
                new FunctionParameterType[]{ FunctionParameterType.FLOAT, FunctionParameterType.FLOAT, FunctionParameterType.FLOAT,
                    FunctionParameterType.FLOAT, FunctionParameterType.BOOL },
                new FunctionParameterType[]{FunctionParameterType.STRING },
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.FLOAT, FunctionParameterType.BOOL},
            };
        }

        protected override void OnStartProcess()
        {
            bool endProcess = true;
            if (manager.GetWorld().GetComponent<MainCameraController>(out cameraController))
            {
                Vector3 position = Vector3.zero;
                float duration = 1.0f;
                bool waitForEnd = false;
                bool immediate = parameters.GetTemplateId() == 0 || parameters.GetTemplateId() == 2;

                if (parameters.GetTemplateId() <= 1)
                {
                    // Explicit
                    if (parameters.GetParameter(0, out float x) &&
                        parameters.GetParameter(1, out float y) &&
                        parameters.GetParameter(2, out float z))
                    {
                        position = new Vector3(x, y, z);
                    }

                    if (parameters.GetTemplateId() == 1)
                    {
                        if (!parameters.GetParameter(4, out waitForEnd))
                            waitForEnd = false;

                        if (!parameters.GetParameter(3, out duration))
                            duration = 1.0f;
                    }
                }
                else
                {
                    // Marker
                    if (manager.GetWorld().GetComponent<BackgroundManager>(out BackgroundManager backgroundManager) &&
                        parameters.GetParameter(0, out string marker))
                    {
                        if (marker.Equals("default"))
                        {
                            position = cameraController.GetDefaultPosition();
                        }
                        else
                        {
                            Background currentBackground = backgroundManager.GetBackground();

                            if (currentBackground != null && marker != null)
                            {
                                position = currentBackground.GetMarkerPosition(marker);
                            }
                        }

                        if (parameters.GetTemplateId() == 3)
                        {
                            if (!parameters.GetParameter(2, out waitForEnd))
                                waitForEnd = false;

                            if (!parameters.GetParameter(1, out duration))
                                duration = 1.0f;
                        }
                    }
                }

                cameraController.SetPosition(position, immediate, duration);
                waitingForObject = !immediate && waitForEnd;
                endProcess = !waitingForObject;

            }

            if (endProcess)
                EndProcess();
        }

        protected override void OnUpdate()
        {
            if (cameraController == null)
            {
                if (!manager.GetWorld().GetComponent<MainCameraController>(out cameraController))
                    return;
            }

            if (cameraController != null && waitingForObject)
            {
                if (!cameraController.Moving)
                {
                    waitingForObject = false;
                    cameraController = null;
                    EndProcess();
                }
            }
        }

        protected override void OnCleanup()
        {
            cameraController = null;
        }

        protected override void OnSave(JSON json)
        {
            json.Add("waitingForObject", waitingForObject);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitingForObject"))
                waitingForObject = json.GetBool("waitingForObject");
        }
    }
}

