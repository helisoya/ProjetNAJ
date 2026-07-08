using ANF.ANSL;
using ANF.Persistent;
using ANF.Scene;
using ANF.Utils;
using Leguar.TotalJSON;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the flow of a specific script.
/// A context can change scripts dynamically without having to recreate a new one
/// </summary>
public class ANSLContext : Jsonable
{
    public bool isRunning { get; private set; }
    public bool isPaused { get; private set; }

    private Dictionary<uint, ANSLFunction> functions;
    private List<ANSLContextStackEntry> scriptStack;

    private uint contextStackLength;
    private uint maxFunctionsPerFrame;

    private string[] currentScript;
    private uint currentLine;
    private string currentFilePath;

    private bool lastFunctionModifiedLine;
    private bool waitingForFunction;
    private uint currentFunctionId;
    private uint currentFunctionDepth;

    private uint ignoreCheckDepthInternalValue;
    private bool ignoreDepthCheck { get { return ignoreCheckDepthInternalValue != 0; } }

    private bool skipModeEnabled;
    private bool autoplayEnabled;
    private ANFManager manager;
    private ANSLSettings settings;

    public ANSLContext(Dictionary<uint, ANSLFunction> functions, uint contextStackLength, uint maxFunctionsPerFrame, ANFManager manager)
    {
        PersistentDataManager.instance.GetANFSettings().FindAdditionalPart(out settings);

        this.contextStackLength = contextStackLength;
        this.maxFunctionsPerFrame = maxFunctionsPerFrame;
        this.functions = functions;
        this.manager = manager;
        isRunning = false;
        isPaused = false;
        scriptStack = new List<ANSLContextStackEntry>();

        foreach (ANSLFunction function in functions.Values)
            function.Initialize(this, manager);
    }

    /// <summary>
    /// Gets the current script's filepath
    /// </summary>
    /// <param name="cleaned">True if the ANSL directory should be removed from the path (start of path)</param>
    /// <returns>The current filepath</returns>
    public string GetCurrentFilepath(bool cleaned = true)
    {
        if(cleaned)
            return currentFilePath.Substring(settings != null ? settings.anslDestinationFolder.Length : 0);
        else
            return currentFilePath;
    }

    /// <summary>
	/// Enables the autoplay for this context
	/// </summary>
	/// <param name="enabled">True if enabled</param>
    public void SetAutoPlay(bool enabled)
    {
        if (autoplayEnabled != enabled)
        {
            autoplayEnabled = enabled;

            if (isRunning && waitingForFunction)
            {
                functions[currentFunctionId].Invoke("OnAutoPlayToggle", autoplayEnabled);
            }
        }

    }

    /// <summary>
    /// Enables the skip mode for this context
    /// </summary>
    /// <param name="enabled">True if enabled</param>
    public void SetSkipMode(bool enabled)
    {
        if (skipModeEnabled != enabled)
        {
            skipModeEnabled = enabled;

            if (isRunning && waitingForFunction)
            {
                functions[currentFunctionId].Invoke("OnSkipModeToggle", skipModeEnabled);
            }
        }
    }

    /// <summary>
    /// Loads a new script file
    /// </summary>
    /// <param name="scriptFilePath">The script's filepath (Local path inside the designated resource folder) </param>
    /// <param name="startLine">The starting line. 0 by default.</param>
    /// <param name="canAddPreviousToStack">True if the previous script can be added to the stack</param>
    /// <param name="isFullPath">True if the path is already the full path to the script<param>
    public void LoadScript(string scriptFilePath, uint startLine = 0, bool canAddPreviousToStack = true, bool isFullPath = false)
    {
        string fullPath = (isFullPath || settings == null ? "" : settings.anslDestinationFolder) +
            scriptFilePath;

        TextAsset data = Resources.Load<TextAsset>(fullPath);
        if (data)
        {
            // Add current script to stack if it isn't finished
            if (currentScript != null && canAddPreviousToStack)
            {
                while (scriptStack.Count >= contextStackLength) // Clear excess entries
                {
                    scriptStack.RemoveAt(0);
                }
                scriptStack.Add(new ANSLContextStackEntry() { filePath = currentFilePath, lineCounter = currentLine + 1 });
            }

            currentFilePath = fullPath;
            currentScript = FileManager.ReadTextAsset(data).ToArray();
            lastFunctionModifiedLine = true;
            currentLine = startLine;

            isRunning = true;
        }
        else
        {
            Debug.LogError($"File not found : {fullPath}");
            // No file, do something ?
            StopContext();
        }
    }

    /// <summary>
    /// Process the next line
    /// </summary>
    public void NextLine()
    {
        if (isPaused || !isRunning)
            return;

        currentFunctionDepth++;
        if (currentFunctionDepth > maxFunctionsPerFrame && !ignoreDepthCheck)
            return;

        if (!lastFunctionModifiedLine)
            currentLine++;

        lastFunctionModifiedLine = false;

        if (currentLine < currentScript.Length)
        {
            string[] split = currentScript[currentLine].Split('|', 2);
            uint functionId;

            if (split.Length == 0 || string.IsNullOrEmpty(currentScript[currentLine]) ||
                !uint.TryParse(split[0], out functionId) || !functions.ContainsKey(functionId))
            {
                Debug.LogError($"Could not parse/find function for : {currentScript[currentLine]}");
                // Could not parse/find function
                NextLine();
                return;
            }
            else
            {
                FunctionParameters parameters = ANSLUtils.CreateParametersInterface(split.Length > 1 ? split[1].Split('|') : null, functions[functionId].GetParametersTemplates());

                if (parameters == null)
                {
                    Debug.LogError($"Could not parse parameters for : {currentScript[currentLine]}");
                    // Parameters couldn't be parsed
                    NextLine();
                    return;
                }

                // Start the new function
                functions[functionId].StartProcess(parameters);

                if (functions[functionId].isProcessing)
                {
                    // Needs to wait until the function is finished
                    currentFunctionId = functionId;
                    waitingForFunction = true;
                    functions[currentFunctionId].Invoke("OnAutoPlayToggle", autoplayEnabled);
                    functions[currentFunctionId].Invoke("OnSkipModeToggle", skipModeEnabled);
                }
                else
                {
                    // Function is already over
                    NextLine();
                    return;
                }
            }
        }
        else
        {
            // No more lines in script

            if (scriptStack.Count == 0)
                StopContext();
            else
            {
                ANSLContextStackEntry entry = scriptStack[scriptStack.Count - 1];
                scriptStack.RemoveAt(scriptStack.Count - 1);
                LoadScript(entry.filePath, entry.lineCounter, false, true);
            }
        }
    }

    /// <summary>
    /// Updates the context
    /// </summary>
    public void Update()
    {
        if (isPaused)
            return;

        // Updates the current function if it isn't finished yet
        if (waitingForFunction && functions.ContainsKey(currentFunctionId))
        {
            functions[currentFunctionId].Update();
            if (!functions[currentFunctionId].isProcessing)
                waitingForFunction = false;
        }

        if (!waitingForFunction)
        {
            // Start next batch of lines 
            currentFunctionDepth = 0;
            NextLine();
        }
    }

    /// <summary>
    /// Changes the current line counter.
    /// This will disable the auto increment at the end of the next function
    /// </summary>
    /// <param name="value">The new line counter</param>
    public void SetLineCounter(uint value)
    {
        currentLine = value;
        lastFunctionModifiedLine = true;
    }

    /// <summary>
	/// Clears the script stack
	/// </summary>
    public void ClearStack()
    {
        scriptStack.Clear();
    }

    /// <summary>
    /// Stops the context
    /// </summary>
    public void StopContext()
    {
        isRunning = false;
        currentScript = null;
    }

    /// <summary>
    /// Pauses the context
    /// </summary>
    /// <param name="paused">True if paused</param>
    public void PauseContext(bool paused)
    {
        isPaused = paused;
    }

    /// <summary>
	/// Changes if the depth check (how many functions per frame) is enabled.
    /// Function depth is still incremented if disabled, and reenabling the depth check may result in a direct cooldown
    /// This internaly works as a stack. (If you disable depth check two times, you'll need to enable it two times too)
	/// </summary>
	/// <param name="enabled">True if enabled</param>
    public void SetDepthCheckEnabled(bool enabled)
    {
        if (enabled)
            ignoreCheckDepthInternalValue--;
        else
            ignoreCheckDepthInternalValue++;
    }

    /// <summary>
	/// Cleanup the context.
    /// Used in rare case. (Ex : When changing scenes)
	/// </summary>
    public void Cleanup()
    {
        if (waitingForFunction && functions.ContainsKey(currentFunctionId))
        {
            functions[currentFunctionId].Cleanup();
        }
    }


    #region Jsonable

    public void Save(JSON json)
    {
        json.Add("isRunning", isRunning);
        json.Add("isPaused", isPaused);

        json.Add("ignoreCheckDepthInternalValue", ignoreCheckDepthInternalValue);

        json.Add("currentLine", currentLine);
        json.Add("currentFilePath", currentFilePath);

        json.Add("lastFunctionModifiedLine", lastFunctionModifiedLine);
        json.Add("waitingForFunction", waitingForFunction);
        json.Add("currentFunctionId", currentFunctionId);
        json.Add("currentFunctionDepth", currentFunctionDepth);

        JArray stackArray = new JArray();
        JSON stackEntry;
        foreach (ANSLContextStackEntry entry in scriptStack)
        {
            stackEntry = new JSON();
            stackEntry.Add("filePath", entry.filePath);
            stackEntry.Add("lineCounter", entry.lineCounter);
        }
        json.Add("scriptStack", stackArray);

        if (isRunning && waitingForFunction && functions.ContainsKey(currentFunctionId))
        {
            JSON currentFunctionParameters = new JSON();
            functions[currentFunctionId].Save(currentFunctionParameters);
            json.Add("currentFunctionParameters", currentFunctionParameters);
        }
    }

    public void Load(JSON json)
    {
        scriptStack.Clear();

        if (json.ContainsKey("isRunning"))
            isRunning = json.GetBool("isRunning");
        if (json.ContainsKey("isPaused"))
            isPaused = json.GetBool("isPaused");
        if (json.ContainsKey("ignoreCheckDepthInternalValue"))
            ignoreCheckDepthInternalValue = json.GetJNumber("ignoreCheckDepthInternalValue").AsUInt();
        if (json.ContainsKey("lastFunctionModifiedLine"))
            lastFunctionModifiedLine = json.GetBool("lastFunctionModifiedLine");
        if (json.ContainsKey("waitingForFunction"))
            waitingForFunction = json.GetBool("waitingForFunction");
        if (json.ContainsKey("currentFunctionId"))
            currentFunctionId = json.GetJNumber("currentFunctionId").AsUInt();
        if (json.ContainsKey("currentFunctionDepth"))
            currentFunctionDepth = json.GetJNumber("currentFunctionDepth").AsUInt();

        if (json.ContainsKey("currentLine"))
            currentLine = json.GetJNumber("currentLine").AsUInt();
        if (json.ContainsKey("currentFilePath"))
            currentFilePath = json.GetString("currentFilePath");

        if (json.ContainsKey("scriptStack"))
        {
            JSON jsonEntry;
            JArray stack = json.GetJArray("scriptStack");
            foreach (JValue entry in stack.Values)
            {
                if (entry is JSON)
                {
                    jsonEntry = entry as JSON;
                    if (jsonEntry.ContainsKey("filePath") && jsonEntry.ContainsKey("lineCounter"))
                        scriptStack.Add(new ANSLContextStackEntry() { filePath = jsonEntry.GetString("filepath"), lineCounter = jsonEntry.GetJNumber("lineCounter").AsUInt() });
                }
            }
        }



        if (isRunning)
        {
            currentScript = FileManager.ReadTextAsset(Resources.Load<TextAsset>(currentFilePath)).ToArray();

            if (waitingForFunction && json.ContainsKey("currentFunctionParameters"))
            {
                JSON parameters = json.GetJSON("currentFunctionParameters");
                functions[currentFunctionId].Load(parameters);
            }
        }
    }
    #endregion

    /// <summary>
    /// Represents an entry in the context's stack
    /// </summary>
    public struct ANSLContextStackEntry
    {
        public string filePath;
        public uint lineCounter;
    }
}
