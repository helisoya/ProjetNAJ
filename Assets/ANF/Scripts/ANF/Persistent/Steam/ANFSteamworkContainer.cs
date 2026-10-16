using Leguar.TotalJSON;
using UnityEngine;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

namespace ANF.Persistent
{
    [System.Serializable]
    public class ANFSteamworkContainer : DataContainer
    {
        [SerializeField] private uint appid;
        public bool overlayOn { get; private set; }

#if !DISABLESTEAMWORKS
        protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;
#endif


        public DataContainer CloneContainer()
        {
            return new ANFSteamworkContainer()
            {
                appid = appid
            };
        }

        public void Initialize(ANFSettings settings)
        {
            new GameObject("SteamManager").AddComponent<SteamManager>().Init(appid);

#if UNITY_EDITOR
            Debug.LogWarning($"Steam Status : {SteamManager.Initialized}");
#endif

#if !DISABLESTEAMWORKS
            if (SteamManager.Initialized)
            {
                // Setup steamwork
                m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
            }
#endif
        }


        /// <summary>
		/// Unlocks an achievement
		/// </summary>
		/// <param name="achievementName">The achievement's ID</param>
        public void UnlockAchievement(string achievementName)
        {
#if !DISABLESTEAMWORKS
            if (SteamManager.Initialized)
            {
                if (SteamUserStats.GetAchievement(achievementName, out bool gotten) && !gotten)
                {
                    SteamUserStats.SetAchievement(achievementName);
                    SteamUserStats.StoreStats();
                }
            }
#endif
        }


#if !DISABLESTEAMWORKS
        private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
        {
            overlayOn = pCallback.m_bActive != 0;
        }
#endif



        // Unused

        public void Reset()
        {

        }

        public void Save(JSON json)
        {

        }

        public bool Load(JSON json)
        {
            return true;
        }
    }
}