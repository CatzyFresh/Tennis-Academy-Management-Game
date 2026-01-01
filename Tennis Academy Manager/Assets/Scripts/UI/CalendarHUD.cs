using TMPro;
using UnityEngine;
using TennisAcademyManager.Core;
using TennisAcademyManager.Systems;

namespace TennisAcademyManager.UI
{
    public class CalendarHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private TMP_Text seasonText;

        private CalendarService calendar;
        private System.Action<SeasonPhase> seasonHandler;

        private void Awake()
        {
            calendar = ServiceLocator.Get<CalendarService>();

            if (calendar == null)
            {
                Debug.LogError("[CalendarHUD] CalendarService not found. Is it registered in GameRoot?");
                enabled = false;
                return;
            }

            seasonHandler = season => Refresh();

            calendar.OnDayPassed += Refresh;
            calendar.OnMonthPassed += Refresh;
            calendar.OnSeasonChanged += seasonHandler;

            Refresh();
        }

        private void OnDestroy()
        {
            if (calendar == null) return;

            calendar.OnDayPassed -= Refresh;
            calendar.OnMonthPassed -= Refresh;
            calendar.OnSeasonChanged -= seasonHandler;
        }

        private void Refresh()
        {
            dateText.text =
                $"Year {calendar.Year} • Month {calendar.Month:00} • Day {calendar.Day:00}";

            seasonText.text =
                $"Season: {calendar.CurrentSeason}";
        }
    }
}
