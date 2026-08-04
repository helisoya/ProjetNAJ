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
            "choice(List)\n\t choice Key:\n\nendchoice"
        },
        functionDesc: "Starts a choice (List/AroundImage)")]
    public class ChoiceFunction : ANSLFunction
    {
        private bool waitingForChoice = false;
        private ChoiceUI choiceUI;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.INT, FunctionParameterType.LISTSTRING}
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

            string type = split[1].ToLower();
            int choiceType = 0;

            if (type.ToLower().Equals("list"))
            {
                choiceType = (int)ChoiceData.ChoiceType.List;
            }
            else if (type.ToLower().Equals("circlearoundimage"))
            {
                choiceType = (int)ChoiceData.ChoiceType.AroundImage;
            }
            else
            {
                // Unknown type
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"Unknown choice type : {cleanedLine}."
                });
                return false;
            }

            List<string> currentCompiledPart = null;
            bool foundEnd = false;
            List<string> buttonKey = new List<string>();
            List<string> buttonSprite = new List<string>();
            List<string> buttonSpriteSheet = new List<string>();
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

                if (currentNextLine.Equals("endchoice"))
                {

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
                        string choiceData = currentNextLine.Substring(7, currentNextLine.Length - 8);

                        string[] splitedChoiceData = choiceData.Split(' ');
                        // 0 = Choice token
                        // 1 = Sprite
                        // 2 = Sprite Sheet

                        if (splitedChoiceData.Length == 0 || splitedChoiceData.Length > 3)
                        {
                            errors.Add(new ANSLUtils.ANSLError()
                            {
                                type = ANSLUtils.ANSLErrorType.ERROR,
                                filePath = compiler.GetSourceFilepath(),
                                line = compiler.GetCurrentLineCounter(),
                                errorMessage = $"Invalid arguments : {currentNextLine}."
                            });
                            return false;
                        }

                        if (splitedChoiceData[0].Contains('\t') || string.IsNullOrEmpty(splitedChoiceData[0]))
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
                        buttonKey.Add(splitedChoiceData[0]);

                        if (splitedChoiceData.Length >= 2 && !string.IsNullOrEmpty(splitedChoiceData[1]))
                            buttonSprite.Add(splitedChoiceData[1]);
                        else
                            buttonSprite.Add("null");

                        if (splitedChoiceData.Length == 3 && !string.IsNullOrEmpty(splitedChoiceData[2]))
                            buttonSpriteSheet.Add(splitedChoiceData[2]);
                        else
                            buttonSpriteSheet.Add("null");
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

            // Choice
            // ID TYPE [CHOICE_ID NEXT_LINE SPRITE_NAME, SPRITE_SHEET]

            string compiledChoiceLine = $"{id}|{choiceType.ToString()}";
            for (int i = 0; i < compiledParts.Count; i++)
            {
                compiledChoiceLine += $"|{buttonKey[i]}|{starts[i]}|{buttonSprite[i]}|{buttonSpriteSheet[i]}";

                compiledLines.AddRange(compiledParts[i]);

                uint jumpToFunctionId = compiler.GetRegisteredFunctionId<JumpToFunction>();
                compiledLines.Add($"{jumpToFunctionId}|{startIdx}");
            }

            compiledLines.Insert(0, compiledChoiceLine);

            return true;
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out int type) &&
                parameters.GetParameter(1, out string[] choices) &&
                manager.GetGUIManager().GetComponent<ChoiceUI>(out choiceUI))
            {
                ChoiceData data = new ChoiceData();
                data.type = (ChoiceData.ChoiceType)type;
                data.entries = new ChoiceData.ChoiceDataEntry[choices.Length / 4];

                for (int i = 0; i < choices.Length; i += 4)
                {
                    data.entries[i / 4] = new ChoiceData.ChoiceDataEntry()
                    {
                        textKey = choices[i],
                        linkedLine = uint.Parse(choices[i + 1]),
                        linkedSprite = choices[i + 2] == "null" ? null : choices[i + 2],
                        linkedSpritesheet = choices[i + 3] == "null" ? null : choices[i + 3]
                    };
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

