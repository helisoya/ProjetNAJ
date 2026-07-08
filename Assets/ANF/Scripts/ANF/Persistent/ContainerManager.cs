using ANF.Utils;
using System.Collections.Generic;

namespace ANF.Persistent
{

    /// <summary>
	/// Handles multiple containers
	/// </summary>
    public class ContainerManager : DataManager<DataContainer>
    {

        public ContainerManager(ComponentRegisterEntry<DataContainer>[] containers, ANFSettings settings)
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
    }
}
