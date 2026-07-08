namespace ANF.ANSL
{
    /// <summary>
    /// Represents an ANSL function parameter
    /// </summary>
    public class FunctionParameters
    {
        private string[] parameters;
        private FunctionParameterType[] template;
        private bool isValid;
        private uint id;

        public void Initialize(string[] parameters, FunctionParameterType[] template, uint id)
        {
            this.parameters = parameters;
            this.template = template;
            this.id = id;
            isValid = CheckIfValid();
        }

        /// <summary>
        /// Gets the template Id
        /// </summary>
        /// <returns>The template Id</returns>
        public uint GetTemplateId()
        {
            return id;
        }

        /// <summary>
        /// Gets the template
        /// </summary>
        /// <returns>The template</returns>
        public FunctionParameterType[] GetTemplate()
        {
            return template;
        }

        /// <summary>
        /// Gets a parameter's type
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <returns>The index</returns>
        public FunctionParameterType GetParameterType(int parameterIndex)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < template.Length)
            {
                return template[parameterIndex];
            }
            return FunctionParameterType.UNKNOWN;
        }

        /// <summary>
        /// Gets if the interface is valid or not
        /// </summary>
        /// <returns>True if valid</returns>
        public bool IsValid()
        {
            return isValid;
        }

        /// <summary>
        /// Gets an int parameter
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <param name="value">The out value</param>
        /// <returns>True if the retrieval was a success</returns>
        public bool GetParameter(int parameterIndex, out int value)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < parameters.Length
                && template[parameterIndex] == FunctionParameterType.INT)
            {
                if (int.TryParse(parameters[parameterIndex], out value))
                    return true;
            }

            value = 0;
            return false;
        }

        /// <summary>
        /// Gets a uint parameter
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <param name="value">The out value</param>
        /// <returns>True if the retrieval was a success</returns>
        public bool GetParameter(int parameterIndex, out uint value)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < parameters.Length
                && template[parameterIndex] == FunctionParameterType.UINT)
            {
                if (uint.TryParse(parameters[parameterIndex], out value))
                    return true;
            }

            value = 0;
            return false;
        }

        /// <summary>
        /// Gets a bool parameter
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <param name="value">The out value</param>
        /// <returns>True if the retrieval was a success</returns>
        public bool GetParameter(int parameterIndex, out bool value)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < parameters.Length
                && template[parameterIndex] == FunctionParameterType.BOOL)
            {
                if (bool.TryParse(parameters[parameterIndex], out value))
                    return true;
            }

            value = false;
            return false;
        }

        /// <summary>
        /// Gets a float parameter
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <param name="value">The out value</param>
        /// <returns>True if the retrieval was a success</returns>
        public bool GetParameter(int parameterIndex, out float value)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < parameters.Length
                && template[parameterIndex] == FunctionParameterType.FLOAT)
            {
                if (float.TryParse(parameters[parameterIndex], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
                    return true;
            }

            value = 0;
            return false;
        }

        /// <summary>
        /// Gets a string parameter
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <param name="value">The out value</param>
        /// <returns>True if the retrieval was a success</returns>
        public bool GetParameter(int parameterIndex, out string value)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < parameters.Length
                && template[parameterIndex] == FunctionParameterType.STRING)
            {
                value = parameters[parameterIndex];
                if (value.ToLower().Equals("null"))
                    value = null;
                return true;
            }

            value = "";
            return false;
        }

        /// <summary>
        /// Gets a string list parameter
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <param name="value">The out value</param>
        /// <returns>True if the retrieval was a success</returns>
        public bool GetParameter(int parameterIndex, out string[] value)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < parameters.Length
                && template[parameterIndex] == FunctionParameterType.LISTSTRING)
            {
                int listSize = parameters.Length - parameterIndex;
                value = new string[listSize];
                for (int i = 0; i < listSize; i++)
                {
                    value[i] = parameters[parameterIndex + i];
                    if (value[i].ToLower().Equals("null"))
                        value[i] = null;
                }

                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Gets an int list parameter
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <param name="value">The out value</param>
        /// <returns>True if the retrieval was a success</returns>
        public bool GetParameter(int parameterIndex, out int[] value)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < parameters.Length
                && template[parameterIndex] == FunctionParameterType.LISTINT)
            {
                int listSize = parameters.Length - parameterIndex;
                value = new int[listSize];
                for (int i = 0; i < listSize; i++)
                {
                    if (int.TryParse(parameters[parameterIndex + i], out int result))
                    {
                        value[i] = result;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Gets a uint list parameter
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <param name="value">The out value</param>
        /// <returns>True if the retrieval was a success</returns>
        public bool GetParameter(int parameterIndex, out uint[] value)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < parameters.Length
                && template[parameterIndex] == FunctionParameterType.LISTUINT)
            {
                int listSize = parameters.Length - parameterIndex;
                value = new uint[listSize];
                for (int i = 0; i < listSize; i++)
                {
                    if (uint.TryParse(parameters[parameterIndex + i], out uint result))
                    {
                        value[i] = result;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Gets a bool list parameter
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <param name="value">The out value</param>
        /// <returns>True if the retrieval was a success</returns>
        public bool GetParameter(int parameterIndex, out bool[] value)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < parameters.Length
                && template[parameterIndex] == FunctionParameterType.LISTBOOL)
            {
                int listSize = parameters.Length - parameterIndex;
                value = new bool[listSize];
                for (int i = 0; i < listSize; i++)
                {
                    if (bool.TryParse(parameters[parameterIndex + i], out bool result))
                    {
                        value[i] = result;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Gets a float list parameter
        /// </summary>
        /// <param name="parameterIndex">The parameter's index</param>
        /// <param name="value">The out value</param>
        /// <returns>True if the retrieval was a success</returns>
        public bool GetParameter(int parameterIndex, out float[] value)
        {
            if (isValid && parameterIndex >= 0 && parameterIndex < parameters.Length
                && template[parameterIndex] == FunctionParameterType.LISTINT)
            {
                int listSize = parameters.Length - parameterIndex;
                value = new float[listSize];
                for (int i = 0; i < listSize; i++)
                {
                    if (float.TryParse(parameters[parameterIndex + i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
                    {
                        value[i] = result;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Checks if the inteface is valid or not
        /// </summary>
        /// <returns>True if valid</returns>
        private bool CheckIfValid()
        {
            if (template == null)
                return false;

            if (template.Length == 0)
                return parameters == null;

            FunctionParameterType lastType = template.Length == 0 ? FunctionParameterType.UNKNOWN : template[template.Length - 1];
            if (parameters.Length != template.Length &&
                !(lastType == FunctionParameterType.LISTSTRING || lastType == FunctionParameterType.LISTINT
                || lastType == FunctionParameterType.LISTUINT || lastType == FunctionParameterType.LISTFLOAT
                || lastType == FunctionParameterType.LISTBOOL))
                return false;
            for (int i = 0; i < template.Length; i++)
            {
                bool valid = true;
                switch (template[i])
                {
                    case FunctionParameterType.INT:
                        valid = int.TryParse(parameters[i], out int _);
                        break;
                    case FunctionParameterType.UINT:
                        valid = uint.TryParse(parameters[i], out uint _);
                        break;
                    case FunctionParameterType.FLOAT:
                        valid = float.TryParse(parameters[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float _);
                        break;
                    case FunctionParameterType.BOOL:
                        valid = bool.TryParse(parameters[i], out bool _);
                        break;
                    case FunctionParameterType.LISTINT:
                        valid = i == template.Length - 1;
                        if (valid)
                        {
                            for (int j = i; j < parameters.Length; j++)
                            {
                                if (!int.TryParse(parameters[j], out int _))
                                {
                                    valid = false;
                                    break;
                                }
                            }
                        }
                        break;
                    case FunctionParameterType.LISTUINT:
                        valid = i == template.Length - 1;
                        if (valid)
                        {
                            for (int j = i; j < parameters.Length; j++)
                            {
                                if (!uint.TryParse(parameters[j], out uint _))
                                {
                                    valid = false;
                                    break;
                                }
                            }
                        }
                        break;
                    case FunctionParameterType.LISTFLOAT:
                        valid = i == template.Length - 1;
                        if (valid)
                        {
                            for (int j = i; j < parameters.Length; j++)
                            {
                                if (!float.TryParse(parameters[j], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float _))
                                {
                                    valid = false;
                                    break;
                                }
                            }
                        }
                        break;
                    case FunctionParameterType.LISTBOOL:
                        valid = i == template.Length - 1;
                        if (valid)
                        {
                            for (int j = i; j < parameters.Length; j++)
                            {
                                if (!bool.TryParse(parameters[j], out bool _))
                                {
                                    valid = false;
                                    break;
                                }
                            }
                        }
                        break;
                    case FunctionParameterType.LISTSTRING:
                        valid = i == template.Length - 1;
                        break;
                }

                if (!valid)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Clears the interface
        /// </summary>
        public void Clear()
        {
            parameters = null;
            template = null;
            isValid = false;
        }

    }

    /// <summary>
    /// Base types for parameters
    /// </summary>
    public enum FunctionParameterType
    {
        UNKNOWN,
        INT,
        UINT,
        FLOAT,
        BOOL,
        STRING,
        LISTINT,
        LISTUINT,
        LISTSTRING,
        LISTFLOAT,
        LISTBOOL
    }
}

