namespace ANF.Scene
{
    /// <summary>
	/// Handles Characters in the scene
	/// </summary>
    [System.Serializable]
    public class CharacterManager : SceneObjectManager<Character>
    {
        public override WorldComponent CloneComponent()
        {
            return new CharacterManager()
            {
                canBeSaved = canBeSaved,
                enabledByDefault = enabledByDefault,
                prefabsPath = prefabsPath
            };
        }
    }

}
