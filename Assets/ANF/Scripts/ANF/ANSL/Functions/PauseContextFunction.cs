namespace ANF.ANSL
{
    /// <summary>
    /// The Pause Context function can be used to pause an ANSL Context
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "pauseContext",
        functionAutoComplete: new string[] { "pauseContext(Id)" },
        functionDesc: "Pauses an ANSL Context")]
    public class PauseContextFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.UINT, FunctionParameterType.BOOL}
            };
        }

        protected override void OnStartProcess()
        {
            if (manager.GetWorld().GetComponent<ANSLManager>(out ANSLManager anslManager) &&
                parameters.GetParameter(0, out uint contextId) &&
                parameters.GetParameter(1, out bool paused))
            {
                ANSLContext context = anslManager.GetContext(contextId);
                if (context != null)
                    context.PauseContext(paused);
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

