using Leguar.TotalJSON;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine.UIElements;

namespace ANF.Utils
{

    /// <summary>
	/// Handles multiple containers/components
	/// </summary>
    public abstract class DataManager<T> : Jsonable where T : Jsonable
    {
        protected Dictionary<string, T> components;

        /// <summary>
		/// Gets a specific component
		/// </summary>
		/// <typeparam name="T">The component's type</typeparam>
		/// <param name="result">The out result</param>
		/// <returns>True if the component was found</returns>
        public bool GetComponent<P>(out P result) where P : T
        {
            if (GetComponent<P>(typeof(P).FullName, out result))
                return true;

            foreach (T component in components.Values)
            {
                if (component.GetType().IsSubclassOf(typeof(P)) || component.GetType() == typeof(P))
                {
                    result = (P)component;
                    return true;
                }
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Gets a specific component
        /// </summary>
        /// <typeparam name="id">The component's id</typeparam>
        /// <typeparam name="P">The component's type</typeparam>
        /// <param name="result">The out result</param>
        /// <returns>True if the component was found</returns>
        public bool GetComponent<P>(string id, out P result) where P : T
        {
            if (components.TryGetValue(id, out T component))
            {
                if (component.GetType().IsSubclassOf(typeof(P)) || component.GetType() == typeof(P))
                {
                    result = (P)component;
                    return true;
                }
            }

            result = default;
            return false;
        }

        /// <summary>
		/// Loads the data containers from the json
		/// </summary>
		/// <param name="json">The data containers</param>
        public void Load(JSON json)
        {
            foreach (string key in components.Keys)
                if (json.ContainsKey(key))
                    components[key].Load(json.GetJSON(key));
        }

        /// <summary>
		/// Saves the data containers to the json
		/// </summary>
		/// <param name="json">The json</param>
        public void Save(JSON json)
        {
            JSON individualDataJson;

            foreach (string containerId in components.Keys)
            {
                individualDataJson = new JSON();
                components[containerId].Save(individualDataJson);

                if (individualDataJson.Count != 0)
                    json.Add(containerId, individualDataJson);
            }
        }

        /// <summary>
        /// Invokes a method on all components
        /// </summary>
        /// <param name="methodName">The method's name</param>
        /// <param name="data">The method's parameter</param>
        public void Invoke(string methodName)
        {
            Invoke<Null>(methodName, null);
        }

        /// <summary>
        /// Invokes a method on all components
        /// </summary>
        /// <typeparam name="ValueType">The method's parameter's type</typeparam>
        /// <param name="methodName">The method's name</param>
        /// <param name="data">The method's parameter</param>
        public void Invoke<ValueType>(string methodName, ValueType data)
        {
            foreach (T component in components.Values)
            {
                MethodInfo info = component.GetType().GetMethod(methodName);
                if (info != null)
                {
                    ParameterInfo[] parameters = info.GetParameters();
                    if (data == null && parameters.Length == 0)
                        info.Invoke(component, null);
                    else if (parameters.Length == 1) // Disabled tpye check, Boolean / bool
                        info.Invoke(component, new object[] { data });
                }
            }
        }
    }
}
