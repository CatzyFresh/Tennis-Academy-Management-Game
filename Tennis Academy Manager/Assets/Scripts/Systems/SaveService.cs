using TennisAcademyManager.Core;
using UnityEngine;

namespace TennisAcademyManager.Systems
{
    public class SaveService : IGameService
    {
        public void Initialize()
        {
            Debug.Log("[SaveService] Initialized");
        }

        public void SaveGame()
        {
            Debug.Log("[SaveService] Save triggered");
        }

        public void LoadGame()
        {
            Debug.Log("[SaveService] Load triggered");
        }
    }
}
