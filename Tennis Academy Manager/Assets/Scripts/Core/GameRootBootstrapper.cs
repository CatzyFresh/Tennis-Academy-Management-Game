using UnityEngine;

namespace TennisAcademyManager.Core
{
    public class GameRootBootstrapper : MonoBehaviour
    {
        [SerializeField] private GameRoot gameRootPrefab;

        private void Awake()
        {
            if (GameRoot.Instance != null) return;

            if (gameRootPrefab == null)
            {
                Debug.LogError("[Bootstrapper] GameRoot prefab not assigned!");
                return;
            }

            Instantiate(gameRootPrefab);
        }
    }
}
