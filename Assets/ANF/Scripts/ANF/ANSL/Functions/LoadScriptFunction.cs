using ANF.Utils;

namespace ANF.ANSL
{
    /// <summary>
    /// The load script function is used to change the current script of the context.
    /// If the change is immediate, the script will be loaded instantly. If not, it will wait for the next Update
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "load",
        functionAutoComplete: new string[] { "load(Script)", "load(Script;StartIndex)" },
        functionDesc: "Jump to is an internal function. It cannot be called from anywhere")]
    public class LoadScriptFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING},
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.UINT}
            };
        }

        protected override void OnStartProcess()
        {
            EndProcess(); // Flags the function as ended to avoid potential problems if the script is loaded instantly

            if (parameters.GetParameter(0, out string scriptFile))
            {
                string resolvedScript = ANSLUtils.ResolveFilePath(context.GetCurrentFilepath(), scriptFile);

                if (resolvedScript != null)
                {
                    uint startLine;
                    if (parameters.GetTemplateId() == 0)
                        context.LoadScript(resolvedScript, 0);
                    else if (parameters.GetTemplateId() == 1 &&
                    parameters.GetParameter(1, out startLine))
                        context.LoadScript(resolvedScript, startLine);
                }
            }

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

