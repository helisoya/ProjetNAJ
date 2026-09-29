using ANF.Persistent;
using ANF.Utils;
using System.Collections.Generic;
using System.Diagnostics;

namespace ANF.ANSL
{
    /// <summary>
    /// The if function is used to check variables.
    /// If the check is successful, the true functions will be loaded. If false, the else statement will be loaded
    /// </summary>
    [ANSLFunctionAttribute(
        functionBody: "if",
        functionAutoComplete: new string[] { "if()\\n\\nelse\\n\\nendif" },
        functionDesc: "Checks variables. You can't use both | and & in the same check.")]
    public class IfFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{ FunctionParameterType.UINT, FunctionParameterType.UINT, FunctionParameterType.STRING}
            };
        }

        public override bool Compile(out List<string> compiledLines, string cleanedLine, uint id, ANSLCompiler compiler, List<ANSLUtils.ANSLError> errors, int outputLine)
        {
            compiledLines = new List<string>();

            string ifContent = ExtractParenthesisContent(cleanedLine);

            if (ifContent == null)
            {
                // No if content
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"No if content detected : {cleanedLine}."
                });
                return false;
            }

            if (!cleanedLine.EndsWith(')'))
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

            if (!CheckIfContent(ifContent, out bool _, true))
            {
                // Check failed
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"Could not parse if content : {cleanedLine}."
                });
                return false;
            }

            List<string> compiledTrue = new List<string>();
            List<string> compiledFalse = new List<string>();
            bool compilingTrues = true;
            bool canContinue = true;
            bool foundEnd = false;
            compiler.CheckNextLine();
            string currentNextLine = compiler.GetCurrentLineClean();

            while (canContinue && currentNextLine != "EOF")
            {
                if (string.IsNullOrEmpty(currentNextLine) || string.IsNullOrWhiteSpace(currentNextLine))
                {
                    compiler.CheckNextLine();
                    currentNextLine = compiler.GetCurrentLineClean();
                    continue;
                }

                if (currentNextLine.Equals("else"))
                {
                    if (!compilingTrues)
                    {
                        // Already compiling else
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Two else detected : {currentNextLine}."
                        });
                        return false;
                    }

                    if (currentNextLine.Length != "else".Length)
                    {
                        // Already compiling else
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Unexpected token after the else : {currentNextLine}."
                        });
                        return false;
                    }

                    compilingTrues = false;
                }
                else if (currentNextLine.Equals("endif"))
                {
                    if (currentNextLine.Length != "endif".Length)
                    {
                        // Already compiling else
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Unexpected token after the endif : {currentNextLine}."
                        });
                        return false;
                    }

                    foundEnd = true;
                    canContinue = false;
                }
                else
                {
                    int outputLineForFunction;
                    if (compilingTrues)
                        outputLineForFunction = outputLine + compiledTrue.Count + 1;
                    else
                        outputLineForFunction = outputLine + compiledTrue.Count + 1 + compiledFalse.Count + 1;

                    // Potential function
                    if (compiler.CompileLine(currentNextLine, out List<string> compiled, outputLineForFunction))
                    {
                        if (compilingTrues)
                            compiledTrue.AddRange(compiled);
                        else
                            compiledFalse.AddRange(compiled);
                    }
                    else
                    {
                        return false;
                    }
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
                    errorMessage = $"endif not found : {currentNextLine}."
                });
                return false;
            }

            int startTrue = outputLine + 1;
            int startFalse = startTrue + compiledTrue.Count + 1;
            int endIndex = startFalse + compiledFalse.Count + 1;

            uint jumpToFunctionId = compiler.GetRegisteredFunctionId<JumpToFunction>();

            compiledLines.Add($"{id}{ANSLUtils.COMPILED_DELIMITER}{startTrue}{ANSLUtils.COMPILED_DELIMITER}{startFalse}{ANSLUtils.COMPILED_DELIMITER}{ifContent}");
            compiledLines.AddRange(compiledTrue);
            compiledLines.Add($"{jumpToFunctionId}{ANSLUtils.COMPILED_DELIMITER}{endIndex}");
            compiledLines.AddRange(compiledFalse);
            compiledLines.Add($"{jumpToFunctionId}{ANSLUtils.COMPILED_DELIMITER}{endIndex}");

            return true;
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out uint trueIdx) &&
                parameters.GetParameter(1, out uint falseIdx) &&
                parameters.GetParameter(2, out string content))
            {
                if (CheckIfContent(content, out bool result))
                {
                    context.SetLineCounter(result ? trueIdx : falseIdx);
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

        /// <summary>
        /// Extracts the string inside the first encountered set of ()
        /// </summary>
        /// <param name="fullString">The string</param>
        /// <returns>The extracted string. Null if nothing was found</returns>
        protected string ExtractParenthesisContent(string fullString)
        {
            int startIdx = -1;
            int endIdx = -1;
            int parenthesisToClose = 0;
            for (int i = 0; i < fullString.Length; i++)
            {
                if (fullString[i] == '(')
                {
                    if (parenthesisToClose == 0)
                        startIdx = i + 1;

                    parenthesisToClose++;
                }
                if (fullString[i] == ')')
                {
                    parenthesisToClose--;

                    if (parenthesisToClose == 0)
                    {
                        endIdx = i - 1;

                        if (startIdx < endIdx && startIdx != -1 && endIdx != -1 && endIdx < fullString.Length)
                        {
                            return fullString.Substring(startIdx, endIdx - startIdx + 1);
                        }
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Checks the result for the specified content
        /// </summary>
        /// <param name="result">True if the if was correct</param>
        /// <param name="debug">True if debug mode is enabled (Don't check variables, only the composition)</param>
        /// <returns>True if there was no errors</returns>
        protected bool CheckIfContent(string ifContent, out bool result, bool debug = false)
        {
            PlayerVariableContainer variableContainer = null;
            if (!debug)
            {
                if (!PersistentDataManager.instance.GetPlayerData().GetComponent<PlayerVariableContainer>(out variableContainer))
                {

                    result = false;
                    return false;
                }
            }

            if (ANFUtils.CheckIfContentImpl(ifContent, variableContainer, out result) || debug)
                return true;

            result = false;
            return true;
        }
    }
}