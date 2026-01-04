using UnityEngine;
using TennisAcademyManager.Settings;

namespace TennisAcademyManager.Core
{
    public class GameConfigService : IGameService
    {
        public GameConfig Config { get; private set; }

        public void Initialize()
        {
            Debug.Log("[GameConfigService] Initialized");
        }

        public void SetConfig(GameConfig config)
        {
            Config = config;

            if (Config == null)
                Debug.LogError("[GameConfigService] GameConfig is NULL! Assign it on GameRoot.");
        }
    }
}
