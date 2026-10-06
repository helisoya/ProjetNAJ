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
            "examination\\n\\npart 1\\n\\npress 1\\n\\npart 2\\n\\npress 2\\n\\nloop\\n\\nfail\\n\\nendexamination"
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

        /// <summary>
		/// Flags for compiling an examination
		/// </summary>
        private enum ExaminationCompilerFlag
        {
            None,
            CompilingPress,
            CompilingPart,
            CompilingFail,
            CompilingLoop
        }

        private void CompilerCloseStep(ExaminationCompilerFlag currentFlag, string currentPartId, ANSLCompiler compiler,
            ref Vector2Int uncompiledLoop, ref Vector2Int uncompiledFail,
            Dictionary<string, Vector2Int> uncompiledPress, Dictionary<string, Vector2Int> uncompiledParts)
        {
            switch (currentFlag)
            {
                case ExaminationCompilerFlag.CompilingPress:
                    uncompiledPress[currentPartId] = new Vector2Int(uncompiledPress[currentPartId].x, compiler.GetCurrentLineCounter() - 1);
                    break;

                case ExaminationCompilerFlag.CompilingPart:
                    uncompiledParts[currentPartId] = new Vector2Int(uncompiledParts[currentPartId].x, compiler.GetCurrentLineCounter() - 1);
                    break;

                case ExaminationCompilerFlag.CompilingFail:
                    uncompiledFail.y = compiler.GetCurrentLineCounter() - 1;
                    break;

                case ExaminationCompilerFlag.CompilingLoop:
                    uncompiledLoop.y = compiler.GetCurrentLineCounter() - 1;
                    break;
            }
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

            Vector2Int uncompiledFail = new Vector2Int(-1, -1);
            Vector2Int uncompiledLoop = new Vector2Int(-1, -1);
            Dictionary<string, Vector2Int> uncompiledParts = new Dictionary<string, Vector2Int>();
            Dictionary<string, Vector2Int> uncompiledPress = new Dictionary<string, Vector2Int>();
            Dictionary<string, string[]> partsParameters = new Dictionary<string, string[]>();

            List<string> compiledFail = new List<string>();
            List<string> compiledLoop = new List<string>();
            Dictionary<string, List<string>> compiledParts = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> compiledPress = new Dictionary<string, List<string>>();
            string currentPartId = null;
            ExaminationCompilerFlag currentFlag = ExaminationCompilerFlag.None;

            bool canContinue = true;
            bool foundEnd = false;
            bool foundFail = false;
            bool foundLoop = false;
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

                    CompilerCloseStep(currentFlag, currentPartId, compiler, ref uncompiledLoop, ref uncompiledFail, uncompiledPress, uncompiledParts);

                    currentFlag = ExaminationCompilerFlag.CompilingFail;
                    currentPartId = null;
                    uncompiledFail.x = compiler.GetCurrentLineCounter() + 1;
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

                    CompilerCloseStep(currentFlag, currentPartId, compiler, ref uncompiledLoop, ref uncompiledFail, uncompiledPress, uncompiledParts);

                    currentFlag = ExaminationCompilerFlag.CompilingLoop;
                    currentPartId = null;
                    uncompiledLoop.x = compiler.GetCurrentLineCounter() + 1;
                }
                else if (currentNextLine.Equals("endexamination"))
                {
                    foundEnd = true;
                    canContinue = false;
                }
                else if (currentNextLine.StartsWith("part"))
                {
                    string[] split = currentNextLine.Split(' ');

                    if (split.Length < 2)
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

                    if (split.Length < 2 || (split.Length >= 3 &&
                        (
                            (!split[2].Equals("present") && !split[2].Equals("press")) ||
                            (split.Length == 3 && split[2].Equals("present")) ||
                            (split.Length > 3 && split[2].Equals("press"))
                        )))
                    {
                        errors.Add(new ANSLUtils.ANSLError()
                        {
                            type = ANSLUtils.ANSLErrorType.ERROR,
                            filePath = compiler.GetSourceFilepath(),
                            line = compiler.GetCurrentLineCounter(),
                            errorMessage = $"Invalid part parameters : {currentNextLine}."
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
                    for (int i = 2; i < split.Length; i++)
                    {
                        parameters[i - 2] = split[i];
                    }
                    partsParameters.Add(partId, parameters);

                    CompilerCloseStep(currentFlag, currentPartId, compiler, ref uncompiledLoop, ref uncompiledFail, uncompiledPress, uncompiledParts);

                    currentFlag = ExaminationCompilerFlag.CompilingPart;
                    currentPartId = partId;
                    uncompiledParts.Add(partId, new Vector2Int(compiler.GetCurrentLineCounter() + 1, -1));
                }
                else if (currentNextLine.StartsWith("press"))
                {
                    string[] split = currentNextLine.Split(' ');

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

                    CompilerCloseStep(currentFlag, currentPartId, compiler, ref uncompiledLoop, ref uncompiledFail, uncompiledPress, uncompiledParts);

                    currentFlag = ExaminationCompilerFlag.CompilingPress;
                    currentPartId = partId;
                    uncompiledPress.Add(partId, new Vector2Int(compiler.GetCurrentLineCounter() + 1, -1));
                }
                else
                {
                    if (currentFlag == ExaminationCompilerFlag.None)
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
                if (!uncompiledPress.ContainsKey(key))
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

            if (currentFlag != ExaminationCompilerFlag.None)
            {
                CompilerCloseStep(currentFlag, currentPartId, compiler, ref uncompiledLoop, ref uncompiledFail, uncompiledPress, uncompiledParts);
            }

            // Fomat :
            // EXAMINATION_FUNCTION ...
            // SET_EXAMINATION_PART_FUNCTION 1
            // SHOW_EXAMINATION_UI_FUNCTION true
            // ...
            // EXAMINATION_WAIT_FUNCTION
            // ... 
            // SET_EXAMINATION_PART_FUNCTION 2
            // JUMP_TO_FUNCTION ouputLine
            // SET_EXAMINATION_PART_FUNCTION 2
            // SHOW_EXAMINATION_UI_FUNCTION true
            // ...
            // EXAMINATION_WAIT_FUNCTION
            // ... 
            // JUMP_TO_FUNCTION loopLine
            // CHECK_EXAMINATION_PRESSED ...
            // SET_EXAMINATION_PART_FUNCTION 1
            // SHOW_EXAMINATION_UI_FUNCTION false
            // ...
            // JUMP_TO_FUNCTION ouputLine
            // SET_EXAMINATION_PART_FUNCTION 1
            // SHOW_EXAMINATION_UI_FUNCTION false
            // ... 
            // JUMP_TO_FUNCTION ouputLine

            int currentLine = outputLine + 1;
            int sourceFileNextLine = compiler.GetCurrentLineCounter();

            string[] keys = partsParameters.Keys.ToArray();
            Dictionary<string, Vector2Int> partsLines = new Dictionary<string, Vector2Int>();
            int failLine;
            int loopLine;


            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                if (uncompiledParts[key].y == -1 || uncompiledParts[key].x == -1)
                {
                    errors.Add(new ANSLUtils.ANSLError()
                    {
                        type = ANSLUtils.ANSLErrorType.WARNING,
                        filePath = compiler.GetSourceFilepath(),
                        line = compiler.GetCurrentLineCounter(),
                        errorMessage = $"Press {key} has unknown range : {uncompiledParts[key].x}-{uncompiledParts[key].y}."
                    });
                    continue;
                }


                List<string> currentList = new List<string>();
                compiledParts.Add(key, currentList);

                Vector2Int lines = new Vector2Int();
                lines.x = currentLine;

                currentLine += 2; // Skip SET_EXAMINATION_PART_FUNCTION & SHOW_EXAMINATION_UI_FUNCTION

                Vector2Int range = uncompiledParts[key];

                compiler.CheckLine(range.x);

                while (compiler.GetCurrentLineCounter() <= range.y)
                {
                    if (compiler.CompileLine(compiler.GetCurrentLineClean(), out List<string> compiled, currentLine))
                    {
                        currentLine += compiled.Count; // Skip compiled lines
                        currentList.AddRange(compiled);
                        compiler.CheckNextLine();
                    }
                    else
                    {
                        return false;
                    }
                }

                currentLine++; // Skip EXAMINATION_WAIT_FUNCTION

                lines.y = currentLine;

                currentList = new List<string>();
                compiledPress.Add(key, currentList);

                range = uncompiledPress[key];

                compiler.CheckLine(range.x);

                while (compiler.GetCurrentLineCounter() <= range.y)
                {
                    if (compiler.CompileLine(compiler.GetCurrentLineClean(), out List<string> compiled, currentLine))
                    {
                        currentLine += compiled.Count; // Skip compiled lines
                        currentList.AddRange(compiled);
                        compiler.CheckNextLine();
                    }
                    else
                    {
                        return false;
                    }
                }

                currentLine++; // SKIP JUMP_TO_FUNCTION
                if (i != keys.Length - 1) // Skip SET_EXAMINATION_PART_FUNCTION
                    currentLine++;

                partsLines.Add(key, lines);
            }

            loopLine = currentLine;

            if (uncompiledLoop.x != -1 && uncompiledLoop.y != -1)
            {
                compiledLoop = new List<string>();

                currentLine += 3; // Skip CHECK_EXAMINATION_PRESSED, SET_EXAMINATION_PART_FUNCTION & SHOW_EXAMINATION_UI_FUNCTION

                compiler.CheckLine(uncompiledLoop.x);

                while (compiler.GetCurrentLineCounter() <= uncompiledLoop.y)
                {
                    if (compiler.CompileLine(compiler.GetCurrentLineClean(), out List<string> compiled, currentLine))
                    {
                        currentLine += compiled.Count; // Skip compiled lines
                        compiledLoop.AddRange(compiled);
                        compiler.CheckNextLine();
                    }
                    else
                    {
                        return false;
                    }
                }
                currentLine++; // SKIP JUMP_TO_FUNCTION
            }
            else
            {
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.WARNING,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"No loop content detected {uncompiledLoop.x}-{uncompiledLoop.y}"
                });
            }


            failLine = currentLine;

            if (uncompiledFail.x != -1 && uncompiledFail.y != -1)
            {
                compiledFail = new List<string>();

                currentLine += 2; // Skip SET_EXAMINATION_PART_FUNCTION & SHOW_EXAMINATION_UI_FUNCTION

                compiler.CheckLine(uncompiledFail.x);

                while (compiler.GetCurrentLineCounter() <= uncompiledFail.y)
                {
                    if (compiler.CompileLine(compiler.GetCurrentLineClean(), out List<string> compiled, currentLine))
                    {
                        currentLine += compiled.Count; // Skip compiled lines
                        compiledFail.AddRange(compiled);
                        compiler.CheckNextLine();
                    }
                    else
                    {
                        return false;
                    }
                }
                currentLine++; // SKIP JUMP_TO_FUNCTION
            }
            else
            {
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.WARNING,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"No fail content detected {uncompiledFail.x}-{uncompiledFail.y}"
                });
            }

            int endLine = currentLine;

            uint jumpToFunctionId = compiler.GetRegisteredFunctionId<JumpToFunction>();
            uint setExaminationPart = compiler.GetRegisteredFunctionId<SetExaminationPartFunction>();
            uint examinationPartWaiter = compiler.GetRegisteredFunctionId<ExaminationWaiterFunction>();
            uint showExaminationUIId = compiler.GetRegisteredFunctionId<ShowExaminationUIFunction>();
            uint checkExaminationPressedFunction = compiler.GetRegisteredFunctionId<CheckExaminationPressedFunction>();


            string partsData = "";
            foreach (string partId in keys)
            {
                string[] parameters = partsParameters[partId];
                partsData += $"{ANSLUtils.COMPILED_DELIMITER}{partId}{ANSLUtils.COMPILED_DELIMITER}{partsLines[partId].x}{ANSLUtils.COMPILED_DELIMITER}{parameters.Length > 0 && parameters[0].Equals("press")}";
            }
            compiledLines.Add($"{id}{ANSLUtils.COMPILED_DELIMITER}{endLine}{partsData}");

            for (int i = 0; i < keys.Length; i++)
            {
                string partId = keys[i];
                compiledLines.Add($"{setExaminationPart}{ANSLUtils.COMPILED_DELIMITER}{partId}");
                compiledLines.Add($"{showExaminationUIId}{ANSLUtils.COMPILED_DELIMITER}true");
                compiledLines.AddRange(compiledParts[partId]);

                string[] parameters = partsParameters[partId];
                bool shouldPress = parameters.Length > 0 && parameters[0].Equals("press");
                string evidenceToShow = "";
                if (parameters.Length > 0 && parameters[0].Equals("present"))
                {
                    for (int j = 1; j < parameters.Length; j++)
                    {
                        evidenceToShow += $"{ANSLUtils.COMPILED_DELIMITER}{parameters[j]}";
                    }
                }

                compiledLines.Add($"{examinationPartWaiter}{ANSLUtils.COMPILED_DELIMITER}{partId}{ANSLUtils.COMPILED_DELIMITER}{shouldPress}{ANSLUtils.COMPILED_DELIMITER}{partsLines[partId].y}{ANSLUtils.COMPILED_DELIMITER}{failLine}{ANSLUtils.COMPILED_DELIMITER}{endLine}" +
                    $"{ANSLUtils.COMPILED_DELIMITER}{(i > 0 ? partsLines[keys[i - 1]].x : partsLines[keys[0]].x)}{ANSLUtils.COMPILED_DELIMITER}{(i == keys.Length - 1 ? loopLine : partsLines[keys[i + 1]].x)}{evidenceToShow}");

                compiledLines.AddRange(compiledPress[partId]);
                if (i != keys.Length - 1)
                    compiledLines.Add($"{setExaminationPart}{ANSLUtils.COMPILED_DELIMITER}{keys[i + 1]}");

                compiledLines.Add($"{jumpToFunctionId}{ANSLUtils.COMPILED_DELIMITER}{(i == keys.Length - 1 ? loopLine : outputLine)}");
            }


            string idsToCheck = "";
            for (int i = 0; i < keys.Length; i++)
            {
                if (partsParameters[keys[i]].Length > 0 && partsParameters[keys[i]][0].Equals("press"))
                {
                    idsToCheck += $"{ANSLUtils.COMPILED_DELIMITER}{keys[i]}";
                }
            }

            compiledLines.Add($"{setExaminationPart}{ANSLUtils.COMPILED_DELIMITER}{keys[0]}");
            compiledLines.Add($"{showExaminationUIId}{ANSLUtils.COMPILED_DELIMITER}false");
            compiledLines.Add($"{checkExaminationPressedFunction}{ANSLUtils.COMPILED_DELIMITER}{endLine}{idsToCheck}");
            compiledLines.AddRange(compiledLoop);
            compiledLines.Add($"{jumpToFunctionId}{ANSLUtils.COMPILED_DELIMITER}{outputLine}");

            compiledLines.Add($"{setExaminationPart}{ANSLUtils.COMPILED_DELIMITER}{keys[0]}");
            compiledLines.Add($"{showExaminationUIId}{ANSLUtils.COMPILED_DELIMITER}false");
            compiledLines.AddRange(compiledFail);
            compiledLines.Add($"{jumpToFunctionId}{ANSLUtils.COMPILED_DELIMITER}{outputLine}");

            compiler.CheckLine(sourceFileNextLine);

            return true;
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out uint endLine) &&
                parameters.GetParameter(1, out string[] partsData) &&
                manager.GetGUIManager().GetComponent(out ExaminationUI examinationUI))
            {
                string[] ids = new string[partsData.Length / 3];
                uint[] lines = new uint[partsData.Length / 3];

                List<string> toPress = new List<string>();

                for (int i = 0; i < ids.Length; i++)
                {
                    ids[i] = partsData[3 * i];
                    uint.TryParse(partsData[3 * i + 1], out lines[i]);
                    if (bool.TryParse(partsData[3 * i + 2], out bool shouldPress) && shouldPress)
                        toPress.Add(ids[i]);
                }

                // Check if all pressed
                if (toPress.Count > 0)
                {
                    List<string> pressed = examinationUI.GetPressedIds();
                    bool good = true;

                    foreach (string check in toPress)
                    {
                        if (!pressed.Contains(check))
                        {
                            good = false;
                            break;
                        }
                    }
                    if (good)
                    {
                        examinationUI.SetEnabled(false);
                        EndProcess();
                        context.SetLineCounter(endLine);
                        return;
                    }
                }

                // Set data to UI and find the next Part to load
                examinationUI.SetIds(ids, lines);

                string currentId = examinationUI.GetCurrentId();
                uint line = lines[0];
                if (!string.IsNullOrEmpty(currentId))
                {
                    for (int i = 0; i < ids.Length; i++)
                    {
                        if (ids[i].Equals(currentId))
                        {
                            line = lines[i];
                            break;
                        }
                    }
                }

                EndProcess();
                context.SetLineCounter(line);
                return;
            }
            else
            {
                EndProcess();
            }
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

