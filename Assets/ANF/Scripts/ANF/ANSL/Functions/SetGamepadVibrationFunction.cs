using ANF.GUI;
using ANF.Scene;
using Leguar.TotalJSON;
using UnityEngine.InputSystem;
using UnityEngine.TextCore;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Gamepad Vibration Function can be used to vibrate the gamepad
    /// ! Be wary that this function does not set a duration to the vibration !
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setGamepadVibration",
        functionAutoComplete: new string[] {
            "setGamepadVibration(Both)",
            "setGamepadVibration(Left;Right)",
        },
        functionDesc: "Sets the gamepad's vibration")]
    public class SetGamepadVibrationFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.FLOAT },
                new FunctionParameterType[]{FunctionParameterType.FLOAT, FunctionParameterType.FLOAT },
            };
        }

        protected override void OnStartProcess()
        {
            if (Gamepad.current != null)
            {
                if (parameters.GetParameter(0, out float left))
                {
                    if (parameters.GetTemplateId() == 1 &&
                        parameters.GetParameter(1, out float right))
                        Gamepad.current.SetMotorSpeeds(left, right);
                    else
                        Gamepad.current.SetMotorSpeeds(left, left);
                }
            }


            EndProcess();
        }

        protected override void OnUpdate()
        {

        }

        protected override void OnCleanup()
        {

        }

        protected override void OnSave(JSON json)
        {

        }

        protected override void OnLoad(JSON json)
        {

        }
    }
}

