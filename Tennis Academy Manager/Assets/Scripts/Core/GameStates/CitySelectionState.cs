using UnityEngine;
using UnityEngine.SceneManagement;


namespace TennisAcademyManager.Core
{
    public class CitySelectionState : IGameState
    {
        public void Enter()
        {
            Debug.Log("[State] Boot");
            SceneManager.LoadScene("CitySelectionScene");
        }

        public void Exit()
        {
            Debug.Log("[State] City Exited");
        }

        
    }
}

