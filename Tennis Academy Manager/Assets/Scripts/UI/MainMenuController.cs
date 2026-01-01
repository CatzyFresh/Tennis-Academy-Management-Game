using UnityEngine;
using TennisAcademyManager.Core;
using TennisAcademyManager.Systems;


namespace TennisAcademyManager.UI
{
    public class MainMenuController : MonoBehaviour
    {
        public void OnPlayClicked()
        {
            Debug.Log("[UI] Play clicked");

            if (GameRoot.Instance == null)
            {
                Debug.LogError("[UI] GameRoot is missing. Start from 00_Boot or use Bootstrapper.");
                return;
            }

            GameRoot.Instance.ChangeState<AcademyHubState>();
        }

        public void OnContinueClicked()
        {
            Debug.Log("[UI] Continue clicked");

            if (GameRoot.Instance == null)
            {
                Debug.LogError("[UI] GameRoot is missing. Start from 00_Boot or use Bootstrapper.");
                return;
            }

            GameRoot.Instance.GetService<SaveService>().LoadGame();
            GameRoot.Instance.ChangeState<AcademyHubState>();
        }

        public void OnQuitClicked()
        {
            Debug.Log("[UI] Quit clicked");
            Application.Quit();
        }
    }
}
