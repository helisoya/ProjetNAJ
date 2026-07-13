using Leguar.TotalJSON;
using ANF.ANSL;
using ANF.Persistent;
using NAJ.Persistent;
using System.Collections.Generic;
using ANF.Utils;
using NAJ.GUI;
using JetBrains.Annotations;
using System.Diagnostics;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Ask function allows you to ask for evidence from the player
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "ask",
        functionAutoComplete: new string[] {
            "ask(Type;CorrectAnwser)\\n\\nwrong\\n\\nendask"
        },
        functionDesc: "Asks for a specific evidence. Multiple anwsers possible. Type can be Evidence,Profile or Any")]
    public class AskFunction : ANSLFunction
    {
        private bool waitingForItem = false;
        private InventoryUI.InventoryMode previousMode = InventoryUI.InventoryMode.ViewOnly;
        private InventoryUI inventoryUI;
        private uint goodIdx;
        private uint badIdx;
        private string[] anwsers;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING,
                    FunctionParameterType.UINT, FunctionParameterType.UINT, FunctionParameterType.LISTSTRING }
            };
        }

        public override bool Compile(out List<string> compiledLines, string cleanedLine, uint id, ANSLCompiler compiler, List<ANSLUtils.ANSLError> errors, int outputLine)
        {
            compiledLines = new List<string>();

            string content = ExtractParenthesisContent(cleanedLine).Replace(" ", "");

            if (content == null)
            {
                // No if content
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"No ask content detected : {cleanedLine}."
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

            string[] contentSplit = content.Split(';');

            if (contentSplit.Length < 2)
            {
                // Not Enough
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"Missing parameters in ask content : {cleanedLine}."
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

            while (canContinue && currentNextLine != null)
            {
                if (string.IsNullOrEmpty(currentNextLine) || string.IsNullOrWhiteSpace(currentNextLine))
                {
                    compiler.CheckNextLine();
                    currentNextLine = compiler.GetCurrentLineClean();
                    continue;
                }

                if (currentNextLine.Equals("wrong"))
                {
                    if (!compilingTrues)
                    {
                        // Already compiling else
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Two wrong detected : {currentNextLine}."
                        });
                        return false;
                    }

                    if (currentNextLine.Length != "wrong".Length)
                    {
                        // Already compiling else
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Unexpected token after the wrong : {currentNextLine}."
                        });
                        return false;
                    }

                    compilingTrues = false;
                }
                else if (currentNextLine.Equals("endask"))
                {
                    if (currentNextLine.Length != "endask".Length)
                    {
                        // Already compiling else
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Unexpected token after the endask : {currentNextLine}."
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
                    errorMessage = $"endask not found : {currentNextLine}."
                });
                return false;
            }

            int startTrue = outputLine + 1;
            int startFalse = startTrue + compiledTrue.Count + 1;
            int endIndex = startFalse + compiledFalse.Count + 1;

            string type = contentSplit[0];
            string anwsers = "";
            for (int i = 1; i < contentSplit.Length; i++)
                anwsers += $"|{contentSplit[i]}";

            uint jumpToFunctionId = compiler.GetRegisteredFunctionId<JumpToFunction>();

            compiledLines.Add($"{id}|{type}|{startTrue}|{startFalse}{anwsers}");
            compiledLines.AddRange(compiledTrue);
            compiledLines.Add($"{jumpToFunctionId}|{endIndex}");
            compiledLines.AddRange(compiledFalse);
            compiledLines.Add($"{jumpToFunctionId}|{endIndex}");

            return true;
        }

        protected override void OnStartProcess()
        {
            waitingForItem = true;
            if (parameters.GetParameter(0, out string type) &&
                parameters.GetParameter(1, out goodIdx) &&
                parameters.GetParameter(2, out badIdx) &&
                parameters.GetParameter(3, out anwsers) &&
                manager.GetGUIManager().GetComponent(out inventoryUI))
            {
                previousMode = inventoryUI.currentMode;
                type = type.ToLower();

                if (type.Equals("any"))
                    inventoryUI.SetCurrentMode(InventoryUI.InventoryMode.MustPresentAny);
                else if (type.Equals("evidence"))
                    inventoryUI.SetCurrentMode(InventoryUI.InventoryMode.MustPresentEvidence);
                else if (type.Equals("profile"))
                    inventoryUI.SetCurrentMode(InventoryUI.InventoryMode.MustPresentProfile);
                else
                    waitingForItem = false;

                if (waitingForItem)
                    inventoryUI.SetEnabled(true);
            }
            else
            {
                waitingForItem = false;
            }

            if (!waitingForItem)
                EndProcess();
        }

        protected override void OnUpdate()
        {
            if (waitingForItem)
            {
                if (!inventoryUI && !manager.GetGUIManager().GetComponent(out inventoryUI))
                {
                    waitingForItem = false;
                    return;
                }

                if (!inventoryUI.isEnabled)
                {
                    inventoryUI.SetCurrentMode(previousMode);
                    waitingForItem = false;

                    string selectedItem = inventoryUI.selectedItem;
                    bool found = false;

                    foreach (string anwser in anwsers)
                    {
                        if (anwser.Equals(selectedItem))
                        {
                            found = true;
                            break;
                        }
                    }

                    EndProcess();
                    if (found)
                        context.SetLineCounter(goodIdx);
                    else
                        context.SetLineCounter(badIdx);
                }
            }
            else
            {
                EndProcess();
            }
        }

        protected override void OnCleanup()
        {
            inventoryUI = null;
        }

        protected override void OnSave(JSON json)
        {
            if (waitingForItem)
            {
                json.Add("waitingForItem", waitingForItem);
                json.Add("previousMode", (int)previousMode);
                json.Add("goodIdx", goodIdx);
                json.Add("badIdx", badIdx);
                json.Add("anwsers", anwsers);
            }
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitingForItem"))
                waitingForItem = json.GetBool("waitingForItem");
            if (json.ContainsKey("previousMode"))
                previousMode = (InventoryUI.InventoryMode)json.GetInt("previousMode");
            if (json.ContainsKey("goodIdx"))
                goodIdx = json.GetJNumber("goodIdx").AsUInt();
            if (json.ContainsKey("badIdx"))
                badIdx = json.GetJNumber("badIdx").AsUInt();
            if (json.ContainsKey("anwsers"))
                anwsers = json.GetJArray("anwsers").AsStringArray();
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
    }
}

