using ANF.ANSL;
using ANF.Persistent;
using ANF.Utils;
using JetBrains.Annotations;
using Leguar.TotalJSON;
using NAJ.GUI;
using NAJ.Persistent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Examination function allows you to start an examination
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "examination",
        functionAutoComplete: new string[] {
            "examination\\n\\n1\\n\\n1Press\\n\\n2\\n\\n2Press\\n\\nloop\\n\\nfail\\n\\nendexamination"
        },
        functionDesc: "Starts a cross examination section. As long as clearExamination() isn't called, consider the examination ongoing.")]
    public class ExaminationFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{ FunctionParameterType.UINT, FunctionParameterType.LISTSTRING }
            };
        }

        public override bool Compile(out List<string> compiledLines, string cleanedLine, uint id, ANSLCompiler compiler, List<ANSLUtils.ANSLError> errors, int outputLine)
        {
            compiledLines = new List<string>();

            if (cleanedLine.Length != "examination".Length)
            {
                // Unknown Token
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"Unknown token detected : {cleanedLine}."
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


            List<string> uncompiledFail = new List<string>();
            List<string> uncompiledLoop = new List<string>();
            Dictionary<string, List<string>> uncompiledParts = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> uncompiledPress = new Dictionary<string, List<string>>();
            Dictionary<string, string[]> partsParameters = new Dictionary<string, string[]>();

            List<string> compiledFail = new List<string>();
            List<string> compiledLoop = new List<string>();
            Dictionary<string,List<string>> compiledParts = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> compiledPress = new Dictionary<string, List<string>>();
            List<string> currentList = null;
            bool canContinue = true;
            bool foundEnd = false;
            bool foundFail = false;
            bool foundLoop = false;
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

                if (currentNextLine.Equals("fail"))
                {
                    if (foundFail)
                    {
                        // Already found a fail
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Fail Token already used : {currentNextLine}."
                        });
                        return false;
                    }

                    foundFail = true;
                    currentList = uncompiledFail;
                }
                else if (currentNextLine.Equals("loop"))
                {
                    if (foundLoop)
                    {
                        // Already found a loop
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Loop Token already used : {currentNextLine}."
                        });
                        return false;
                    }

                    foundLoop = true;
                    currentList = uncompiledLoop;
                }
                else if (currentNextLine.Equals("endexamination"))
                {
                    foundEnd = true;
                    canContinue = false;
                }
                else if (currentNextLine.StartsWith("part"))
                {
                    string[] split = currentNextLine.Split("");

                    if(split.Length < 2)
                    {
                        // No ID
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"No ID in part : {currentNextLine}."
                        });
                        return false;
                    }

                    if(split.Length >= 3 && !((split.Length == 3 && split[2].Equals("press")) || (split[3].Equals("present") && split.Length >= 4)))
                    {
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Missing part parameters : {currentNextLine}."
                        });
                        return false;
                    }

                    string partId = split[1];
                    if(string.IsNullOrEmpty(partId))
                    {
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Invalid part ID : {currentNextLine}."
                        });
                        return false;
                    }

                    if (partsParameters.ContainsKey(partId))
                    {
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Part Id already in use : {currentNextLine}."
                        });
                        return false;
                    }

                    string[] parameters = new string[split.Length - 2];
                    for(int i = 2;i < split.Length;i++)
                    {
                        parameters[i-2] = split[i];
                    }
                    partsParameters.Add(partId, parameters);
                    currentList = new List<string>();
                    uncompiledParts.Add(partId,currentList);
                }
                else if (currentNextLine.StartsWith("press"))
                {
                    string[] split = currentNextLine.Split("");

                    if (split.Length != 2)
                    {
                        // Invalid arguments
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Invalid arguments : {currentNextLine}."
                        });
                        return false;
                    }

                    string partId = split[1];
                    if (string.IsNullOrEmpty(partId))
                    {
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Invalid part ID : {currentNextLine}."
                        });
                        return false;
                    }

                    if (uncompiledPress.ContainsKey(partId))
                    {
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Press Id already in use : {currentNextLine}."
                        });
                        return false;
                    }

                    currentList = new List<string>();
                    uncompiledPress.Add(partId, currentList);
                }
                else
                {
                    if (currentList == null)
                    {
                        // Already found a fail
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Misplaced function call. Did you forget a token ? : {currentNextLine}."
                        });
                        return false;
                    }

                    currentList.Add(currentNextLine);
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
                    errorMessage = $"endexamination not found : {currentNextLine}."
                });
                return false;
            }

            if (!foundFail)
            {
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"fail keyword not found : {currentNextLine}."
                });
                return false;
            }

            if (!foundLoop)
            {
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"loop keyword not found : {currentNextLine}."
                });
                return false;
            }

            if (partsParameters.Count == 0)
            {
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"No parts detected : {currentNextLine}."
                });
                return false;
            }

            foreach (string key in uncompiledParts.Keys)
            {
                if(!uncompiledPress.ContainsKey(key))
                {
                    errors.Add(new ANSLUtils.ANSLError()
                    {
                        type = ANSLUtils.ANSLErrorType.ERROR,
                        filePath = compiler.GetSourceFilepath(),
                        line = compiler.GetCurrentLineCounter(),
                        errorMessage = $"Part {key} has no press data : {currentNextLine}."
                    });
                    return false;
                }
            }

            foreach (string key in uncompiledPress.Keys)
            {
                if (!uncompiledParts.ContainsKey(key))
                {
                    errors.Add(new ANSLUtils.ANSLError()
                    {
                        type = ANSLUtils.ANSLErrorType.ERROR,
                        filePath = compiler.GetSourceFilepath(),
                        line = compiler.GetCurrentLineCounter(),
                        errorMessage = $"Press {key} has no linked data : {currentNextLine}."
                    });
                    return false;
                }
            }

            // Fomat :
            // EXAMINATION_FUNCTION ...
            // SET_EXAMINATION_PART_FUNCTION 1
            // ...
            // EXAMINATION_WAIT_FUNCTION
            // ... 
            // JUMP_TO_FUNCTION ouputLine
            // SET_EXAMINATION_PART_FUNCTION 2
            // ...
            // EXAMINATION_WAIT_FUNCTION
            // ... 
            // JUMP_TO_FUNCTION ouputLine
            // SET_EXAMINATION_PART_FUNCTION 1
            // ...
            // JUMP_TO_FUNCTION ouputLine
            // SET_EXAMINATION_PART_FUNCTION 1
            // ... 
            // JUMP_TO_FUNCTION ouputLine

            int currentLine = outputLine + 1;

            Dictionary<string, Vector2Int> partsLines = new Dictionary<string, Vector2Int>();
            int failLine;
            int loopLine;


            foreach (string key in uncompiledParts.Keys)
            {
                if (uncompiledParts[key].Count == 0)
                    continue;

                currentList = new List<string>();
                compiledParts.Add(key, currentList);

                Vector2Int lines = new Vector2Int();
                lines.x = currentLine;

                currentLine++; // Skip SET_EXAMINATION_PART_FUNCTION

                foreach (string uncompiledLine in uncompiledParts[key])
                {
                    if (compiler.CompileLine(uncompiledLine, out List<string> compiled, currentLine))
                    {
                        currentLine += compiled.Count; // Skip compiled lines
                        currentList.AddRange(compiled);
                    }
                    else
                    {
                        return false;
                    }
                }

                currentLine++; // Skip EXAMINATION_WAIT_FUNCTION

                lines.x = currentLine;

                currentList = new List<string>();
                compiledPress.Add(key, currentList);

                foreach (string uncompiledLine in uncompiledPress[key])
                {
                    if (compiler.CompileLine(uncompiledLine, out List<string> compiled, currentLine))
                    {
                        currentLine += compiled.Count; // Skip compiled lines
                        currentList.AddRange(compiled);
                    }
                    else
                    {
                        return false;
                    }
                }

                currentLine++; // SKIP JUMP_TO_FUNCTION
                partsLines.Add(key,lines);
            }

            loopLine = currentLine;

            if (uncompiledLoop.Count != 0)
            {
                compiledLoop = new List<string>();

                currentLine++; // Skip SET_EXAMINATION_PART_FUNCTION

                foreach (string uncompiledLine in uncompiledLoop)
                {
                    if (compiler.CompileLine(uncompiledLine, out List<string> compiled, currentLine))
                    {
                        currentLine += compiled.Count; // Skip compiled lines
                        compiledLoop.AddRange(compiled);
                    }
                    else
                    {
                        return false;
                    }
                }
                currentLine++; // SKIP JUMP_TO_FUNCTION
            }

            failLine = currentLine;

            if (uncompiledFail.Count != 0)
            {
                compiledFail = new List<string>();

                currentLine++; // Skip SET_EXAMINATION_PART_FUNCTION

                foreach (string uncompiledLine in uncompiledFail)
                {
                    if (compiler.CompileLine(uncompiledLine, out List<string> compiled, currentLine))
                    {
                        currentLine += compiled.Count; // Skip compiled lines
                        compiledFail.AddRange(compiled);
                    }
                    else
                    {
                        return false;
                    }
                }
                currentLine++; // SKIP JUMP_TO_FUNCTION
            }

            int endLine = currentLine;

            uint jumpToFunctionId = compiler.GetRegisteredFunctionId<JumpToFunction>();
            uint setExaminationPart = compiler.GetRegisteredFunctionId<JumpToFunction>();
            uint examinationPartWaiter = compiler.GetRegisteredFunctionId<JumpToFunction>();

            string[] keys = partsParameters.Keys.ToArray();

            string partsData = "";
            foreach(string partId in keys)
            {
                string[] parameters = partsParameters[partId];
                partsData += $"|{partId}{partsLines[partId].x}{parameters.Length > 0 && parameters[0].Equals("present")}";
            }
            compiledLines.Add($"{id}|{endLine}{partsData}");



            for(int i = 0; i < keys.Length; i++)
            {
                string partId = keys[i];
                compiledLines.Add($"{setExaminationPart}|{partId}");
                compiledLines.AddRange(compiledParts[partId]);

                string[] parameters = partsParameters[partId];
                bool shouldPress = parameters.Length > 0 && parameters[0].Equals("press");
                string evidenceToShow = "";
                if(parameters.Length > 0 && parameters[0].Equals("present"))
                {
                    for(int j = 1; j < parameters.Length;j++)
                    {
                        evidenceToShow += $"|{parameters[j]}";
                    }
                }

                compiledLines.Add($"{examinationPartWaiter}|{partId}|{shouldPress}|{partsLines[partId].y}|" +
                    $"{(i > 0 ? partsLines[keys[i-1]].x : -1)}|{(i == keys.Length - 1 ? loopLine : partsLines[keys[i + 1]].x)}");

                compiledLines.AddRange(compiledPress[partId]);
                compiledLines.Add($"{jumpToFunctionId}|{outputLine}");
            }

            compiledLines.Add($"{setExaminationPart}|{keys[0]}");
            compiledLines.AddRange(compiledLoop);
            compiledLines.Add($"{jumpToFunctionId}|{outputLine}");

            compiledLines.Add($"{setExaminationPart}|{keys[0]}");
            compiledLines.AddRange(compiledFail);
            compiledLines.Add($"{jumpToFunctionId}|{outputLine}");

            return true;
        }

        protected override void OnStartProcess()
        {
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnCleanup()
        {
        }

        protected override void OnSave(JSON json)
        {
        }

        protected override void OnLoad(JSON json)
        {

        }
    }
}

