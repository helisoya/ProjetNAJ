using ANF.Persistent;
using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Add Variable Function can be used to create a new variable locally
    /// </summary>
    [ANSLFunctionAttribute(
        functionBody: "createUserVariable",
        functionAutoComplete: new string[] {
            "createUserVariable(Name;Value)",
            "createUserVariable(Name;VariableToCopy)"
        },
        functionDesc: "Creates a user variable")]
    public class CreateUserVariableFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.INT },
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING }
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string name) &&
                PersistentDataManager.instance.GetPlayerData().GetComponent(out PlayerVariableContainer container))
            {
                if (parameters.GetTemplateId() == 0 &&
                    parameters.GetParameter(1, out int value))
                    container.AddVariable(name, value);
                else if (parameters.GetParameter(1, out string otherVariable)
                    && container.GetVariable(otherVariable, out int otherValue))
                    container.AddVariable(name, otherValue);
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

