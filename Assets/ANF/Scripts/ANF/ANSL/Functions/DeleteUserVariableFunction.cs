using ANF.Persistent;
using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Remove Variable Function can be used to create a new variable locally
    /// </summary>
    [ANSLFunctionAttribute(
        functionBody: "deleteUserVariable",
        functionAutoComplete: new string[] {
            "deleteUserVariable(Name)",
            "deleteUserVariable(Name)"
        },
        functionDesc: "Removes a user variable")]
    public class DeleteUserVariableFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING },
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string name) &&
                PersistentDataManager.instance.GetPlayerData().GetComponent(out PlayerVariableContainer container))
            {
                container.RemoveVariable(name);
            }

            EndProcess();
        }

        protected override void OnUpdate()
        {

        }

        protected override void OnCleanup()
        {
            // Unused
        }

        protected override void OnSave(JSON json)
        {

        }

        protected override void OnLoad(JSON json)
        {

        }
    }
}

