using UnityEngine;


namespace ANF.Utils
{
	/// <summary>
	/// General ANF Utilities
	/// </summary>
	public class ANFUtils
	{

		/// <summary>
		/// Loads a sprite (possibly from a spritesheet)
		/// </summary>
		/// <param name="resourcePath">The resource path</param>
		/// <param name="spriteName">The sprite's name</param>
		/// <param name="spritesheet">The spritesheet's name (can be null)</param>
		/// <returns>The sprite</returns>
		public static Sprite LoadSprite(string resourcePath, string spriteName, string spritesheet)
		{
			if (spriteName == null)
				return null;

			if (string.IsNullOrEmpty(spritesheet))
				return Resources.Load<Sprite>(resourcePath + spriteName);

			Sprite[] sprites = Resources.LoadAll<Sprite>(resourcePath + spritesheet);

			foreach (Sprite sprite in sprites)
				if (sprite.name.Equals(spriteName))
					return sprite;

			return null;
		}
	}
}