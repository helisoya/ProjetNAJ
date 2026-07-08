using ANF.Persistent;
using ANF.Scene;
using ANF.Utils;
using Leguar.TotalJSON;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ANF.ANSL
{
    /// <summary>
    /// Handles the different ANSL Contexts
    /// </summary>
    [System.Serializable]
    public class ANSLManager : WorldComponent
    {
        private ANSLContext[] contexts;

        [Tooltip("How many ANSL function can be called per frame (per context)")]
        [SerializeField] private uint maxFunctionsPerFrame = 10;
        [Tooltip("How many concurrent contexts can coexist")]
        [SerializeField] private uint maxContexts = 20;
        [Tooltip("How large the script stack can be (per context)")]
        [SerializeField] private uint contextStackLength = 10;
        private bool autoplayEnabled;
        private bool skipModeEnabled;

        /// <summary>
        /// Initialize the manager
        /// </summary>
        public override void OnInitialize()
        {
            GenerateContexts(manager);
        }

        public override void OnStart()
        {
            // Unused
        }

        public void OnAutoPlayToggle(bool enabled)
        {
            foreach (ANSLContext context in contexts)
                context.SetAutoPlay(enabled);
        }

        public void OnSkipModeToggle(bool enabled)
        {
            foreach (ANSLContext context in contexts)
                context.SetSkipMode(enabled);
        }

        /// <summary>
        /// Generate the context pool
        /// </summary>
        /// <param name="manager">The ANF Manager</param>
        private void GenerateContexts(ANFManager manager)
        {
            List<KeyValuePair<Type,uint>> functions = ANSLUtils.GetValidANSLFunctionsList(PersistentDataManager.instance.GetANFSettings());

            contexts = new ANSLContext[maxContexts];
            for (int i = 0; i < contexts.Length; i++)
            {
                Dictionary<uint, ANSLFunction> instances = new Dictionary<uint, ANSLFunction>();
                foreach (KeyValuePair<Type, uint> type in functions)
                {
                    ANSLFunctionAttribute attribute = type.Key.GetAttribute<ANSLFunctionAttribute>();
                    if (attribute != null && !instances.ContainsKey(type.Value))
                    {
                        instances.Add(type.Value, (ANSLFunction)type.Key.Instantiate());
                    }
                }

                contexts[i] = new ANSLContext(instances, contextStackLength, maxFunctionsPerFrame, manager);
            }
        }

        /// <summary>
        /// Gets a specific context
        /// </summary>
        /// <param name="contextId">The context's Id</param>
        /// <returns>The context if found</returns>
        public ANSLContext GetContext(uint contextId)
        {
            if (contextId < 0 || contextId >= contexts.Length)
                return null;

            return contexts[contextId];
        }

        /// <summary>
        /// Starts a new context
        /// </summary>
        /// <param name="scriptPath">The script's filepath</param>
        /// <param name="startLine">The starting line counter. 0 by default</param>
        /// <param name="startImmediately">True if the script should be started this frame</param>
        /// <returns>The new context's Id. -1 if no context was avaiable</returns>
        public int StartNewContext(string scriptPath, uint startLine = 0, bool startImmediately = true)
        {
            for (int i = 0; i < contexts.Length; i++)
            {
                if (!contexts[i].isRunning)
                {
                    contexts[i].LoadScript(scriptPath, startLine, startImmediately);
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Pauses all contexts
        /// </summary>
        /// <param name="paused">True if the contexts are now paused</param>
        public void PauseAllContexts(bool paused)
        {
            foreach (ANSLContext context in contexts)
            {
                context.PauseContext(paused);
            }
        }

        /// <summary>
        /// Pauses a specific context
        /// </summary>
        /// <param name="contextId">The context's Id</param>
        /// <param name="paused">True if the context should be paused</param>
        public void PauseContext(uint contextId, bool paused)
        {
            if (contextId >= 0 && contextId < contexts.Length)
                contexts[contextId].PauseContext(paused);
        }

        public override WorldComponent CloneComponent()
        {
            return new ANSLManager()
            {
                canBeSaved = canBeSaved,
                enabledByDefault = enabledByDefault,
                manager = manager,
                maxFunctionsPerFrame = maxFunctionsPerFrame,
                maxContexts = maxContexts,
                contextStackLength = contextStackLength
            };
        }

        public override void OnUpdate()
        {
            foreach (ANSLContext context in contexts)
            {
                if (context.isRunning)
                    context.Update();
            }
        }

        /// <summary>
        /// Refresh whether contexts should be enabled
        /// </summary>
        private void CheckEnableState()
        {
            PauseAllContexts(isPaused);
        }

        public override void OnPaused()
        {
            CheckEnableState();
        }

        public override void OnUnPaused()
        {
            CheckEnableState();
        }

        public override void OnEnabled()
        {
            CheckEnableState();
        }

        public override void OnDisabled()
        {
            CheckEnableState();
        }

        public override void OnRegisterInputs()
        {
        }

        public override void OnUnRegisterInputs()
        {
        }

        public override void OnChangeScene()
        {
            foreach (ANSLContext context in contexts)
            {
                context.Cleanup();
            }
        }

        public override void OnSave(JSON json)
        {
            JArray contextsArray = new JArray();
            JSON contextJSON;
            foreach (ANSLContext context in contexts)
            {
                contextJSON = new JSON();
                context.Save(contextJSON);
                contextsArray.Add(contextJSON);
            }

            json.Add("contexts", contextsArray);
        }

        public override void OnLoad(JSON json)
        {
            if (json.ContainsKey("contexts"))
            {
                JArray contextsArray = json.GetJArray("contexts");
                JSON contextJSON;
                for (int i = 0; i < contextsArray.Length; i++)
                {
                    if (i >= contexts.Length)
                        continue;

                    if (contextsArray.Values[i] is JSON)
                    {
                        contextJSON = contextsArray.Values[i] as JSON;
                        contexts[i].Load(contextJSON);
                    }
                }
            }
            CheckEnableState();
        }
    }
}