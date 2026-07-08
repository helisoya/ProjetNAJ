using ANF.ANSL;
using ANF.Persistent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.VisualScripting;

namespace ANF.Utils
{
    /// <summary>
    /// Contains various utilitary functions
    /// </summary>
    public class ANSLUtils
    {
        #region General

        /// <summary>
        /// Find all ANSL Functions
        /// </summary>
        /// <returns>All ANSL Functions</returns>
        public static List<Type> GetANSLFunctionsList()
        {
            List<Type> output = new List<Type>();

            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (System.Reflection.Assembly assembly in assemblies)
            {
                Type[] assemblyTypes = assembly.GetTypes();

                foreach (Type type in assemblyTypes)
                {
                    if (type.IsDefined(typeof(ANSLFunctionAttribute), false) && type.IsSubclassOf(typeof(ANSLFunction)))
                        output.Add(type);
                }
            }
            return output;
        }

        /// <summary>
        /// Find all Valid ANSL Functions (Registered and Enabled)
        /// </summary>
        /// <returns>All Valid ANSL Functions</returns>
        public static List<KeyValuePair<Type,uint>> GetValidANSLFunctionsList(ANFSettings settings)
        {
            List<KeyValuePair<Type, uint>> output = new List<KeyValuePair<Type, uint>>();


            if(settings.FindAdditionalPart(out ANSLSettings anslSettings))
            {
                foreach(ANSLSettings.ANSLFunctionSettingsData data in anslSettings.registeredFunctions)
                {
                    if(data.enabled)
                    {
                        output.Add(new KeyValuePair<Type, uint>(Type.GetType(data.typeName),data.id));
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Finds the correct template for the specified parameters and create an interface for it
        /// Returns null if none found
        /// </summary>
        /// <param name="parameters">The parameters list</param>
        /// <param name="templates">The templates list</param>
        /// <returns>The interfaced template</returns>
        public static FunctionParameters CreateParametersInterface(string[] parameters, FunctionParameterType[][] templates)
        {
            FunctionParameters parameterInterface = new FunctionParameters();
            for (uint i = 0; i < templates.Length; i++)
            {
                parameterInterface.Clear();
                parameterInterface.Initialize(parameters, templates[i], i);
                if (parameterInterface.IsValid())
                    return parameterInterface;
            }

            return null;
        }

        #endregion

        #region Compilation

        /// <summary>
        /// Represents an error when compiling ANSL files
        /// </summary>
        public struct ANSLError
        {
            public ANSLErrorType type;
            public string filePath;
            public int line;
            public string errorMessage;
        }

        /// <summary>
        /// Error types for ANSL
        /// </summary>
        public enum ANSLErrorType
        {
            WARNING,
            ERROR,
            FUNCTION
        }

        /// <summary>
        /// Resolves the next filepath considering the previous one.
        /// Use / to force an absolute path instead of a relative one.
        /// Ex : Previous(ANF/Test/FileA) & Next(Test2/FileB) -> ANF/Test/Test2/FileB
        /// </summary>
        /// <param name="currentFilepath">The previous filepath</param>
        /// <param name="nextFilePath">The next filepath</param>
        /// <returns></returns>
        public static string ResolveFilePath(string currentFilepath, string nextFilePath)
        {
            if (string.IsNullOrEmpty(nextFilePath))
                return null;

            List<string> parts = new List<string>();
            string[] split;

            if (!string.IsNullOrEmpty(currentFilepath) && !nextFilePath.StartsWith('/'))
            {
                split = currentFilepath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                for(int i = 0; i < split.Length - 1;i++) // Skip last part (actual filename)
                {
                    if (split[i].Equals(".."))
                    {
                        if(parts.Count > 0)
                            parts.RemoveAt(parts.Count - 1);
                        else
                            return null;
                    }
                    else
                    {
                        parts.Add(split[i]);
                    }
                }
            }

            split = nextFilePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].Equals(".."))
                {
                    if (parts.Count > 0)
                        parts.RemoveAt(parts.Count - 1);
                    else
                        return null;
                }
                else
                {
                    parts.Add(split[i]);
                }
            }


            string result = "";
            for(int i = 0; i < parts.Count;i++)
            {
                result += parts[i];
                if (i < parts.Count - 1)
                    result += "/";
            }

            return result;
        }

        /// <summary>
		/// Regenerates the VS Code Snippets
		/// </summary>
        public static void RegenerateVSCodeSnippets(ANFSettings settings)
        {
            if (!settings.FindAdditionalPart(out ANSLSettings anslSettings))
                return;

            string targetFile = anslSettings.anslVSCodeSnippetsPath + "/ANF.code-snippets";

            new FileInfo(targetFile).Directory.Create();

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            StreamWriter outStream = new StreamWriter(targetFile, false);

            List<KeyValuePair<Type,uint>> functions = GetValidANSLFunctionsList(settings);

            outStream.Write("{");

            foreach (KeyValuePair<Type, uint> type in functions)
            {
                ANSLFunctionAttribute attribute = type.Key.GetCustomAttribute<ANSLFunctionAttribute>();

                if (attribute != null && !string.IsNullOrEmpty(attribute.functionBody) && attribute.functionAutoComplete != null)
                {
                    int idx = 0;
                    foreach (string autoComplete in attribute.functionAutoComplete)
                    {
                        outStream.Write($"\n\t\"{type.Value}_{idx}\": {{");
                        outStream.Write($"\n\t\t\"scope\": \"ansl\",");
                        outStream.Write($"\n\t\t\"prefix\": \"{attribute.functionBody}\",");
                        outStream.Write($"\n\t\t\"body\": [\"{autoComplete}\"],");
                        outStream.Write($"\n\t\t\"description\": \"{attribute.functionDesc}\"");
                        outStream.Write($"\n\t}},");
                        idx++;
                    }
                }
            }
            outStream.Close();
        }


        /// <summary>
        /// Compiles all ANSL Files
        /// </summary>
        /// <returns>The error list</returns>
        public static List<ANSLError> CompileAll(ANFSettings settings)
        {
            List<ANSLError> errors = new List<ANSLError>();
            List<KeyValuePair<Type,uint>> functions = GetValidANSLFunctionsList(settings);
            ANSLCompiler compiler = new ANSLCompiler();

            if (!settings.FindAdditionalPart(out ANSLSettings anslSettings))
            {
                errors.Add(new ANSLError()
                {
                    type = ANSLErrorType.FUNCTION,
                    filePath = "Settings",
                    errorMessage = $"ANSLSetings were not found. Did you forget to register it to the ANFSettings ?"
                });
                return errors;
            }

            if (CheckANSLFunctions(functions, errors))
            {
                Dictionary<string, KeyValuePair<ANSLFunction, uint>> functionInstances = new Dictionary<string, KeyValuePair<ANSLFunction, uint>>();
                foreach (KeyValuePair<Type, uint> type in functions)
                {
                    ANSLFunctionAttribute attribute = type.Key.GetCustomAttribute<ANSLFunctionAttribute>();
                    if (attribute != null)
                    {
                        functionInstances.Add(attribute.functionBody, new KeyValuePair<ANSLFunction,uint>((ANSLFunction)type.Key.Instantiate(),type.Value));
                    }
                }

                // Compile Defines
                Stack<string> directories = new Stack<string>();
                directories.Push(anslSettings.anslSourceFolder);

                while (directories.Count > 0)
                {
                    string directory = directories.Pop();

                    foreach (string subDir in Directory.GetDirectories(directory))
                        directories.Push(subDir);

                    foreach (string file in Directory.GetFiles(directory))
                    {
                        if (file.EndsWith(".defines"))
                            compiler.CompileANSLMacros(file, errors);
                    }
                }

                if (errors.Count > 0)
                    return errors;

                // Compile regular files

                directories.Clear();
                directories.Push(anslSettings.anslSourceFolder);

                while (directories.Count > 0)
                {
                    string directory = directories.Pop();

                    foreach (string subDir in Directory.GetDirectories(directory))
                        directories.Push(subDir);

                    foreach (string file in Directory.GetFiles(directory))
                    {
                        if (file.EndsWith(".ansl"))
                        {
                            string destPath = "Assets/Resources/" + anslSettings.anslDestinationFolder + file.Substring(anslSettings.anslSourceFolder.Length).Replace(".ansl", ".txt");
                            compiler.Compile(file, destPath, functionInstances, errors);
                        }
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// Checks the ANSL Functions for errors
        /// </summary>
        /// <param name="types">The functions list</param>
        /// <param name="errors">The global error list</param>
        /// <returns>True if no errors were found</returns>
        private static bool CheckANSLFunctions(List<KeyValuePair<Type, uint>> functions, List<ANSLError> errors)
        {
            List<uint> usedIds = new List<uint>();

            bool errorFound = false;

            foreach (KeyValuePair<Type, uint> function in functions)
            {
                ANSLFunctionAttribute attribute = function.Key.GetCustomAttribute<ANSLFunctionAttribute>(false);
                if (attribute != null)
                {
                    if (usedIds.Contains(function.Value))
                    {
                        errors.Add(new ANSLError()
                        {
                            type = ANSLErrorType.FUNCTION,
                            filePath = function.Key.FullName,
                            errorMessage = $"Id {function.Value} is already used by another function."
                        });
                        errorFound = true;
                    }
                    else
                    {
                        usedIds.Add(function.Value);
                    }
                }
                else
                {
                    errors.Add(new ANSLError()
                    {
                        type = ANSLErrorType.FUNCTION,
                        filePath = function.Key.FullName,
                        errorMessage = $"Failed to retrieve {function.Key.FullName}'s class Attribute."
                    });
                    errorFound = true;
                }
            }
            return !errorFound;
        }
        #endregion
    }
}

