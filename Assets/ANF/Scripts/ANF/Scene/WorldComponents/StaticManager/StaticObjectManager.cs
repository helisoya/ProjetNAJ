namespace ANF.Scene
{
    /// <summary>
	/// Handles static objects in the scene
	/// </summary>
    [System.Serializable]
    public class StaticObjectManager : SceneObjectManager<StaticObject>
    {
        public override WorldComponent CloneComponent()
        {
            return new StaticObjectManager()
            {
                canBeSaved = canBeSaved,
                enabledByDefault = enabledByDefault,
                prefabsPath = prefabsPath
            };
        }
    }

}
