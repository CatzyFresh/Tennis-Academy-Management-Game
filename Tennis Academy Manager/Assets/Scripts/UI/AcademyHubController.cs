using TMPro;
using UnityEngine;
using TennisAcademyManager.Core;
using TennisAcademyManager.Systems;

namespace TennisAcademyManager.UI
{
    public class AcademyHubController : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text timeText;

        private EconomyService economy;
        private TimeService time;
        private SaveService save;
        private CalendarService calendar;

        private void Awake()
        {
            economy = ServiceLocator.Get<EconomyService>();
            time = ServiceLocator.Get<TimeService>();
            save = ServiceLocator.Get<SaveService>();
            calendar = ServiceLocator.Get<CalendarService>(); 
        }

        private void Update()
        {
            if (economy == null || time == null) return;

            moneyText.text = $"Money: ₹{economy.Cash:N0}";
            timeText.text = $"Day {calendar.Day} • {time.Hour:00}:00";
        }

        // Buttons 
        // DEV ONLY – REMOVE / HIDE IN PRODUCTION
        public void AddHour() => time.AdvanceHour();

        public void AddDay() => calendar.AdvanceDay();

        public void AddMonth() => calendar.AdvanceMonth();

        public void Earn() => economy.Earn(1000);

        public void Spend()
        {
            if (economy.CanAfford(1000)) economy.Spend(1000);
        }

        public void SaveGame() => save.SaveGame();

        public void BackToMenu()
        {
            GameRoot.Instance.ChangeState<MainMenuState>();
        }
    }
}
