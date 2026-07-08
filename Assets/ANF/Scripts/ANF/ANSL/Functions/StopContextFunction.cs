namespace ANF.ANSL
{
    /// <summary>
    /// The Stop Context can be used to stop an ANSL Context
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "stopContext",
        functionAutoComplete: new string[] { "stopContext(Id)" },
        functionDesc: "Stops an ANSL Context")]
    public class StopContextFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.UINT}
            };
        }

        protected override void OnStartProcess()
        {
            if (manager.GetWorld().GetComponent<ANSLManager>(out ANSLManager anslManager) &&
                parameters.GetParameter(0, out uint contextId))
            {
                ANSLContext context = anslManager.GetContext(contextId);
                if (context != null)
                    context.StopContext();
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

