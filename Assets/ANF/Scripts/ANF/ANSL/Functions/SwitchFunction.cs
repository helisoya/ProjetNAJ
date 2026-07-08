using ANF.Persistent;
using ANF.Utils;
using System.Collections.Generic;

namespace ANF.ANSL
{
    /// <summary>
    /// The switch function is used to check a single variable.
    /// If no checks are correct, it will jump to the default marker if it exists
    /// </summary>
    [ANSLFunctionAttribute(
        functionBody: "switch",
        functionAutoComplete: new string[] { "switch(Variable)\\n\\tcase default:\\n\\n\\tcase 0:\\n\\nendswitch" },
        functionDesc: "Checks a single variables. You can use 'case default:' to check when none of the checks are valid")]
    public class SwitchFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] { // variable to check, default jump, list of [value, jump to]
                new FunctionParameterType[]{ FunctionParameterType.STRING,FunctionParameterType.INT, FunctionParameterType.LISTINT}
            };
        }

        public override bool Compile(out List<string> compiledLines, string cleanedLine, uint id, ANSLCompiler compiler, List<ANSLUtils.ANSLError> errors, int outputLine)
        {
            compiledLines = new List<string>();

            string[] split = cleanedLine.Split(new char[] { '(', ')' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (split.Length != 2 && split.Length != 3)
            {
                // Missing parts
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"Bad number of parenthesis : {cleanedLine}."
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

            string variable = split[1];

            List<string> currentCompiledPart = null;
            bool foundEnd = false;
            List<int> variables = new List<int>();
            List<List<string>> compiledParts = new List<List<string>>();

            bool canContinue = true;

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

                if (currentNextLine.StartsWith("endswitch"))
                {
                    if (currentNextLine.Length != "endswitch".Length)
                    {
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Unknown token after the endswitch : {currentNextLine}."
                        });
                        return false;
                    }

                    foundEnd = true;
                    canContinue = false;

                    if (currentCompiledPart != null)
                    {
                        compiledParts.Add(currentCompiledPart);
                        currentCompiledPart = null;
                    }
                }
                else if (currentNextLine.StartsWith("case"))
                {
                    if (currentNextLine.StartsWith("case ") && currentNextLine.EndsWith(":"))
                    {
                        string token = currentNextLine.Substring(5, currentNextLine.Length - 6);
                        if (token.Contains(' ') || token.Contains('\t') || string.IsNullOrEmpty(token))
                        {
                            errors.Add(new ANSLUtils.ANSLError()
                            {
                                type = ANSLUtils.ANSLErrorType.ERROR,
                                filePath = compiler.GetSourceFilepath(),
                                line = compiler.GetCurrentLineCounter(),
                                errorMessage = $"Invalid token : {currentNextLine}."
                            });
                            return false;
                        }

                        if (currentCompiledPart != null)
                        {
                            compiledParts.Add(currentCompiledPart);
                            currentCompiledPart = null;
                        }

                        currentCompiledPart = new List<string>();
                        bool isCompilingDefault = token.Equals("default");

                        if (!isCompilingDefault)
                        {
                            if (int.TryParse(token, out int value))
                            {
                                if (variables.Contains(value))
                                {
                                    errors.Add(new ANSLUtils.ANSLError()
                                    {
                                        type = ANSLUtils.ANSLErrorType.ERROR,
                                        filePath = compiler.GetSourceFilepath(),
                                        line = compiler.GetCurrentLineCounter(),
                                        errorMessage = $"Token already used in switch : {currentNextLine}."
                                    });
                                    return false;
                                }

                                variables.Add(value);
                            }
                            else
                            {
                                errors.Add(new ANSLUtils.ANSLError()
                                {
                                    type = ANSLUtils.ANSLErrorType.ERROR,
                                    filePath = compiler.GetSourceFilepath(),
                                    line = compiler.GetCurrentLineCounter(),
                                    errorMessage = $"Could to parse token to int : {currentNextLine}."
                                });
                                return false;
                            }
                        }
                        else
                        {
                            if (variables.Contains(int.MinValue))
                            {
                                errors.Add(new ANSLUtils.ANSLError()
                                {
                                    type = ANSLUtils.ANSLErrorType.ERROR,
                                    filePath = compiler.GetSourceFilepath(),
                                    line = compiler.GetCurrentLineCounter(),
                                    errorMessage = $"Token already used in switch : {currentNextLine}."
                                });
                                return false;
                            }
                            variables.Add(int.MinValue);
                        }
                    }
                    else
                    {
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Bad case creation : {currentNextLine}."
                        });
                        return false;
                    }
                }
                else
                {
                    if (compiledParts == null)
                    {
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Function linked to no case : {currentNextLine}."
                        });
                        return false;
                    }

                    int outputLineForFunction = outputLine;
                    foreach (List<string> part in compiledParts)
                        outputLineForFunction += part.Count + 1;
                    outputLineForFunction += currentCompiledPart.Count + 1;

                    // Potential function
                    if (compiler.CompileLine(currentNextLine, out List<string> compiled, outputLineForFunction))
                        currentCompiledPart.AddRange(compiled);

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
                    errorMessage = $"endswitch not found : {currentNextLine}."
                });
                return false;
            }


            int startIdx = outputLine + 1;
            int startDefault = -1;
            List<int> starts = new List<int>();
            for (int i = 0; i < compiledParts.Count; i++)
            {
                if (variables[i] == int.MinValue) // default
                    startDefault = startIdx;

                starts.Add(startIdx);

                startIdx += compiledParts[i].Count + 1;
            }

            if (startDefault == -1)
                startDefault = startIdx;

            uint jumpToFunctionId = compiler.GetRegisteredFunctionId<JumpToFunction>();

            string compiledSwitchLine = $"{id}|{variable}|{startDefault}";
            for (int i = 0; i < compiledParts.Count; i++)
            {
                if (variables[i] != int.MinValue)
                    compiledSwitchLine += $"|{variables[i]}|{starts[i]}";

                compiledLines.AddRange(compiledParts[i]);
                compiledLines.Add($"{jumpToFunctionId}|{startIdx}");
            }

            compiledLines.Insert(0, compiledSwitchLine);

            return true;
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string variable) &&
                parameters.GetParameter(1, out int defaultIdx) &&
                parameters.GetParameter(2, out int[] others))
                ProcessSwitch(variable, defaultIdx, others);
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
        /// Process the switch
        /// </summary>
        /// <param name="variable">The variable to check</param>
        /// <param name="defaultIndex">The default jump index</param>
        /// <param name="others">The cases to check</param>
        protected void ProcessSwitch(string variable, int defaultIndex, int[] others)
        {
            int result = defaultIndex;

            if (PersistentDataManager.instance.GetPlayerData().GetComponent(out PlayerVariableContainer container) &&
                container.GetVariable(variable, out int variableValue))
            {
                for (int i = 0; i < others.Length; i += 2)
                {
                    if (others[i] == variableValue)
                    {
                        result = others[i + 1];
                        break;
                    }
                }
            }

            context.SetLineCounter((uint)result);
        }
    }
}