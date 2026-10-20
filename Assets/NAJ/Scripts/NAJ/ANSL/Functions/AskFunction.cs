using Leguar.TotalJSON;
using ANF.ANSL;
using ANF.Persistent;
using NAJ.Persistent;
using System.Collections.Generic;
using ANF.Utils;
using NAJ.GUI;
using JetBrains.Annotations;
using System.Diagnostics;
using System;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Ask function allows you to ask for evidence from the player
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "ask",
        functionAutoComplete: new string[] {
            "ask(Type)\\n\\ncase Item:\\n\\nendask"
        },
        functionDesc: "Asks for a specific evidence. Multiple anwsers possible. Type can be Evidence,Profile or Any")]
    public class AskFunction : ANSLFunction
    {
        private bool waitingForItem = false;
        private InventoryUI.InventoryMode previousMode = InventoryUI.InventoryMode.ViewOnly;
        private InventoryUI inventoryUI;
        private string[] anwsers;
        private uint afterIdx;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.UINT, FunctionParameterType.LISTSTRING }
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

            string type = split[1];

            List<string> currentCompiledPart = null;
            bool foundEnd = false;
            List<string[]> items = new List<string[]>();
            List<List<string>> compiledParts = new List<List<string>>();

            bool canContinue = true;

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

                if (currentNextLine.Equals("endask"))
                {
                    if (currentNextLine.Length != "endask".Length)
                    {
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Unknown token after the endask : {currentNextLine}."
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
                                errorMessage = $"Invalid items : {currentNextLine}."
                            });
                            return false;
                        }

                        if (currentCompiledPart != null)
                        {
                            compiledParts.Add(currentCompiledPart);
                            currentCompiledPart = null;
                        }

                        string[] splitItems = token.Split(';');

                        currentCompiledPart = new List<string>();

                        foreach (string item in splitItems)
                        {
                            foreach (string[] itemData in items)
                            {
                                foreach (string individualItem in itemData)
                                {
                                    if (item.Equals(individualItem))
                                    {
                                        errors.Add(new ANSLUtils.ANSLError()
                                        {
                                            type = ANSLUtils.ANSLErrorType.ERROR,
                                            filePath = compiler.GetSourceFilepath(),
                                            line = compiler.GetCurrentLineCounter(),
                                            errorMessage = $"Item already used in ask : {currentNextLine}."
                                        });
                                        return false;
                                    }
                                }
                            }
                        }

                        items.Add(splitItems);
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
                    errorMessage = $"endask not found : {currentNextLine}."
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

            uint jumpToFunctionId = compiler.GetRegisteredFunctionId<JumpToFunction>();

            string compiledSwitchLine = $"{id}{ANSLUtils.COMPILED_DELIMITER}{type}{ANSLUtils.COMPILED_DELIMITER}{startIdx}";
            for (int i = 0; i < compiledParts.Count; i++)
            {
                foreach (string item in items[i])
                    compiledSwitchLine += $"{ANSLUtils.COMPILED_DELIMITER}{item}{ANSLUtils.COMPILED_DELIMITER}{starts[i]}";

                compiledLines.AddRange(compiledParts[i]);
                compiledLines.Add($"{jumpToFunctionId}{ANSLUtils.COMPILED_DELIMITER}{startIdx}");
            }

            compiledLines.Insert(0, compiledSwitchLine);

            return true;
        }

        protected override void OnStartProcess()
        {
            waitingForItem = true;
            if (parameters.GetParameter(0, out string type) &&
                parameters.GetParameter(1, out afterIdx) &&
                parameters.GetParameter(2, out anwsers) &&
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
                    uint defaultIndex = 0;
                    uint foundIdx = 0;
                    bool foundDefault = false;
                    bool found = false;

                    int anwserCount = anwsers.Length / 2;

                    for (int i = 0; i < anwserCount; i++)
                    {
                        if (anwsers[i * 2].Equals(selectedItem))
                        {
                            uint.TryParse(anwsers[i * 2 + 1], out foundIdx);
                            found = true;
                            break;
                        }
                        else if (anwsers[i * 2].Equals("default"))
                        {
                            uint.TryParse(anwsers[i * 2 + 1], out defaultIndex);
                            foundDefault = true;
                        }
                    }

                    EndProcess();
                    if (found)
                        context.SetLineCounter(foundIdx);
                    else if (foundDefault)
                        context.SetLineCounter(defaultIndex);
                    else
                        context.SetLineCounter(afterIdx);
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
                json.Add("afterIdx", afterIdx);
                json.Add("anwsers", anwsers);
            }
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitingForItem"))
                waitingForItem = json.GetBool("waitingForItem");
            if (json.ContainsKey("previousMode"))
                previousMode = (InventoryUI.InventoryMode)json.GetInt("previousMode");
            if (json.ContainsKey("afterIdx"))
                afterIdx = json.GetJNumber("afterIdx").AsUInt();
            if (json.ContainsKey("anwsers"))
                anwsers = json.GetJArray("anwsers").AsStringArray();
        }
    }
}

