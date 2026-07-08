using ANF.Scene;
using Leguar.TotalJSON;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Camera Position Function can be used to move the camera
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setCameraRotation",
        functionAutoComplete: new string[] {
            "setCameraRotation(X;Y;Z)",
            "setCameraRotation(X;Y;Z;Duration;WaitForEnd)",
            "setCameraRotation(Marker)",
            "setCameraRotation(Marker;Duration;WaitForEnd)",
            "setCameraRotation(default)",
            "setCameraRotation(default;Duration;WaitForEnd)"
        },
        functionDesc: "Rotates the camera")]
    public class SetCameraRotationFunction : ANSLFunction
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
                Vector3 rotation = Vector3.zero;
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
                        rotation = new Vector3(x, y, z);
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
                            rotation = cameraController.GetDefaultRotation();
                        }
                        else
                        {
                            Background currentBackground = backgroundManager.GetBackground();

                            if (currentBackground != null && marker != null)
                            {
                                rotation = currentBackground.GetMarkerRotation(marker);
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

                cameraController.SetRotation(rotation, immediate, duration);
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
                if (!cameraController.Rotating)
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

