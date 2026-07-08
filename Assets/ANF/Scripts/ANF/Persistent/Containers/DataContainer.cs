using ANF.Utils;

namespace ANF.Persistent
{
    /// <summary>
	/// Represents a container that contains data
    /// Data can be global (ex: Settings), and local to a savefile (ex: playerName)
	/// </summary>
    public interface DataContainer : Jsonable
    {
        /// <summary>
		/// Initialize the container
		/// </summary>
        /// <param name="settings">The ANF Settings</param>
        public abstract void Initialize(ANFSettings settings);

        /// <summary>
		/// Resets the container
		/// </summary>
        public abstract void Reset();

        /// <summary>
        /// Clones the container
        /// </summary>
        /// <returns>The cloned container</returns>
        public abstract DataContainer CloneContainer();
    }
}
