using Leguar.TotalJSON;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ANF.Persistent
{
    /// <summary>
	/// Represents the default Audio Manager that uses the default Unity Audio Engine
	/// </summary>
    [System.Serializable]
    public class DefaultAudioManager : AudioManager
    {
        [Header("Base Settings")]
        [SerializeField] private float sfxVolume;
        [SerializeField] private float ambientVolume;
        [SerializeField] private float voiceVolume;
        [SerializeField] private float musicVolume;


        [Header("Infos")]
        [Tooltip("True if all audio clips should be cached for any further use")]
        [SerializeField] private bool cacheAudioClips = true;
        [SerializeField] private string pathToAudioResources;
        [SerializeField] private AudioMixer mixer;
        private Dictionary<string, AudioClip>[] cache; // SFX, Voice, Ambient, Music
        private List<DefaultAudioSong> musics;
        private List<DefaultAudioSong> ambients;

        private AudioSource sfxSource;
        private AudioSource voiceSource;
        private string currentAmbient;
        private float currentAmbientVolume;
        private string currentMusic;
        private float currentMusicVolume;

        private float defaultMusicVolume;
        private float defaultSFXVolume;
        private float defaultAmbientVolume;
        private float defaultVoiceVolume;

        public override DataContainer CloneContainer()
        {
            return new DefaultAudioManager()
            {
                pathToAudioResources = pathToAudioResources,
                cacheAudioClips = cacheAudioClips,
                mixer = mixer,
                sfxVolume = sfxVolume,
                musicVolume = musicVolume,
                voiceVolume = voiceVolume,
                ambientVolume = ambientVolume,
            };
        }

        public override void Initialize(ANFSettings settings)
        {
            if (cacheAudioClips)
            {
                cache = new Dictionary<string, AudioClip>[4]
                {
                    new(),
                    new(),
                    new(),
                    new()
                };
            }

            defaultMusicVolume = musicVolume;
            defaultSFXVolume = sfxVolume;
            defaultAmbientVolume = ambientVolume;
            defaultVoiceVolume = voiceVolume;

            currentAmbient = null;
            currentMusic = null;

            musics = new List<DefaultAudioSong>();
            ambients = new List<DefaultAudioSong>();

            GameObject audioObj = new GameObject("Audio");
            audioObj.transform.SetParent(PersistentDataManager.instance.transform);

            mixer.SetFloat("SFX", sfxVolume);
            mixer.SetFloat("BGM", musicVolume);
            mixer.SetFloat("Ambient", ambientVolume);
            mixer.SetFloat("Voice", voiceVolume);

            sfxSource = audioObj.AddComponent<AudioSource>();
            sfxSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
            sfxSource.playOnAwake = false;

            voiceSource = audioObj.AddComponent<AudioSource>();
            voiceSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Voice")[0];
            voiceSource.playOnAwake = false;

            audioObj.AddComponent<DefaultAudioManagerBehaviour>().Init(this);
        }

        public override void Save(JSON json)
        {
            json.Add("sfxVolume", sfxVolume);
            json.Add("voiceVolume", voiceVolume);
            json.Add("musicVolume", musicVolume);
            json.Add("ambientVolume", ambientVolume);

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

        public override void Load(JSON json)
        {
            if (json.ContainsKey("sfxVolume"))
                sfxVolume = json.GetFloat("sfxVolume");
            if (json.ContainsKey("voiceVolume"))
                voiceVolume = json.GetFloat("voiceVolume");
            if (json.ContainsKey("musicVolume"))
                musicVolume = json.GetFloat("musicVolume");
            if (json.ContainsKey("ambientVolume"))
                ambientVolume = json.GetFloat("ambientVolume");

            if (json.ContainsKey("currentAmbient"))
            {
                float volume = 1.0f;
                if (json.ContainsKey("currentAmbientVolume"))
                    volume = json.GetFloat("currentAmbientVolume");
                PlayAmbient(json.GetString("currentAmbient"), volume);
            }


            if (json.ContainsKey("currentMusic"))
            {
                float volume = 1.0f;
                if (json.ContainsKey("currentMusicVolume"))
                    volume = json.GetFloat("currentMusicVolume");
                PlayMusic(json.GetString("currentMusic"), volume);
            }
        }

        public override void Reset()
        {
            SetAmbientVolume(defaultAmbientVolume);
            SetVoiceVolume(defaultVoiceVolume);
            SetSfxVolume(defaultSFXVolume);
            SetMusicVolume(defaultMusicVolume);
        }

        public override void PlayAmbient(string ambientName, float baseVolume)
        {
            currentAmbient = ambientName;
            currentAmbientVolume = baseVolume;

            if (ambientName == null)
                return;

            foreach (DefaultAudioSong ambient in ambients)
                if (ambient.GetName().Equals(ambientName))
                    return;

            AudioClip clip = GetCachedClip(ambientName, "Ambient/", 2);
            if (clip != null)
            {
                GameObject obj = new GameObject($"Ambient-{ambientName}");
                obj.transform.SetParent(sfxSource.transform);
                AudioSource source = obj.AddComponent<AudioSource>();
                source.clip = clip;

                musics.Add(new DefaultAudioSong(source, ambientName, baseVolume));
            }
        }

        public override void PlayMusic(string musicName, float baseVolume)
        {
            currentMusic = musicName;
            currentMusicVolume = baseVolume;

            if (musicName == null)
                return;

            foreach (DefaultAudioSong music in musics)
                if (music.GetName().Equals(musicName))
                    return;

            AudioClip clip = GetCachedClip(musicName, "Music/", 3);
            if (clip != null)
            {
                GameObject obj = new GameObject($"Music-{musicName}");
                obj.transform.SetParent(sfxSource.transform);
                AudioSource source = obj.AddComponent<AudioSource>();
                source.clip = clip;

                musics.Add(new DefaultAudioSong(source, musicName, baseVolume));
            }
        }

        public override void PlaySFX(string sfxName, float baseVolume)
        {
            if (sfxName == null)
                return;

            AudioClip clip = GetCachedClip(sfxName, "SFX/", 0);

            if (clip)
            {
                sfxSource.PlayOneShot(clip, baseVolume);
            }
        }

        public override void PlayVoice(string voiceName, float baseVolume)
        {
            if (voiceName == null)
                return;

            AudioClip clip = GetCachedClip(voiceName, "Voice/", 1);

            if (clip)
            {
                sfxSource.PlayOneShot(clip, baseVolume);
            }
        }

        public override void PlayUICursorCancelSFX()
        {
            PlaySFX("CursorCancel", 1.0f);
        }

        public override void PlayUICursorConfirmSFX()
        {
            PlaySFX("CursorConfirm", 1.0f);
        }

        public override void PlayUICursorMoveSFX()
        {
            PlaySFX("CursorSelect", 1.0f);
        }

        /// <summary>
		/// Updates the manager
		/// </summary>
        public void UpdateManager()
        {
            int i = musics.Count - 1;
            while (i >= 0 && musics.Count != 0)
            {
                if (musics[i].UpdateSong(musics[i].GetName().Equals(currentMusic)))
                {
                    musics[i].Destroy();
                    musics.RemoveAt(i);
                }

                i--;
            }

            i = ambients.Count - 1;
            while (i >= 0 && ambients.Count != 0)
            {
                if (ambients[i].UpdateSong(ambients[i].GetName().Equals(currentAmbient)))
                {
                    ambients[i].Destroy();
                    ambients.RemoveAt(i);
                }

                i--;
            }
        }

        /// <summary>
		/// Gets a clip from the cache, or loads in in memory if not found
		/// </summary>
		/// <param name="clipName">The clip's name</param>
		/// <param name="subFolderName">The subfolder in the resources (Ex : SFX)</param>
		/// <param name="cacheIndex">The cache index (Ex : SFX -> 0)</param>
		/// <returns>The audio clip if found</returns>
        private AudioClip GetCachedClip(string clipName, string subFolderName, int cacheIndex)
        {
            AudioClip clip;

            if (!cacheAudioClips || !cache[cacheIndex].TryGetValue(clipName, out clip))
            {
                clip = Resources.Load<AudioClip>($"{pathToAudioResources}{subFolderName}{clipName}");

                if (clip && cacheAudioClips)
                    cache[cacheIndex].Add(clipName, clip);
            }

            return clip;
        }

        public override void SetMusicVolume(float newVolume)
        {
            musicVolume = newVolume;
            mixer.SetFloat("BGM", newVolume);
        }

        public override void SetSfxVolume(float newVolume)
        {
            sfxVolume = newVolume;
            mixer.SetFloat("SFX", newVolume);
        }

        public override void SetAmbientVolume(float newVolume)
        {
            ambientVolume = newVolume;
            mixer.SetFloat("Ambient", newVolume);
        }

        public override void SetVoiceVolume(float newVolume)
        {
            voiceVolume = newVolume;
            mixer.SetFloat("Voice", newVolume);
        }

        public override float GetMusicVolume()
        {
            return musicVolume;
        }

        public override float GetAmbientVolume()
        {
            return ambientVolume;
        }

        public override float GetSfxVolume()
        {
            return sfxVolume;
        }

        public override float GetVoiceVolume()
        {
            return voiceVolume;
        }
    }

    /// <summary>
	/// Represents the monobehaviour linked to the default audio manager
	/// </summary>
    public class DefaultAudioManagerBehaviour : MonoBehaviour
    {
        private DefaultAudioManager audioManager;


        /// <summary>
		/// Initialize the audio manager
		/// </summary>
		/// <param name="audioManager">The audio manager</param>
        public void Init(DefaultAudioManager audioManager)
        {
            this.audioManager = audioManager;
        }

        void Update()
        {
            audioManager.UpdateManager();
        }
    }

    /// <summary>
	/// Represents a default audio "song" (ambient/music)
	/// </summary>
    public class DefaultAudioSong
    {
        private AudioSource source;
        private string linkedSong;
        private float baseVolume;
        private float transitionSpeed;

        public DefaultAudioSong(AudioSource source, string linkedSong, float baseVolume, float transitionSpeed = 3f, bool playAtStart = true)
        {
            this.source = source;
            this.linkedSong = linkedSong;
            this.baseVolume = baseVolume;
            this.transitionSpeed = transitionSpeed;

            source.playOnAwake = playAtStart;
            source.loop = true;
            source.volume = 0;

            if (playAtStart)
                Play();
        }

        /// <summary>
		/// Gets the name of the song
		/// </summary>
		/// <returns>Its name</returns>
        public string GetName()
        {
            return linkedSong;
        }

        /// <summary>
		/// Updates the song
		/// </summary>
		/// <param name="isCurrentMusic">True if this is the current song</param>
		/// <returns>True if this song should be deleted</returns>
        public bool UpdateSong(bool isCurrentMusic)
        {
            if (isCurrentMusic && source.volume < baseVolume)
            {
                source.volume = Mathf.MoveTowards(source.volume, baseVolume, Time.deltaTime * transitionSpeed);
            }
            else if (!isCurrentMusic && source.volume > 0)
            {
                source.volume = Mathf.MoveTowards(source.volume, 0f, Time.deltaTime * transitionSpeed);
                if (source.volume <= 0.0f)
                    return true;
            }

            return false;
        }

        /// <summary>
		/// Plays the song
		/// </summary>
        public void Play()
        {
            source.Play();
        }

        /// <summary>
        /// Stops the song
        /// </summary>
        public void Stop()
        {
            source.Stop();
        }

        /// <summary>
		/// Destroys the song
		/// </summary>
        public void Destroy()
        {
            Object.Destroy(source.gameObject);
        }
    }
}
