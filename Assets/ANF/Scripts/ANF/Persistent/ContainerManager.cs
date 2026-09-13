using ANF.Utils;
using System.Collections.Generic;

namespace ANF.Persistent
{

    /// <summary>
	/// Handles multiple containers
	/// </summary>
    public class ContainerManager : DataManager<DataContainer>
    {

        public ContainerManager(ComponentRegisterEntry<DataContainer>[] containers)
        {
            this.components = new Dictionary<string, DataContainer>();

            foreach (ComponentRegisterEntry<DataContainer> entry in containers)
            {
                if (entry.data == null)
                    continue;

                string finalId = string.IsNullOrEmpty(entry.id) ? entry.data.GetType().FullName : entry.id;

                DataContainer copy = entry.data.CloneContainer();

                this.components.Add(finalId, copy);
            }
        }

        /// <summary>
		/// Initialize the containers
		/// </summary>
		/// <param name="settings">The ANF Settings</param>
        public void Initialize(ANFSettings settings)
        {
            foreach (DataContainer container in components.Values)
            {
                container.Initialize(settings);
            }
        }

        /// <summary>
		/// Resets all components
		/// </summary>
        public void ResetAll()
        {
            foreach (DataContainer container in components.Values)
            {
                container.Reset();
            }
        }

        /// <summary>
		/// Adds a new container to the manager
		/// </summary>
		/// <typeparam name="T">The container type</typeparam>
		/// <param name="uninitializedContainer">The container (uninitialized)</param>
		/// <param name="name">The container's name (null to auto generate one)</param>
		/// <param name="anfSettings">The settings</param>
		/// <returns>True if the container was added</returns>
        public bool AddComponent<T>(T uninitializedContainer, string name, ANFSettings anfSettings) where T : DataContainer
        {
            if (string.IsNullOrEmpty(name))
                name = uninitializedContainer.GetType().FullName;

            if (components.ContainsKey(name))
            {
                return false;
            }

            components.Add(name, uninitializedContainer);
            uninitializedContainer.Initialize(anfSettings);
            return true;
        }
    }
}
