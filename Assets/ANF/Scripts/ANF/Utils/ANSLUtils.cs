using ANF.ANSL;
using ANF.Persistent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.VisualScripting;
using System.Security.Cryptography;
using System.Text;


#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor;
#endif

namespace ANF.Utils
{
    /// <summary>
    /// Contains various utilitary functions
    /// </summary>
    public class ANSLUtils
    {
        public const char SOURCE_DELIMITER = ';';
        public const char COMPILED_DELIMITER = (char)31;

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
        public static List<KeyValuePair<Type, uint>> GetValidANSLFunctionsList(ANFSettings settings)
        {
            List<KeyValuePair<Type, uint>> output = new List<KeyValuePair<Type, uint>>();


            if (settings.FindAdditionalPart(out ANSLSettings anslSettings))
            {
                foreach (ANSLSettings.ANSLFunctionSettingsData data in anslSettings.registeredFunctions)
                {
                    if (data.enabled)
                    {
                        output.Add(new KeyValuePair<Type, uint>(Type.GetType(data.typeName), data.id));
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
		/// Represents base stats for ANSL Compiling
		/// </summary>
        public struct ANSLCompileStats
        {
            public int compilationGood;
            public int compilationFailed;
            public int compilationSkipped;
            public List<ANSLError> errors;
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
        /// Generate a file's checksum
        /// </summary>
        /// <param name="filename">The filename</param>
        /// <returns>The file's checksum</returns>
        public static string GenerateCheckSum(string filename)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(filename))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }

        /// <summary>
        /// Generate a list of string's checksum
        /// </summary>
        /// <param name="list">The list</param>
        /// <returns>The file's checksum</returns>
        public static string GenerateCheckSum(List<string> list)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
                    {
                        foreach (string line in list)
                            writer.Write(line);
                    }

                    stream.Position = 0;

                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }


        /// <summary>
		/// Generate a function list's hash
		/// </summary>
		/// <param name="functions">The function list</param>
		/// <returns>The function list's checksum</returns>
        public static string GenerateFunctionHash(List<KeyValuePair<Type, uint>> functions)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
                    {
                        foreach (KeyValuePair<Type, uint> pair in functions)
                        {
                            writer.Write($"-{pair.Value}-{pair.Key.FullName}-");
                        }
                    }

                    stream.Position = 0;

                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }

        /// <summary>
        /// Resolves the next filepath considering the previous one.
        /// Use / to force an absolute path instead of a relative one.
        /// Ex : Previous(ANF/Test/FileA) & Next(Test2/FileB) -> ANF/Test/Test2/FileB
        /// </summary>
        /// <param name="currentFilepath">The previous filepath</param>
        /// <param name="nextFilePath">The next filepath</param>
        /// <returns>The correct filepath</returns>
        public static string ResolveFilePath(string currentFilepath, string nextFilePath)
        {
            if (string.IsNullOrEmpty(nextFilePath))
                return null;

            List<string> parts = new List<string>();
            string[] split;

            if (!string.IsNullOrEmpty(currentFilepath) && !nextFilePath.StartsWith('/'))
            {
                split = currentFilepath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < split.Length - 1; i++) // Skip last part (actual filename)
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
            for (int i = 0; i < parts.Count; i++)
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

            // Find Macros
            List<KeyValuePair<string, string>> macros = new List<KeyValuePair<string, string>>();
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
                    {
                        string[] lines = File.ReadAllLines(file);
                        if (lines != null)
                        {

                        }
                        foreach (string line in lines)
                        {
                            if (line.StartsWith('#') || string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                                continue;

                            if (line.StartsWith("Define "))
                            {
                                string cleanedLine = line.Substring("Define ".Length).Replace(" ", "").Replace("\t", "");
                                int idxStart = cleanedLine.IndexOf('(');
                                int idxEnd = cleanedLine.IndexOf(')');


                                if (idxStart != -1 && idxEnd != -1 && idxStart > 0 && idxEnd == cleanedLine.Length - 1)
                                {
                                    macros.Add(new KeyValuePair<string, string>(
                                        $":{cleanedLine.Substring(0, idxStart)}",
                                        $":{cleanedLine}"
                                    ));
                                }
                            }
                        }
                    }
                }
            }


            string targetFile = anslSettings.anslVSCodeSnippetsPath + "/ANF.code-snippets";

            new FileInfo(targetFile).Directory.Create();

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            StreamWriter outStream = new StreamWriter(targetFile, false);

            List<KeyValuePair<Type, uint>> functions = GetValidANSLFunctionsList(settings);

            outStream.Write("{");
            int idx;
            foreach (KeyValuePair<Type, uint> type in functions)
            {
                ANSLFunctionAttribute attribute = type.Key.GetCustomAttribute<ANSLFunctionAttribute>();

                if (attribute != null && !string.IsNullOrEmpty(attribute.functionBody) && attribute.functionAutoComplete != null)
                {
                    idx = 0;
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

            idx = 0;
            foreach (KeyValuePair<string, string> pair in macros)
            {
                outStream.Write($"\n\t\"Macro_{idx}\": {{");
                outStream.Write($"\n\t\t\"scope\": \"ansl\",");
                outStream.Write($"\n\t\t\"prefix\": \"{pair.Key}\",");
                outStream.Write($"\n\t\t\"body\": [\"{pair.Value}\"],");
                outStream.Write($"\n\t\t\"description\": \"This is a user generated macro\"");
                outStream.Write($"\n\t}},");
                idx++;
            }
            outStream.Close();
        }


        /// <summary>
        /// Compiles all ANSL Files
        /// </summary>
        /// <param name="ignoreChecksum">True if the checksum should be ignored</param>
        /// <returns>The result</returns>
        public static ANSLCompileStats CompileAll(ANFSettings settings, bool ignoreChecksum = false)
        {
            ANSLCompileStats stats = new ANSLCompileStats()
            {
                compilationFailed = 0,
                compilationGood = 0,
                compilationSkipped = 0,
                errors = new List<ANSLError>()
            };

#if UNITY_EDITOR
            List<KeyValuePair<Type, uint>> functions = GetValidANSLFunctionsList(settings);
            string functionsChecksum = GenerateFunctionHash(functions);
            ANSLCompiler compiler = new ANSLCompiler();

            if (!settings.FindAdditionalPart(out ANSLSettings anslSettings))
            {
                stats.errors.Add(new ANSLError()
                {
                    type = ANSLErrorType.FUNCTION,
                    filePath = "Settings",
                    errorMessage = $"ANSLSetings were not found. Did you forget to register it to the ANFSettings ?"
                });
                return stats;
            }

            int totalProgress = 1;
            int currentProgress = 0;
            List<KeyValuePair<string, string>> anslFiles = new List<KeyValuePair<string, string>>();
            List<string> definesFiles = new List<string>();

            Stack<string> directories = new Stack<string>();
            directories.Push(anslSettings.anslSourceFolder);

            int directoryID = 0;
            bool hasANSLFiles;

            while (directories.Count > 0)
            {
                hasANSLFiles = false;
                string directory = directories.Pop();
                string directoryName = new DirectoryInfo(directory).Name;

                foreach (string subDir in Directory.GetDirectories(directory))
                    directories.Push(subDir);

                foreach (string file in Directory.GetFiles(directory))
                {
                    if (file.EndsWith(".defines"))
                        definesFiles.Add(file);
                    else if (file.EndsWith(".ansl"))
                    {
                        hasANSLFiles = true;
                        anslFiles.Add(new KeyValuePair<string, string>(file, $"{directoryID}_{directoryName}"));
                    }
                }

                if (hasANSLFiles)
                    directoryID++;
            }

            totalProgress += anslFiles.Count + definesFiles.Count;


            EditorUtility.DisplayProgressBar("ANSL Compilation", "Checking Functions", 0.0f);

            if (CheckANSLFunctions(functions, stats.errors))
            {
                List<KeyValuePair<ANSLFunction, uint>> functionInstances = new List<KeyValuePair<ANSLFunction, uint>>();
                foreach (KeyValuePair<Type, uint> type in functions)
                {
                    ANSLFunctionAttribute attribute = type.Key.GetCustomAttribute<ANSLFunctionAttribute>();
                    if (attribute != null)
                    {
                        functionInstances.Add(new KeyValuePair<ANSLFunction, uint>((ANSLFunction)type.Key.Instantiate(), type.Value));

                    }
                }

                // Compile Defines
                foreach (string file in definesFiles)
                {
                    currentProgress++;
                    EditorUtility.DisplayProgressBar("ANSL Compilation", file, (float)currentProgress / totalProgress);
                    compiler.CompileANSLMacros(file, stats.errors);
                }

                if (stats.errors.Count > 0)
                {
                    EditorUtility.ClearProgressBar();
                    return stats;
                }

                AddressableAssetSettings addressableSettings = AddressableAssetSettingsDefaultObject.Settings;

                // Compile regular files
                foreach (var file in anslFiles)
                {
                    currentProgress++;
                    EditorUtility.DisplayProgressBar("ANSL Compilation", file.Key, (float)currentProgress / totalProgress);
                    string destPath = anslSettings.anslDestinationFolder + file.Key.Substring(anslSettings.anslSourceFolder.Length).Replace(".ansl", ".txt");
                    string resourcePath = (anslSettings.anslResourcePath + file.Key.Substring(anslSettings.anslSourceFolder.Length).Replace(".ansl", "")).Replace('\\', '/').Replace("//", "/");
                    ANSLCompiler.ResultType result = compiler.Compile(file.Key, destPath, functionInstances, stats.errors, functionsChecksum, ignoreChecksum);

                    if (result == ANSLCompiler.ResultType.Success)
                        stats.compilationGood++;
                    else if (result == ANSLCompiler.ResultType.Failure)
                        stats.compilationFailed++;
                    else if (result == ANSLCompiler.ResultType.SameFile)
                        stats.compilationSkipped++;

                    if (result == ANSLCompiler.ResultType.Success && anslSettings.linkScriptsToAddressables)
                    {
                        AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceSynchronousImport);
                        AddressableAssetGroup group = addressableSettings.FindGroup($"AUTO_ANSL_SCRIPT_{file.Value}");

                        if (!group)
                        {
                            group = addressableSettings.CreateGroup($"AUTO_ANSL_SCRIPT_{file.Value}", false, false, false,
                            null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
                        }

                        AddressableAssetEntry entry = addressableSettings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(destPath), group);
                        entry.SetAddress(resourcePath);
                        entry.SetLabel(anslSettings.addressablesLabel, true);
                    }
                }

                EditorUtility.SetDirty(addressableSettings);
            }
            EditorUtility.ClearProgressBar();
#endif

            return stats;
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

