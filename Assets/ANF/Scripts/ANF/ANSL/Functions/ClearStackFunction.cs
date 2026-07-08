namespace ANF.ANSL
{
    /// <summary>
    /// The jumps to function is used to advance the line counter manually.
    /// It is only used internaly, and may not be used in ANSL scripts directly
    /// </summary>
    [ANSLFunctionAttribute(
        functionBody: "clearStack",
        functionAutoComplete: new string[] { "clearStack()", "clearStack(ContextIndex)" },
        functionDesc: "Clears the context's stack. Can also clear other context's")]
    public class ClearStack : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{},
                new FunctionParameterType[]{FunctionParameterType.UINT}
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetTemplateId() == 0)
                context.ClearStack();
            else if (parameters.GetParameter(0, out uint contextId))
            {
                if (manager.GetWorld().GetComponent(out ANSLManager anslManager))
                {
                    ANSLContext otherContext = anslManager.GetContext(contextId);
                    if (otherContext != null && otherContext.isRunning)
                        otherContext.ClearStack();
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

