using UnityEngine;
using UnityEngine.SceneManagement;

namespace TennisAcademyManager.Core
{
    public class BootState : IGameState
    {
        public void Enter()
        {
            Debug.Log("[State] Boot");
            SceneManager.LoadScene("01_MainMenu");
        }

        public void Exit() { }
    }
}
