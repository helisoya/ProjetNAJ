using ANF.GUI;
using ANF.Persistent;


namespace ANF.ANSL
{
    /// <summary>
    /// The Create User Log function can be used to create a new local log
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "createUserLog",
        functionAutoComplete: new string[] {
            "createUserLog(Log)"
            },
        functionDesc: "Creates a User Log")]
    public class CreateUserLogFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING}
            };
        }

        protected override void OnStartProcess()
        {
            if (PersistentDataManager.instance.GetPlayerData().GetComponent<LogsContainer>(out LogsContainer logsContainer) &&
                parameters.GetParameter(0, out string logId))
            {
                logsContainer.CreateUserLog(logId);
            }

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

