using UnityEngine;
using UnityEngine.SceneManagement;

namespace TennisAcademyManager.Core
{
    public class AcademyHubState : IGameState
    {
        public void Enter()
        {
            Debug.Log("[State] Academy Hub");
            SceneManager.LoadScene("02_Hub_Academy");
        }

        public void Exit() { }
    }
}
