namespace ANF.ANSL
{
    /// <summary>
    /// The return function stops the current script
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "return",
        functionAutoComplete: new string[] { "return()" },
        functionDesc: "Stops the current script")]
    public class ReturnFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{}
            };
        }

        protected override void OnStartProcess()
        {
            context.SetLineCounter(uint.MaxValue);
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

