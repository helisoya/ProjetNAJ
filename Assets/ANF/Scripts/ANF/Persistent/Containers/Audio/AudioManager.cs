using Leguar.TotalJSON;

namespace ANF.Persistent
{
    /// <summary>
	/// Represents the base template for an audio manager. 
    /// To add a new implementation (Wwise, Fmod, ...), extends this class
	/// </summary>
    public abstract class AudioManager : DataContainer
    {
        public abstract DataContainer CloneContainer();
        public abstract void Initialize(ANFSettings settings);
        public abstract void Load(JSON json);
        public abstract void Save(JSON json);
        public abstract void Reset();

        /// <summary>
        /// Sets the volume for the music
        /// </summary>
        /// <param name="newVolume">The new volume</param>
        public abstract void SetMusicVolume(float newVolume);

        /// <summary>
        /// Sets the volume for the Sfx
        /// </summary>
        /// <param name="newVolume">The new volume</param>
        public abstract void SetSfxVolume(float newVolume);

        /// <summary>
        /// Sets the volume for the Ambient
        /// </summary>
        /// <param name="newVolume">The new volume</param>
        public abstract void SetAmbientVolume(float newVolume);

        /// <summary>
        /// Sets the volume for the voices
        /// </summary>
        /// <param name="newVolume">The new volume</param>
        public abstract void SetVoiceVolume(float newVolume);

        /// <summary>
        /// Gets the music volume
        /// </summary>
        /// <returns>Its volume</returns>
        public abstract float GetMusicVolume();

        /// <summary>
        /// Gets the ambient volume
        /// </summary>
        /// <returns>Its volume</returns>
        public abstract float GetAmbientVolume();

        /// <summary>
        /// Gets the sfx volume
        /// </summary>
        /// <returns>Its volume</returns>
        public abstract float GetSfxVolume();

        /// <summary>
        /// Gets the voices volume
        /// </summary>
        /// <returns>Its volume</returns>
        public abstract float GetVoiceVolume();

        /// <summary>
		/// Plays a sound effect
		/// </summary>
		/// <param name="sfxName">The SFX's name</param>
		/// <param name="baseVolume">The base volume for this sound (between 0 and 1)</param>
        public abstract void PlaySFX(string sfxName, float baseVolume);

        /// <summary>
		/// Plays a voice clip
		/// </summary>
		/// <param name="voiceName">The voice clip's name</param>
		/// <param name="baseVolume">The base volume for this sound (between 0 and 1)</param>
        public abstract void PlayVoice(string voiceName, float baseVolume);

        /// <summary>
		/// Plays a specific music (only one at a time)
		/// </summary>
		/// <param name="musicName">The music's name</param>
		/// <param name="baseVolume">The base volume for this sound (between 0 and 1)</param>
        public abstract void PlayMusic(string musicName, float baseVolume);

        /// <summary>
		/// Plays a specific ambient sound (only one at a time)
		/// </summary>
		/// <param name="ambientName">The ambient's name</param>
		/// <param name="baseVolume">The base volume for this sound (between 0 and 1)</param>
        public abstract void PlayAmbient(string ambientName, float baseVolume);

        /// <summary>
		/// Plays a SFX for moving the in game cursor
		/// </summary>
        public abstract void PlayUICursorMoveSFX();

        /// <summary>
		/// Plays a SFX for confirming something in the UI
		/// </summary>
        public abstract void PlayUICursorConfirmSFX();

        /// <summary>
		/// Plays an SFX for canceling something in the UI
		/// </summary>
        public abstract void PlayUICursorCancelSFX();
    }
}
