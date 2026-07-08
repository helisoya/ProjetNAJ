using ANF.Utils;

namespace ANF.Scene
{
    /// <summary>
	/// Represents the 3D world and handles its components
	/// </summary>
    public class World : ANFComponentManager<WorldComponent>
    {
        protected ANFManager manager;

        public World(ANFManager manager, ComponentRegisterEntry<WorldComponent>[] registeredComponents)
        {
            this.manager = manager;

            components = new System.Collections.Generic.Dictionary<string, WorldComponent>();
            foreach (ComponentRegisterEntry<WorldComponent> entry in registeredComponents)
            {
                if (entry.data == null)
                    continue;

                string finalId = string.IsNullOrEmpty(entry.id) ? entry.data.GetType().FullName : entry.id;

                WorldComponent copy = entry.data.CloneComponent();
                copy.Initialize(manager);
                components.Add(finalId, copy);
            }
        }

        public override void OnUpdate()
        {
            foreach (WorldComponent component in components.Values)
                if (component.isEnabled && !component.isPaused)
                    component.OnUpdate();
        }
    }

}
