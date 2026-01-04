using System;
using System.Collections.Generic;
using System.Linq;
using TennisAcademyManager.Systems.Health;
using TennisAcademyManager.Systems.Players;

namespace TennisAcademyManager.Systems.Tournaments
{
    public static class TournamentBracketSim
    {
        public sealed class PlayerEntry
        {
            public string playerId;
            public PlayerInstance player;
            public float strength;
            public bool isAcademyPlayer;
        }

        public sealed class DrawResult
        {
            public Dictionary<string, Round> achievedRound = new(); // playerId -> best round achieved
            public string championId;
            public List<string> qualifiers = new();
        }

        public static DrawResult RunTournament(
            TournamentLevelSO level,
            IReadOnlyList<PlayerEntry> directMain,
            IReadOnlyList<PlayerEntry> qualifyingPool,
            int qualSlots,
            System.Random rng)
        {
            var result = new DrawResult();

            // 1) Qualifying -> winners fill qualSlots
            var qualifiers = new List<PlayerEntry>();
            if (level.hasQualifying && qualSlots > 0 && qualifyingPool != null && qualifyingPool.Count > 0)
            {
                qualifiers = RunSingleElim(
                    qualifyingPool.Take(level.qualDrawSize).ToList(),
                    Round.Qual_R32,
                    isQualifying: true,
                    rng: rng,
                    result: result);

                qualifiers = qualifiers.Take(qualSlots).ToList();
                result.qualifiers = qualifiers.Select(q => q.playerId).ToList();
            }

            // 2) Build main draw
            int mainSlots = level.mainDrawSize;
            var mainEntries = new List<PlayerEntry>();

            if (directMain != null) mainEntries.AddRange(directMain);
            mainEntries.AddRange(qualifiers);

            // pad with generated “AI field” if needed
            while (mainEntries.Count < mainSlots)
                mainEntries.Add(GenerateNpc(level, rng));

            // trim if overflow
            if (mainEntries.Count > mainSlots)
                mainEntries = mainEntries.Take(mainSlots).ToList();

            // 3) Main draw single elimination
            var championList = RunSingleElim(mainEntries, StartingMainRound(mainSlots), isQualifying: false, rng: rng, result: result);
            var champion = championList.FirstOrDefault();
            result.championId = champion?.playerId;

            return result;
        }

        // Returns list of winners in final order (champion at [0])
        private static List<PlayerEntry> RunSingleElim(
            List<PlayerEntry> entrants,
            Round startingRound,
            bool isQualifying,
            System.Random rng,
            DrawResult result)
        {
            // Ensure power-of-two by trimming (MVP)
            int pow2 = HighestPowerOfTwoAtMost(entrants.Count);
            entrants = entrants.Take(pow2).ToList();

            // Seed by strength (simple)
            entrants = entrants.OrderByDescending(e => e.strength).ToList();

            var round = startingRound;
            var current = entrants;

            while (current.Count > 1)
            {
                var next = new List<PlayerEntry>();

                for (int i = 0; i < current.Count; i += 2)
                {
                    var a = current[i];
                    var b = current[i + 1];

                    var winner = PickWinner(a, b, rng);
                    var loser = winner == a ? b : a;

                    // record achieved round for loser
                    if (!result.achievedRound.ContainsKey(loser.playerId))
                        result.achievedRound[loser.playerId] = round;

                    next.Add(winner);
                }

                current = next;
                round = NextRound(round, isQualifying);
            }

            // champion achieved W (main) or Qual_F winner (qualifying)
            var champ = current[0];
            if (!result.achievedRound.ContainsKey(champ.playerId))
                result.achievedRound[champ.playerId] = isQualifying ? Round.Qual_F : Round.W;

            return current;
        }

        private static PlayerEntry PickWinner(PlayerEntry a, PlayerEntry b, System.Random rng)
        {
            // Win probability based on strength ratio (soft)
            float sa = a.strength;
            float sb = b.strength;

            // Avoid div by zero
            float total = Math.Max(1f, sa + sb);
            float pA = sa / total;

            // Add some upset noise
            pA = Clamp01(0.15f + 0.70f * pA);

            double roll = rng.NextDouble();
            return roll < pA ? a : b;
        }

        private static Round StartingMainRound(int drawSize)
        {
            return drawSize switch
            {
                >= 128 => Round.R128,
                >= 64 => Round.R64,
                _ => Round.R32
            };
        }

        private static Round NextRound(Round round, bool isQualifying)
        {
            if (isQualifying)
            {
                return round switch
                {
                    Round.Qual_R32 => Round.Qual_R16,
                    Round.Qual_R16 => Round.Qual_QF,
                    Round.Qual_QF => Round.Qual_SF,
                    Round.Qual_SF => Round.Qual_F,
                    _ => Round.Qual_F
                };
            }

            return round switch
            {
                Round.R128 => Round.R64,
                Round.R64 => Round.R32,
                Round.R32 => Round.R16,
                Round.R16 => Round.QF,
                Round.QF => Round.SF,
                Round.SF => Round.F,
                Round.F => Round.W,
                _ => Round.W
            };
        }

        private static int HighestPowerOfTwoAtMost(int n)
        {
            int p = 1;
            while (p * 2 <= n) p *= 2;
            return p;
        }

        private static float Clamp01(float v) => v < 0 ? 0 : (v > 1 ? 1 : v);

        private static PlayerEntry GenerateNpc(TournamentLevelSO level, System.Random rng)
        {
            // NPC strength depends on tier (simple)
            float baseStrength = level.circuit switch
            {
                TennisCircuit.AITA_Juniors => 55f,
                TennisCircuit.ITF_Juniors => 65f,
                TennisCircuit.Pro => 75f,
                _ => 55f
            };

            // series/grade bump
            baseStrength += level.aitaSeries switch
            {
                AitaSeries.Talent => -8f,
                AitaSeries.Championship => -3f,
                AitaSeries.Super => +2f,
                AitaSeries.NationalSeries => +6f,
                AitaSeries.Nationals => +10f,
                _ => 0f
            };

            baseStrength += level.itfGrade switch
            {
                ItfJuniorGrade.J5 => -6f,
                ItfJuniorGrade.J4 => -2f,
                ItfJuniorGrade.J3 => +2f,
                ItfJuniorGrade.J2 => +6f,
                ItfJuniorGrade.J1 => +10f,
                _ => 0f
            };

            baseStrength += (float)(rng.NextDouble() * 12.0 - 6.0);

            return new PlayerEntry
            {
                playerId = Guid.NewGuid().ToString("N"),
                player = null,
                strength = Math.Max(10f, baseStrength),
                isAcademyPlayer = false
            };
        }
    }
}
