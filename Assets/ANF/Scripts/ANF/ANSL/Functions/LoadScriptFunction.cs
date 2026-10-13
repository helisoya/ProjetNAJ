using ANF.Utils;

namespace ANF.ANSL
{
    /// <summary>
    /// The load script function is used to change the current script of the context.
    /// If the change is immediate, the script will be loaded instantly. If not, it will wait for the next Update
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "load",
        functionAutoComplete: new string[] { "load(Script)", "load(Script;StartIndex)", "load(Script;StartIndex;ContextId)" },
        functionDesc: "Loads a new ANSL script")]
    public class LoadScriptFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING},
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.UINT},
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.UINT, FunctionParameterType.UINT}
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
                        startLine = 0;
                    else if (!parameters.GetParameter(1, out startLine))
                        startLine = 0;

                    if (parameters.GetTemplateId() == 2 &&
                        parameters.GetParameter(2, out uint id) &&
                        manager.GetWorld().GetComponent(out ANSLManager anslManager))
                    {
                        ANSLContext context = anslManager.GetContext(id);
                        if (context != null)
                        {
                            context.LoadScript(resolvedScript, startLine);
                        }
                    }

                    else
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

