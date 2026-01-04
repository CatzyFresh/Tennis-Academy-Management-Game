using System;
using System.Collections.Generic;
using System.Linq;
using TennisAcademyManager.Core;

namespace TennisAcademyManager.Systems.Tournaments
{
    public sealed class RankingService : IGameService
    {
        public sealed class ResultRecord
        {
            public string playerId;
            public int year;
            public int week;
            public TennisCircuit circuit;
            public string categoryKey;   // "U14", "U18", "ITFJ", "PRO"
            public TournamentLevelSO level;
            public int points;
        }

        private readonly List<ResultRecord> records = new();

        // Keep it simple & tunable (best 14 results)
        private const int BestResultsCap = 14;

        // Rolling window in weeks
        private const int RollingWindowWeeks = 52;

        public void Initialize() { }

        public void AddResult(ResultRecord r)
        {
            if (r == null) return;
            records.Add(r);
        }

        public int GetRollingPoints(string playerId, int currentYear, int currentWeek, TennisCircuit circuit, string categoryKey)
        {
            int minWeekIndex = ToWeekIndex(currentYear, currentWeek) - (RollingWindowWeeks - 1);

            return records
                .Where(x => x.playerId == playerId
                            && x.circuit == circuit
                            && x.categoryKey == categoryKey
                            && ToWeekIndex(x.year, x.week) >= minWeekIndex)
                .OrderByDescending(x => x.points)
                .Take(BestResultsCap)
                .Sum(x => x.points);
        }

        // Lower rank number is better
        public int GetRankPosition(string playerId, int currentYear, int currentWeek, TennisCircuit circuit, string categoryKey)
        {
            // compute points for all players seen in that circuit/category
            var ids = records
                .Where(r => r.circuit == circuit && r.categoryKey == categoryKey)
                .Select(r => r.playerId)
                .Distinct()
                .ToList();

            if (ids.Count == 0) return 999999;

            var scores = ids
                .Select(id => new
                {
                    id,
                    pts = GetRollingPoints(id, currentYear, currentWeek, circuit, categoryKey)
                })
                .OrderByDescending(x => x.pts)
                .ToList();

            int index = scores.FindIndex(x => x.id == playerId);
            return index < 0 ? 999999 : index + 1;
        }

        private static int ToWeekIndex(int year, int week) => (year - 1) * 52 + week;
    }
}
