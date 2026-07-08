using System.Collections.Generic;

namespace ANF.Persistent
{
    /// <summary>
    /// Represents a definition set for a map.
    /// (When can a button can be clicked, and where does it lead)
    /// </summary>
    public class MapDefs
    {
        public string id;
        public List<MapButtonDef> buttons;
    }

    /// <summary>
	/// Represents a map definition entry.
    /// It stipulate under which conditions a button can be shown
	/// </summary>
    public class MapButtonDef
    {
        public string linkedButton;
        public MapDefsType type;
        public string linkedScript;

        public string linkedVariable;
        public MapDefsVariableCheckType variableCheckType;
        public int variableCheckValue;

        /// <summary>
		/// Checks if the linked button can be shown
		/// </summary>
		/// <param name="variableValue">The linked variable value (unused if not a variable type)</param>
		/// <returns>True if the button is visible</returns>
        public bool CheckIfVisible(int variableValue)
        {
            switch (type)
            {
                case MapDefsType.Always:
                    return true;
                case MapDefsType.Never:
                    return false;
                case MapDefsType.VariableToggle:
                    return variableValue != 0;
                case MapDefsType.Variable:
                    switch (variableCheckType)
                    {
                        case MapDefsVariableCheckType.Equals:
                            return variableCheckValue == variableValue;
                        case MapDefsVariableCheckType.NotEquals:
                            return variableCheckValue != variableValue;
                        case MapDefsVariableCheckType.Less:
                            return variableValue < variableCheckValue;
                        case MapDefsVariableCheckType.Greater:
                            return variableValue > variableCheckValue;
                        case MapDefsVariableCheckType.LessOrEquals:
                            return variableValue <= variableCheckValue;
                        case MapDefsVariableCheckType.GreaterOrEquals:
                            return variableValue >= variableCheckValue;
                    }
                    return false;
            }
            return false;
        }
    }

    /// <summary>
	/// Check type if the button is linked to a variable
	/// </summary>
    public enum MapDefsVariableCheckType
    {
        Equals,
        NotEquals,
        Less,
        Greater,
        LessOrEquals,
        GreaterOrEquals
    }

    /// <summary>
	/// The definition type regarding a map button
	/// </summary>
    public enum MapDefsType
    {
        Always,
        Variable,
        VariableToggle,
        Never
    }

}
