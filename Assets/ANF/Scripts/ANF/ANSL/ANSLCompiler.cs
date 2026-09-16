using ANF.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using static ANF.Utils.ANSLUtils;

namespace ANF.ANSL
{
    /// <summary>
    /// Handles the compiling of an ANSL file
    /// </summary>
    public class ANSLCompiler
    {
        public enum ResultType
        {
            Success,
            SameFile,
            Failure
        }


        private string sourceFilepath;
        private bool isfirstLine;

        private int currentLineInOutput;
        private int currentLine;
        private string cachedCurrentLine;
        private string cachedCurrentLineClean;

        private StreamWriter outStream;
        private List<string> inLines;
        private List<KeyValuePair<ANSLFunction, uint>> functions;
        private List<ANSLUtils.ANSLError> errors;
        private List<ANSLMacroData> macros;

        /// <summary>
        /// Gets the id of a registered function
        /// </summary>
        /// <typeparam name="T">The function's type</typeparam>
        /// <returns>The function's id</returns>
        public uint GetRegisteredFunctionId<T>()
        {
            foreach (KeyValuePair<ANSLFunction, uint> pair in functions)
            {
                if (pair.Key.GetType() == typeof(T))
                    return pair.Value;
            }

            return 0;
        }

        /// <summary>
        /// Compiles a file composed of ANSL macros
        /// </summary>
        /// <param name="sourceFile">The source file</param>
        /// <param name="errors">The error list</param>
        /// <returns>True if there was no errors</returns>
        public bool CompileANSLMacros(string sourceFile, List<ANSLUtils.ANSLError> errors)
        {
            if (macros == null)
                macros = new List<ANSLMacroData>();

            if (!File.Exists(sourceFile))
            {
                errors.Add(new ANSLError()
                {
                    type = ANSLErrorType.WARNING,
                    filePath = sourceFile,
                    line = 0,
                    errorMessage = $"Could not open file ANSL Macro file (not required)"
                });
                return false;
            }

            string[] inLines = File.ReadAllLines(sourceFile);

            if (inLines == null)
            {
                errors.Add(new ANSLError()
                {
                    type = ANSLErrorType.ERROR,
                    filePath = sourceFile,
                    line = 0,
                    errorMessage = $"Could not open file."
                });
                return false;
            }

            ANSLMacroData macroData = null;
            string currentLine;


            for (int i = 0; i < inLines.Length; i++)
            {
                currentLine = inLines[i];
                if (currentLine.StartsWith('#') || string.IsNullOrEmpty(currentLine) || string.IsNullOrWhiteSpace(currentLine))
                    continue;

                if (currentLine.StartsWith("Define "))
                {
                    if (macroData != null)
                    {
                        errors.Add(new ANSLError()
                        {
                            type = ANSLErrorType.ERROR,
                            filePath = sourceFile,
                            line = i,
                            errorMessage = $"Cannot declare a macro inside a macro. EndDefine missing ?"
                        });
                        return false;
                    }

                    currentLine = currentLine.Substring("Define ".Length).Replace(" ", "").Replace("\t", "");
                    int idxStart = currentLine.IndexOf('(');
                    int idxEnd = currentLine.IndexOf(')');


                    if (idxStart == -1 || idxEnd == -1)
                    {
                        errors.Add(new ANSLError()
                        {
                            type = ANSLErrorType.ERROR,
                            filePath = sourceFile,
                            line = i,
                            errorMessage = $"Missing () in macro declaration"
                        });
                    }

                    if (idxEnd != currentLine.Length - 1)
                    {
                        errors.Add(new ANSLError()
                        {
                            type = ANSLErrorType.ERROR,
                            filePath = sourceFile,
                            line = i,
                            errorMessage = $"Unknown character at the end of line"
                        });
                    }

                    if (idxStart == 0)
                    {
                        errors.Add(new ANSLError()
                        {
                            type = ANSLErrorType.ERROR,
                            filePath = sourceFile,
                            line = i,
                            errorMessage = $"No Macro name detected"
                        });
                    }

                    macroData = new ANSLMacroData(currentLine.Substring(0, idxStart));

                    if (idxStart + 1 != idxEnd)
                    {
                        string[] parameterSplit = currentLine.Substring(idxStart + 1, currentLine.Length - idxStart - 2).Split(';');

                        if (parameterSplit != null || parameterSplit.Length > 0)
                        {
                            foreach (string parameter in parameterSplit)
                                if (!string.IsNullOrEmpty(parameter))
                                    macroData.parameters.Add(parameter);
                        }
                    }
                }
                else if (currentLine.StartsWith("EndDefine"))
                {
                    if (macroData == null)
                    {
                        errors.Add(new ANSLError()
                        {
                            type = ANSLErrorType.ERROR,
                            filePath = sourceFile,
                            line = i,
                            errorMessage = $"Missing macro declaration"
                        });
                    }

                    macros.Add(macroData);
                    macroData = null;
                }
                else
                {
                    if (macroData != null)
                    {
                        macroData.content.Add(currentLine);
                    }
                    else
                    {
                        errors.Add(new ANSLError()
                        {
                            type = ANSLErrorType.ERROR,
                            filePath = sourceFile,
                            line = i,
                            errorMessage = $"Out of place function"
                        });
                        return false;
                    }
                }
            }

            if (macroData != null)
            {
                errors.Add(new ANSLError()
                {
                    type = ANSLErrorType.ERROR,
                    filePath = sourceFile,
                    line = 0,
                    errorMessage = $"Missing EndDefine detected"
                });
                return false;
            }

            return true;
        }

        /// <summary>
        /// Starts compiling a new ANSL file
        /// </summary>
        /// <param name="sourceFile">The source file</param>
        /// <param name="destinationFile">The destination file</param>
        /// <param name="functions">The function list</param>
        /// <param name="errors">The global error list</param>
        /// <param name="functionsChecksum">The function list's checksum</param>
        /// <param name="ignoreChecksum">True if the checksum should be ignored</param>
        public ResultType Compile(string sourceFile, string destinationFile, List<KeyValuePair<ANSLFunction, uint>> functions,
            List<ANSLUtils.ANSLError> errors, string functionsChecksum, bool ignoreChecksum = false)
        {
            sourceFilepath = sourceFile;
            this.errors = errors;
            this.functions = functions;
            currentLine = -1;
            currentLineInOutput = 0;
            isfirstLine = true;

            inLines = new List<string>(File.ReadAllLines(sourceFile));

            if (inLines == null)
            {
                errors.Add(new ANSLError()
                {
                    type = ANSLErrorType.ERROR,
                    filePath = sourceFile,
                    line = 0,
                    errorMessage = $"Could not open file."
                });
                return ResultType.Failure;
            }

            for (int i = 0; i < inLines.Count; i++)
            {
                string line = inLines[i].Replace("\t", "").Replace(" ", "");

                if (!string.IsNullOrEmpty(line) && line.StartsWith(':'))
                {
                    line = line.Substring(1);
                    int startIdx = line.IndexOf('(');
                    int endIdx = line.IndexOf(')');

                    if (startIdx <= 0 || endIdx == -1 || endIdx != line.Length - 1)
                    {
                        errors.Add(new ANSLError()
                        {
                            type = ANSLErrorType.ERROR,
                            filePath = sourceFile,
                            line = i,
                            errorMessage = $"Bad macro usage : {line}."
                        });
                        return ResultType.Failure;
                    }

                    string name = line.Substring(0, startIdx);

                    foreach (ANSLMacroData macroData in macros)
                    {
                        if (name.Equals(macroData.name))
                        {
                            string[] parameters = new string[] { };

                            if (startIdx + 1 != endIdx)
                                parameters = line.Substring(startIdx + 1, line.Length - startIdx - 2).Split(';');

                            if (macroData.parameters.Count != parameters.Length)
                            {
                                errors.Add(new ANSLError()
                                {
                                    type = ANSLErrorType.ERROR,
                                    filePath = sourceFile,
                                    line = i,
                                    errorMessage = $"Unknown parameters count : {line}."
                                });
                                return ResultType.Failure;
                            }
                            int newLineIndex = i;
                            foreach (string macroLine in macroData.content)
                            {
                                string processedLine = new string(macroLine);

                                for (int j = 0; j < parameters.Length; j++)
                                {
                                    if (string.IsNullOrEmpty(macroData.parameters[j]))
                                        continue;

                                    processedLine = processedLine.Replace($"{{{macroData.parameters[j]}}}", parameters[j]);
                                }

                                inLines.Insert(newLineIndex, processedLine);
                                newLineIndex++;
                            }
                            inLines.RemoveAt(newLineIndex);

                            continue;
                        }
                    }
                }
            }

            string newChecksum = ANSLUtils.GenerateCheckSum(inLines) + "-" + functionsChecksum;

            // Check existing checksum for this file
            // Note that any change in function indexes will bypass the checksum
            // In that case, checksum should be ignored for at least one compilation
            if (!ignoreChecksum && File.Exists(destinationFile))
            {
                using (StreamReader stream = new StreamReader(destinationFile))
                {
                    string firstLine = stream.ReadLine() ?? null;

                    if (firstLine != null && firstLine.StartsWith("#") && firstLine.Length > 0)
                    {
                        string checksum = firstLine.Substring(1);
                        if (checksum.Equals(newChecksum))
                            return ResultType.SameFile;
                    }
                }
            }

            new FileInfo(destinationFile).Directory.Create();

            if (File.Exists(destinationFile))
                File.Delete(destinationFile);

            outStream = new StreamWriter(destinationFile, false);

            outStream.WriteLine($"#{newChecksum}");

            CheckNextLine();

            bool result = ContinueCompiling();

            if (result)
                return ResultType.Success;

            if (File.Exists(destinationFile))
                File.Delete(destinationFile);

            return ResultType.Failure;
        }

        /// <summary>
        /// Continues the compiling process
        /// </summary>
		/// <returns>True if the compiling failed</returns>
        private bool ContinueCompiling()
        {
            if (currentLine >= inLines.Count)
            {
                // End of File
                Clean();
                return true;
            }
            else
            {
                if (CompileLine(cachedCurrentLineClean, out List<string> compiledLines, currentLineInOutput))
                {
                    foreach (string line in compiledLines)
                    {
                        if (isfirstLine)
                            isfirstLine = false;
                        else
                            outStream.Write("\n");

                        outStream.Write(line);
                    }
                    currentLineInOutput += compiledLines.Count;
                }
                else
                {
                    Clean();
                    return false;
                }

                CheckNextLine();
                return ContinueCompiling();
            }
        }

        /// <summary>
		/// Compiles a specific line
		/// </summary>
		/// <param name="line">The line counter</param>
		/// <param name="compiledLines">The compiled lines of code</param>
        /// <param name="outputLine">The outline line the function will start in</param>
		/// <returns>True if the compiling resulted in success</returns>
        public bool CompileLine(string line, out List<string> compiledLines, int outputLine)
        {
            compiledLines = new List<string>();
            if (!string.IsNullOrEmpty(line) && !string.IsNullOrWhiteSpace(line))
            {
                bool found = false;
                foreach (KeyValuePair<ANSLFunction, uint> pair in functions)
                {
                    string body = pair.Key.GetType().GetCustomAttribute<ANSLFunctionAttribute>().functionBody;

                    if (string.IsNullOrEmpty(body)) // Skip undefined functions
                        continue;

                    if (line.StartsWith(body))
                    {
                        found = true;

                        // Compile with this function
                        if (pair.Key.Compile(out List<string> result, line, pair.Value, this, errors, outputLine))
                        {
                            foreach (string compiledLine in result)
                            {
                                compiledLines.Add(compiledLine);
                            }
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                if (!found)
                {
                    errors.Add(new ANSLError()
                    {
                        type = ANSLErrorType.WARNING,
                        filePath = sourceFilepath,
                        line = currentLine,
                        errorMessage = $"Unknown function : {cachedCurrentLineClean}."
                    });
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks the next line and caches a version without tabs and spaces at the start
        /// </summary>
        public void CheckNextLine()
        {
            currentLine++;
            if (currentLine >= inLines.Count)
            {
                // End of file
                cachedCurrentLine = "EOF";
                cachedCurrentLineClean = "EOF";
            }
            else
            {
                cachedCurrentLine = inLines[currentLine];
                cachedCurrentLineClean = cachedCurrentLine.Replace("\t", "");
                while (cachedCurrentLineClean.StartsWith(" ") && cachedCurrentLineClean.Length > 0)
                    cachedCurrentLineClean = cachedCurrentLineClean.Substring(1);
                while (cachedCurrentLineClean.EndsWith(" ") && cachedCurrentLineClean.Length > 0)
                    cachedCurrentLineClean = cachedCurrentLineClean.Substring(0, cachedCurrentLine.Length - 1);

                if (cachedCurrentLineClean.StartsWith('#'))
                {
                    cachedCurrentLineClean = null;
                    cachedCurrentLine = null;
                }
            }
        }

        /// <summary>
        /// Checks the previous line and caches a version without tabs and spaces at the start
        /// </summary>
        public void CheckPreviousLine()
        {
            currentLine++;
            if (currentLine < 0)
                currentLine = 0;

            cachedCurrentLine = inLines[currentLine];
            cachedCurrentLineClean = cachedCurrentLine.Replace("\t", "");
            while (cachedCurrentLineClean.StartsWith(" ") && cachedCurrentLineClean.Length > 0)
                cachedCurrentLine = cachedCurrentLineClean.Substring(1);
            while (cachedCurrentLineClean.EndsWith(" ") && cachedCurrentLineClean.Length > 0)
                cachedCurrentLineClean = cachedCurrentLineClean.Substring(0, cachedCurrentLine.Length - 1);
        }

        /// <summary>
        /// Removes the first character from the cleaned line
        /// </summary>
        public void RemoveFirstCharacterFromCleanedLine()
        {
            cachedCurrentLineClean = cachedCurrentLineClean.Substring(1);
        }

        /// <summary>
        /// Gets the current line counter in the output file
        /// </summary>
        /// <returns>The line counter</returns>
        public int GetCurrentOutputLineCounter()
        {
            return currentLineInOutput;
        }

        /// <summary>
		/// Returns the cached current line
		/// </summary>
		/// <returns>The current line</returns>
        public string GetCurrentLine()
        {
            return cachedCurrentLine;
        }

        /// <summary>
		/// Returns the cleaned cached current line (Without tabs and white spaces at the start)
		/// </summary>
		/// <returns>The cleaned cached current line</returns>
        public string GetCurrentLineClean()
        {
            return cachedCurrentLineClean;
        }

        /// <summary>
		/// Gets the source filepath
		/// </summary>
		/// <returns>The source filepath</returns>
        public string GetSourceFilepath()
        {
            return sourceFilepath;
        }

        /// <summary>
		/// Gets the current line counter
		/// </summary>
		/// <returns>The current line counter</returns>
        public int GetCurrentLineCounter()
        {
            return currentLine;
        }

        /// <summary>
        /// Cleans the compiler
        /// </summary>
        public void Clean()
        {
            if (outStream != null)
            {
                outStream.Close();
                outStream = null;
            }

            inLines = null;
        }
    }

    /// <summary>
    /// Represents the data of an ANSL macro
    /// </summary>
    public class ANSLMacroData
    {
        public string name;
        public List<string> parameters;
        public List<string> content;

        public ANSLMacroData(string name)
        {
            this.name = name;
            this.parameters = new List<string>();
            this.content = new List<string>();
        }
    }
}

