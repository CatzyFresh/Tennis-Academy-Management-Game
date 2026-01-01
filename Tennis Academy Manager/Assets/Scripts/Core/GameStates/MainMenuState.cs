using UnityEngine;
using UnityEngine.SceneManagement;

namespace TennisAcademyManager.Core
{
    public class MainMenuState : IGameState
    {
        public void Enter()
        {
            Debug.Log("[State] Main Menu");
            SceneManager.LoadScene("01_MainMenu");
        }

        public void Exit() { }
    }
}
