using ANF.Utils;
using System.Collections.Generic;

namespace ANF.ANSL
{
    /// <summary>
    /// The if function is used to check variables.
    /// If the check is successful, the true functions will be loaded. If false, the else statement will be loaded
    /// </summary>
    [ANSLFunctionAttribute(
        functionBody: "block",
        functionAutoComplete: new string[] { "block\\n\\nendblock" },
        functionDesc: "All functions inside a block will be processed during the same frame, ignoring any depth check.")]
    public class BlockFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.BOOL }
            };
        }

        public override bool Compile(out List<string> compiledLines, string cleanedLine, uint id, ANSLCompiler compiler, List<ANSLUtils.ANSLError> errors, int outputLine)
        {
            compiledLines = new List<string>();

            if (cleanedLine.Length != "block".Length)
            {
                // Unknown character
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"Unknown character at the end of the line : {cleanedLine}."
                });
                return false;
            }

            List<string> compiledBetween = new List<string>();
            bool canContinue = true;
            bool foundEnd = false;
            compiler.CheckNextLine();
            string currentNextLine = compiler.GetCurrentLineClean();

            while (canContinue && currentNextLine != null)
            {
                if (string.IsNullOrEmpty(currentNextLine) || string.IsNullOrWhiteSpace(currentNextLine))
                {
                    compiler.CheckNextLine();
                    currentNextLine = compiler.GetCurrentLineClean();
                    continue;
                }

                if (currentNextLine.StartsWith("endblock"))
                {
                    if (currentNextLine.Length != "endblock".Length)
                    {
                        // Unknown character
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Unknown character at the end of the line : {currentNextLine}."
                        });
                        return false;
                    }

                    foundEnd = true;
                    canContinue = false;
                }
                else
                {
                    int outputLineForFunction = outputLine + compiledBetween.Count + 1;

                    // Potential function
                    if (compiler.CompileLine(currentNextLine, out List<string> compiled, outputLineForFunction))
                        compiledBetween.AddRange(compiled);
                    else
                        return false;
                }

                if (canContinue)
                {
                    compiler.CheckNextLine();
                    currentNextLine = compiler.GetCurrentLineClean();
                }
            }

            if (!foundEnd)
            {
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"endblock not found : {currentNextLine}."
                });
                return false;
            }

            compiledLines.Add($"{id}|false");
            compiledLines.AddRange(compiledBetween);
            compiledLines.Add($"{id}|true");

            return true;
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out bool enabled))
                context.SetDepthCheckEnabled(enabled);
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