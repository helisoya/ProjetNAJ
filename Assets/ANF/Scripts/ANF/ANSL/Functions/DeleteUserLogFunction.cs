using ANF.GUI;
using ANF.Persistent;


namespace ANF.ANSL
{
    /// <summary>
    /// The Delete User Log function can be used to delete a local log
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "deleteUserLog",
        functionAutoComplete: new string[] {
            "deleteUserLog(Log)"
            },
        functionDesc: "Deletes a User Log")]
    public class DeleteUserLogFunction : ANSLFunction
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
                logsContainer.RemoveUserLog(logId);
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

