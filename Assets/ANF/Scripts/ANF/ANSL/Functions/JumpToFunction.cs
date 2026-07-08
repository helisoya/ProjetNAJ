namespace ANF.ANSL
{
    /// <summary>
    /// The jumps to function is used to advance the line counter manually.
    /// It is only used internaly, and may not be used in ANSL scripts directly
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "",
        functionAutoComplete: null,
        functionDesc: "Jump to is an internal function. It cannot be called from anywhere")]
    public class JumpToFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.UINT}
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out uint lineCounter))
                context.SetLineCounter(lineCounter);

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

