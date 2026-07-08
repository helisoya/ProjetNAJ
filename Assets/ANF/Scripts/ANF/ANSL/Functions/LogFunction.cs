using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The log function simply logs a message to the debug console. 
    /// Only used in the editor
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "log",
        functionAutoComplete: new string[] { "log(Type;Message)" },
        functionDesc: "Logs a message to the debug console. (Can be Log, Warning or Error)")]
    public class LogFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING}
            };
        }

        protected override void OnStartProcess()
        {
#if UNITY_EDITOR
            if (parameters.GetParameter(0, out string type) && parameters.GetParameter(1, out string message))
            {
                if (type.Equals("Log"))
                    Debug.Log(message);
                else if (type.Equals("Warning"))
                    Debug.LogWarning(message);
                else if (type.Equals("Error"))
                    Debug.LogError(message);
            }
#endif
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
    }
}

