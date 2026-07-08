using ANF.Persistent;


namespace ANF.ANSL
{
    /// <summary>
    /// The variable function changes a variable's value
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "variable",
        functionAutoComplete: new string[] { "variable(Name;Operator;Value)", "variable(Name;Operator;Variable)", "variable(Name;default)" },
        functionDesc: "Changes a variable's value. Operator can be +, -, and default")]
    public class VariableFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.INT },
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.STRING },
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING },
            };
        }

        protected override void OnStartProcess()
        {
            if (PersistentDataManager.instance.GetPlayerData().GetComponent(out PlayerVariableContainer container))
            {
                if (parameters.GetParameter(0, out string name) && parameters.GetParameter(1, out string op) &&
                container.GetVariable(name, out int currentValue))
                {
                    if (op.Equals("default"))
                    {
                        container.ResetVariable(name);
                    }
                    else
                    {
                        int otherValue = 0;

                        if (parameters.GetTemplateId() == 0)
                            parameters.GetParameter(2, out otherValue);
                        else
                            container.GetVariable(name, out otherValue);

                        switch (op)
                        {
                            case "=":
                                currentValue = otherValue;
                                break;
                            case "+":
                                currentValue += otherValue;
                                break;
                            case "-":
                                currentValue -= otherValue;
                                break;
                        }
                        container.SetVariable(name, currentValue);
                    }
                }
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

