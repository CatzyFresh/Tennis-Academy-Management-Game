using System;
using TennisAcademyManager.Core;
using UnityEngine;

namespace TennisAcademyManager.Systems
{
    public class CalendarService : IGameService
    {
        public int Year { get; private set; } = 1;
        public int Month { get; private set; } = 1; // 1–12
        public int Day { get; private set; } = 1;

        public SeasonPhase CurrentSeason { get; private set; }

        // Events (core rule from GDD)
        public event Action OnDayPassed;
        public event Action OnMonthPassed;
        public event Action<SeasonPhase> OnSeasonChanged;

        public void Initialize()
        {
            UpdateSeason();
            Debug.Log("[CalendarService] Initialized");
        }

        public void AdvanceDay()
        {
            Day++;

            if (Day > DaysInMonth(Month))
            {
                Day = 1;
                AdvanceMonth();
            }

            OnDayPassed?.Invoke();
        }

        public void AdvanceMonth()
        {
            Month++;

            if (Month > 12)
            {
                Month = 1;
                Year++;
            }

            OnMonthPassed?.Invoke();
            UpdateSeason();
        }

        private void UpdateSeason()
        {
            var newSeason = Month switch
            {
                1 or 2 => SeasonPhase.PreSeason,
                3 or 4 or 5 or 6 => SeasonPhase.Competition,
                7 or 8 or 9 => SeasonPhase.Monsoon,
                10 or 11 => SeasonPhase.Peak,
                _ => SeasonPhase.OffSeason
            };

            if (newSeason != CurrentSeason)
            {
                CurrentSeason = newSeason;
                OnSeasonChanged?.Invoke(CurrentSeason);
                Debug.Log($"[Calendar] Season changed → {CurrentSeason}");
            }
        }

        private int DaysInMonth(int month)
        {
            return month switch
            {
                2 => 28,
                4 or 6 or 9 or 11 => 30,
                _ => 31
            };
        }
    }
}
