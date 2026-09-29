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
        public abstract bool Load(JSON json);
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

    /// <summary>
    /// Represents the part of the audio manager that needs to be saved in the player data
    /// </summary>
    public class PlayerAudioData : DataContainer
    {
        private AudioManager audioManager;
        private string currentAmbient;
        private float currentAmbientVolume;
        private string currentMusic;
        private float currentMusicVolume;

        /// <summary>
		/// Sets the current ambient
		/// </summary>
		/// <param name="ambient">The new ambient</param>
        public void SetCurrentAmbient(string ambient)
        {
            currentAmbient = ambient;
        }

        /// <summary>
		/// Gets the current ambient
		/// </summary>
		/// <returns>The current ambient</returns>
        public string GetCurrentAmbient()
        {
            return currentAmbient;
        }

        /// <summary>
		/// Sets the current music
		/// </summary>
		/// <param name="music">The new music</param>
        public void SetCurrentMusic(string music)
        {
            currentMusic = music;
        }

        /// <summary>
		/// Gets the current music
		/// </summary>
		/// <returns>The current music</returns>
        public string GetCurrentMusic()
        {
            return currentMusic;
        }

        /// <summary>
        /// Sets the current ambient volume
        /// </summary>
        /// <param name="ambient">The new ambient volume</param>
        public void SetCurrentAmbientVolume(float ambientVolume)
        {
            currentAmbientVolume = ambientVolume;
        }

        /// <summary>
		/// Gets the current ambient volume
		/// </summary>
		/// <returns>The current ambient volume</returns>
        public float GetCurrentAmbientVolume()
        {
            return currentAmbientVolume;
        }

        /// <summary>
        /// Sets the current music volume
        /// </summary>
        /// <param name="music">The new music volume</param>
        public void SetCurrentMusicVolume(float musicVolume)
        {
            currentMusicVolume = musicVolume;
        }

        /// <summary>
		/// Gets the current music
		/// </summary>
		/// <returns>The current music volume</returns>
        public float GetCurrentMusicVolume()
        {
            return currentMusicVolume;
        }

        /// <summary>
		/// Sets the linked audio manager
		/// </summary>
		/// <param name="audioManager">The audio manager</param>
        public void SetAudioManager(AudioManager audioManager)
        {
            this.audioManager = audioManager;
        }

        public DataContainer CloneContainer()
        {
            return new PlayerAudioData() { currentAmbient = null, currentMusic = null };
        }

        public void Initialize(ANFSettings settings)
        {

        }

        public bool Load(JSON json)
        {
            if (json.ContainsKey("currentAmbient") && audioManager != null)
            {
                float volume = 1.0f;
                if (json.ContainsKey("currentAmbientVolume"))
                    volume = json.GetFloat("currentAmbientVolume");
                audioManager.PlayAmbient(json.GetString("currentAmbient"), volume);
            }


            if (json.ContainsKey("currentMusic") && audioManager != null)
            {
                float volume = 1.0f;
                if (json.ContainsKey("currentMusicVolume"))
                    volume = json.GetFloat("currentMusicVolume");
                audioManager.PlayMusic(json.GetString("currentMusic"), volume);
            }

            return true;
        }

        public void Save(JSON json)
        {
            if (currentAmbient != null)
            {
                json.Add("currentAmbient", currentAmbient);
                json.Add("currentAmbientVolume", currentAmbientVolume);
            }

            if (currentMusic != null)
            {
                json.Add("currentMusic", currentMusic);
                json.Add("currentMusicVolume", currentMusicVolume);
            }
        }

        public void Reset()
        {

        }
    }
}
