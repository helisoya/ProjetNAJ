using ANF.Scene;
using ANF.Utils;
using Leguar.TotalJSON;
using System.Collections.Generic;
using System.Reflection;

namespace ANF.ANSL
{
    /// <summary>
    /// Represents an ANSL Function
    /// Ex:
    /// setBackground(Mountain)
    /// </summary>
    public abstract class ANSLFunction : Jsonable
    {
        public bool isProcessing { get; protected set; }

        protected ANSLContext context;
        protected ANFManager manager;
        protected FunctionParameters parameters;

        public ANSLFunction()
        {
            isProcessing = false;
        }

        /// <summary>
		/// Gets the function's attribute
		/// </summary>
		/// <returns>The function attribute</returns>
        public ANSLFunctionAttribute GetAttribute()
        {
            return GetType().GetCustomAttribute<ANSLFunctionAttribute>();
        }

        /// <summary>
		/// Compiles the function
		/// </summary>
		/// <param name="compiledLines">The compiled lines of script for this function</param>
		/// <param name="cleanedLine">The line to compile (cleaned)</param>
        /// <param name="compiler">The compiler</param>
        /// <param name="id">The function's id</param>
		/// <param name="errors">The global error list</param>
        /// <param name="outputLine">The output line the function will start in</param>
		/// <returns>True if the compiling was successful</returns>
        public virtual bool Compile(out List<string> compiledLines, string cleanedLine, uint id, ANSLCompiler compiler, List<ANSLUtils.ANSLError> errors, int outputLine)
        {
            compiledLines = new List<string>();

            string[] split = cleanedLine.Split(new char[] { '(', ')' });

            if (split.Length != 3 || split[2].Length != 0)
            {
                // More than one pair of () found
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"Too many () detected : {cleanedLine}."
                });
                return false;
            }

            string[] parameters = split[1].Split(';');
            if (parameters.Length == 1 && parameters[0].Length == 0)
                parameters = null;

            if (ANSLUtils.CreateParametersInterface(parameters, GetParametersTemplates()) == null)
            {
                // Failed to find a valid parameter template
                errors.Add(new ANSLUtils.ANSLError()
                {
                    type = ANSLUtils.ANSLErrorType.ERROR,
                    filePath = compiler.GetSourceFilepath(),
                    line = compiler.GetCurrentLineCounter(),
                    errorMessage = $"Unknown parameter template : {cleanedLine}."
                });
                return false;
            }

            string compiledLine = id.ToString();

            if (parameters != null)
            {
                foreach (string parameter in parameters)
                {
                    compiledLine += "|" + parameter;
                }
            }

            compiledLines.Add(compiledLine);

            return true;
        }

        /// <summary>
        /// Generates the parameters templates for this function.
        /// You can have multiple templates for one function.
        /// Ex : setPosition(MARKER), setPosition(X;Y;Z) 
        /// </summary>
        /// <returns>The templates</returns>
        public abstract FunctionParameterType[][] GetParametersTemplates();

        /// <summary>
		/// Initialize the function
		/// </summary>
		/// <param name="context">The ANSL context</param>
		/// <param name="manager">The ANF Manager</param>
        public void Initialize(ANSLContext context, ANFManager manager)
        {
            this.context = context;
            this.manager = manager;
        }

        /// <summary>
        /// Starts the function's process
        /// </summary>
        /// <param name="parameters">The function's parameters</param>
        public void StartProcess(FunctionParameters parameters)
        {
            isProcessing = true;
            this.parameters = parameters;
            OnStartProcess();
        }

        /// <summary>
        /// Marks the function as ended.
        /// Script flow will then resume
        /// </summary>
        protected void EndProcess()
        {
            isProcessing = false;
            OnCleanup();
        }

        /// <summary>
        /// Updates the function
        /// </summary>
        public void Update()
        {
            if (isProcessing)
            {
                OnUpdate();
            }
        }

        /// <summary>
		/// Cleans up the function. Called in rare occasions (Changing scenes for instance)
		/// </summary>
        public void Cleanup()
        {
            OnCleanup();
        }

        /// <summary>
        /// Called when updating the function
        /// Use this is the function does things in more than one frame
        /// </summary>
        protected abstract void OnUpdate();

        /// <summary>
        /// Called when the function starts processing.
        /// If the function doesn't last over time (Ex: Edit a variable), call EndProcess() here
        /// </summary>
        protected abstract void OnStartProcess();

        /// <summary>
        /// Called when ending the function's processing.
        /// Clean things here if needs be
        /// </summary>
        protected abstract void OnCleanup();

        /// <summary>
        /// On save callback
        /// </summary>
        /// <param name="json">The json to save to</param>
        protected virtual void OnSave(JSON json) { }

        /// <summary>
		/// On load callback
		/// </summary>
		/// <param name="json">The json to load from</param>
        protected virtual void OnLoad(JSON json) { }

        public void Save(JSON json)
        {
            json.Add("isProcessing", isProcessing);

            OnSave(json);
        }

        public void Load(JSON json)
        {
            OnLoad(json);

            if (json.ContainsKey("isProcessing"))
                isProcessing = json.GetBool("isProcessing");
        }

        /// <summary>
        /// Invokes a method on this function
        /// </summary>
        /// <typeparam name="ValueType">The method's parameter's type</typeparam>
        /// <param name="methodName">The method's name</param>
        /// <param name="data">The method's parameter</param>
        public void Invoke<ValueType>(string methodName, ValueType data)
        {
            MethodInfo info = GetType().GetMethod(methodName);
            if (info != null)
            {
                ParameterInfo[] parameters = info.GetParameters();
                if (data == null && parameters.Length == 0)
                    info.Invoke(this, null);
                else if (parameters.Length == 1) // Disabled type check, Boolean / bool
                    info.Invoke(this, new object[] { data });
            }
        }
    }
}

