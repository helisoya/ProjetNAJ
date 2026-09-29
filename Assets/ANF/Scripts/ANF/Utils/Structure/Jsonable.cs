using Leguar.TotalJSON;

namespace ANF.Utils
{
    /// <summary>
    /// A Jsonable can be loaded and saved to a JSON
    /// </summary>
    public interface Jsonable
    {
        /// <summary>
		/// Saves the object to a json
		/// </summary>
		/// <param name="json">The JSON to save to</param>
        public abstract void Save(JSON json);

        /// <summary>
		/// Loads the object from a json
		/// </summary>
		/// <param name="json">The json</param>
        /// <returns>True if the load was immediate. False if it is still ongoing</returns>
        public abstract bool Load(JSON json);
    }
}

