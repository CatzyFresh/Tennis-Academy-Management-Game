using UnityEngine;
using TennisAcademyManager.Systems;

namespace TennisAcademyManager.Core
{
    public class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSystems();
        }

        private void InitializeSystems()
        {
            Debug.Log("[GameRoot] Initializing systems...");

            var timeService = new TimeService();
            var economyService = new EconomyService();
            var saveService = new SaveService();

            timeService.Initialize();
            economyService.Initialize();
            saveService.Initialize();

            ServiceLocator.Register(timeService);
            ServiceLocator.Register(economyService);
            ServiceLocator.Register(saveService);
        }

    }
}
