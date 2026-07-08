using ANF.GUI;
using ANF.Utils;
using Leguar.TotalJSON;
using System.Collections.Generic;


namespace ANF.ANSL
{
    /// <summary>
    /// The Choice Function starts a choice sequence, allowing the player to chose between a few options
    /// </summary>
    [ANSLFunctionAttribute(
        functionBody: "choice",
        functionAutoComplete: new string[] {
            "choice(Title)\n\t choice Key:\n\nendchoice"
        },
        functionDesc: "Starts a choice")]
    public class ChoiceFunction : ANSLFunction
    {
        private bool waitingForChoice = false;
        private ChoiceUI choiceUI;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.LISTSTRING}
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

            string titleKey = split[1];

            List<string> currentCompiledPart = null;
            bool foundEnd = false;
            List<string> buttonKey = new List<string>();
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

                if (currentNextLine.StartsWith("endchoice"))
                {
                    if (currentNextLine.Length != "endchoice".Length)
                    {
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Unknown token after the endchoice : {currentNextLine}."
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
                else if (currentNextLine.StartsWith("choice"))
                {
                    if (currentNextLine.StartsWith("choice ") && currentNextLine.EndsWith(":"))
                    {
                        string token = currentNextLine.Substring(7, currentNextLine.Length - 8);
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
                        buttonKey.Add(token);
                    }
                    else
                    {
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Bad choice creation : {currentNextLine}."
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
                            errorMessage = $"Function linked to no choice : {currentNextLine}."
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
                    errorMessage = $"endchoice not found : {currentNextLine}."
                });
                return false;
            }


            int startIdx = outputLine + 1;
            List<int> starts = new List<int>();
            for (int i = 0; i < compiledParts.Count; i++)
            {
                starts.Add(startIdx);
                startIdx += compiledParts[i].Count + 1;
            }

            string compiledSwitchLine = $"{id}|{titleKey}";
            for (int i = 0; i < compiledParts.Count; i++)
            {
                compiledSwitchLine += $"|{buttonKey[i]}|{starts[i]}";

                compiledLines.AddRange(compiledParts[i]);

                uint jumpToFunctionId = compiler.GetRegisteredFunctionId<JumpToFunction>();
                compiledLines.Add($"{jumpToFunctionId}|{startIdx}");
            }

            compiledLines.Insert(0, compiledSwitchLine);

            return true;
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string titleKey) &&
                parameters.GetParameter(1, out string[] choices) &&
                manager.GetGUIManager().GetComponent<ChoiceUI>(out choiceUI))
            {
                ChoiceData data = new ChoiceData();
                data.title = titleKey;
                data.entries = new ChoiceData.ChoiceDataEntry[choices.Length / 2];
                for (int i = 0; i < choices.Length; i += 2)
                {
                    data.entries[i / 2] = new ChoiceData.ChoiceDataEntry() { textKey = choices[i], linkedLine = uint.Parse(choices[i + 1]) };
                }

                choiceUI.SetEnabled(true, data);

                waitingForChoice = true;
            }
            else
            {
                EndProcess();
            }

        }

        protected override void OnUpdate()
        {
            if (choiceUI == null)
                manager.GetGUIManager().GetComponent<ChoiceUI>(out choiceUI);

            if (choiceUI != null && choiceUI.showingChoice)
                return;

            if (choiceUI)
            {
                if (choiceUI.showingChoice)
                    return;

                EndProcess();
                context.SetLineCounter(choiceUI.selectedLine);
                choiceUI = null;
            }
            else
            {
                EndProcess();
            }
        }

        protected override void OnCleanup()
        {
            
        }

        protected override void OnSave(JSON json)
        {
            json.Add("waitingForChoice", waitingForChoice);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitingForChoice"))
                waitingForChoice = json.GetBool("waitingForChoice");
            else
                waitingForChoice = false;
        }
    }
}

