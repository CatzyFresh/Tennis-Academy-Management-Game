using TennisAcademyManager.Core;
using UnityEngine;

namespace TennisAcademyManager.Systems
{
    public class TimeService : IGameService
    {
        public int Day { get; private set; } = 1;
        public int Hour { get; private set; } = 9;

        public void Initialize()
        {
            Debug.Log("[TimeService] Initialized");
        }

        public void AdvanceHour()
        {
            Hour++;
            if (Hour >= 24)
            {
                Hour = 0;
                Day++;
            }
        }
    }
}
