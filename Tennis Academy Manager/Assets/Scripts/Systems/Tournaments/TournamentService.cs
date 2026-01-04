using System;
using System.Collections.Generic;
using System.Linq;
using TennisAcademyManager.Core;
using TennisAcademyManager.Systems.Health;
using TennisAcademyManager.Systems.Players;
using UnityEngine;

namespace TennisAcademyManager.Systems.Tournaments
{
    public sealed class TournamentService : IGameService
    {
        public event Action OnWeeklyTournamentsGenerated;
        public event Action<string> OnTournamentCompleted; // tournament name

        private CalendarService calendar;
        private EconomyService economy;
        private ReputationService reputation; // used as federation trust proxy for now
        private PlayerService players;
        private RankingService ranking;
        private HealthGameService healthGame;

        // Configure these in inspector via a registry Mono or via config later
        private List<TournamentTemplateSO> templates = new();

        private readonly List<TournamentWeekInstance> weekly = new();

        private System.Random rng;

        public IReadOnlyList<TournamentWeekInstance> WeeklyTournaments => weekly;

        public void Initialize()
        {
            calendar = ServiceLocator.Get<CalendarService>();
            economy = ServiceLocator.Get<EconomyService>();
            reputation = ServiceLocator.Get<ReputationService>();
            players = ServiceLocator.Get<PlayerService>();
            ranking = ServiceLocator.Get<RankingService>();
            healthGame = ServiceLocator.Get<HealthGameService>();

            rng = new System.Random();
            Debug.Log("[TournamentService] Initialized");
        }

        /// <summary>
        /// Provide templates from bootstrap/config (since you don't have addressables/config pipeline yet).
        /// </summary>
        public void SetTemplates(List<TournamentTemplateSO> list)
        {
            templates = list ?? new List<TournamentTemplateSO>();
        }

        /// <summary>
        /// Called on calendar.OnWeekPassed (weekly scheduling).
        /// 1) Generates this week's tournaments.
        /// 2) Runs AI selection for academy players.
        /// 3) Simulates brackets (qual + main).
        /// 4) Applies costs (NOW), fatigue, points, prizes.
        /// </summary>
        public void RunWeeklyCycle()
        {
            GenerateWeeklyTournaments();
            RunAcademyAiEntriesAndSimulate();
        }

        private void GenerateWeeklyTournaments()
        {
            weekly.Clear();

            foreach (var t in templates)
            {
                if (t == null || t.level == null || t.pointsTable == null) continue;
                if (!t.appearsEveryWeek) continue;

                if (t.restrictBySeason && calendar.CurrentSeason != t.onlySeason) continue;

                weekly.Add(new TournamentWeekInstance(t.level, t.pointsTable));
            }

            OnWeeklyTournamentsGenerated?.Invoke();
            Debug.Log($"[TournamentService] Week {calendar.Week}: Generated {weekly.Count} tournaments");
        }

        private void RunAcademyAiEntriesAndSimulate()
        {
            int federationTrust = GetFederationTrustProxy();

            // 1) Build per tournament entry lists
            foreach (var tw in weekly)
            {
                tw.ClearEntries();

                // Each academy player decides to enter (or not) this week.
                // First-pass AI: pick ONE best tournament max per player/week.
                // We'll do selection globally: each player selects best tournament among all weekly tournaments.
            }

            var activePlayers = players.AllPlayers.Values
                .Where(p => p != null && p.Status == PlayerStatus.Active)
                .ToList();

            // player -> chosen tournament
            var chosen = new Dictionary<string, TournamentWeekInstance>();

            foreach (var p in activePlayers)
            {
                var pick = PickBestTournamentForPlayer(p, weekly, federationTrust);
                if (pick != null)
                    chosen[p.Id] = pick;
            }

            // assign players to tournament buckets
            foreach (var kv in chosen)
            {
                string playerId = kv.Key;
                var tourney = kv.Value;

                var p = activePlayers.FirstOrDefault(x => x.Id == playerId);
                if (p == null) continue;

                tourney.AddAcademyEntrant(p);
            }

            // 2) Simulate each tournament that has at least one academy entrant
            foreach (var tw in weekly)
            {
                if (tw.AcademyEntrants.Count == 0)
                    continue;

                SimulateTournament(tw, federationTrust);
            }
        }

        private TournamentWeekInstance PickBestTournamentForPlayer(
            PlayerInstance p,
            List<TournamentWeekInstance> options,
            int federationTrust)
        {
            // Hard constraints: budget & gates
            int cash = economy.Cash;

            TournamentWeekInstance best = null;
            float bestScore = float.NegativeInfinity;

            foreach (var tw in options)
            {
                var lvl = tw.Level;

                if (!IsEligible(p, lvl, federationTrust))
                    continue;

                int cost = lvl.baseTravelCost + lvl.baseTournamentExpenses;

                // Must afford NOW (punishing early career)
                if (cash < cost)
                    continue;

                // Heuristic scoring:
                // points potential - cost pressure - fatigue pressure
                float expectedPoints = EstimateExpectedPoints(p, tw);
                float costPenalty = cost / 1000f; // tune
                float fatiguePenalty = (p.Health?.Fatigue ?? 0f) / 10f;

                // discourage entering while severely injured
                if (p.Health != null && p.Health.Injury != null && p.Health.Injury.IsInjured)
                    fatiguePenalty += 3f;

                float score = expectedPoints - 1.3f * costPenalty - 1.0f * fatiguePenalty;

                // Segment-based preference (AITA for juniors, ITF gated)
                score += SegmentPreferenceBonus(p, lvl);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = tw;
                }
            }

            // simple participation throttle: not everyone plays every week
            if (best != null)
            {
                float playChance = 0.55f; // tune
                if (p.Health.Fatigue > 70f) playChance -= 0.25f;
                if (UnityEngine.Random.value > playChance) return null;
            }

            return best;
        }

        private float SegmentPreferenceBonus(PlayerInstance p, TournamentLevelSO lvl)
        {
            // Your segments: HobbyKids/CompetitiveJuniors/EliteProspects/Adults
            // (from your PlayerSegment enum used across demand/pricing)
            // We'll do a simple circuit preference:
            switch (p.Segment)
            {
                case PlayerSegment.HobbyKids:
                    return lvl.circuit == TennisCircuit.AITA_Juniors ? 3f : -6f;

                case PlayerSegment.CompetitiveJuniors:
                    return lvl.circuit == TennisCircuit.AITA_Juniors ? 2f : -4f;

                case PlayerSegment.EliteProspects:
                    // elite starts dipping into ITF if trust gates allow
                    return lvl.circuit == TennisCircuit.ITF_Juniors ? 2f : 1f;

                case PlayerSegment.Adults:
                    return lvl.circuit == TennisCircuit.Pro ? 2f : (lvl.circuit == TennisCircuit.AITA_Juniors ? -6f : -1f);

                default:
                    return 0f;
            }
        }

        private float EstimateExpectedPoints(PlayerInstance p, TournamentWeekInstance tw)
        {
            // Rough estimate: stronger player expects deeper rounds.
            // We'll map strength to a “likely round” and read points table.
            float strength = ComputeAcademyPlayerStrength(p);
            // thresholded expected achievement
            Round expected = strength switch
            {
                >= 85f => Round.W,
                >= 78f => Round.F,
                >= 72f => Round.SF,
                >= 66f => Round.QF,
                _ => Round.R16
            };

            // main points for expected round
            return tw.Points.GetPoints(DrawType.Main, expected);
        }

        private bool IsEligible(PlayerInstance p, TournamentLevelSO lvl, int federationTrust)
        {
            // federation trust gate
            if (federationTrust < lvl.minFederationTrust) return false;

            // AITA rank gate (optional; if set)
            if (lvl.circuit == TennisCircuit.AITA_Juniors && lvl.minAitaRank < 999999)
            {
                string catKey = AgeKey(lvl.ageGroup);
                int rankPos = ranking.GetRankPosition(p.Id, calendar.Year, calendar.Week, TennisCircuit.AITA_Juniors, catKey);
                if (rankPos > lvl.minAitaRank) return false;
            }

            // juniors-only age groups: you can enforce later via player age
            // for now: segments steer selection; gates are above.

            return true;
        }

        private void SimulateTournament(TournamentWeekInstance tw, int federationTrust)
        {
            var lvl = tw.Level;

            // Pay costs NOW for each academy entrant (punishing early career)
            // If player can’t pay, they withdraw.
            var paidEntrants = new List<PlayerInstance>();

            foreach (var p in tw.AcademyEntrants)
            {
                int cost = lvl.baseTravelCost + lvl.baseTournamentExpenses;
                if (!economy.TrySpendNow(cost, $"Tournament costs: {lvl.name}"))
                    continue;

                // Track in ledger too (so month summary shows it)
                economy.AddLedgerEntry(LedgerEntryType.Expense, LedgerCategory.Travel, cost, $"Tournament travel+expenses: {lvl.name}");

                // Apply tournament fatigue day
                if (healthGame?.Health != null && p.Health != null)
                {
                    // Use existing ActivityType.TournamentDay and multiplier via repeated application
                    // multiplier: apply additional "match-long" style load if >1 match expected (simple)
                    healthGame.Health.ApplyActivity(
                        p.Id,
                        p.Health,
                        new ActivityContext(ActivityType.TournamentDay, p.Health.Injury.IsInjured),
                        HealthModifiers.Default
                    );

                    // extra fatigue scaling by tier
                    if (lvl.fatigueMultiplier > 1.01f)
                    {
                        int extraTicks = Mathf.Clamp(Mathf.RoundToInt((lvl.fatigueMultiplier - 1f) * 2f), 0, 3);
                        for (int i = 0; i < extraTicks; i++)
                        {
                            healthGame.Health.ApplyActivity(
                                p.Id,
                                p.Health,
                                new ActivityContext(ActivityType.MatchLong, p.Health.Injury.IsInjured),
                                HealthModifiers.Default
                            );
                        }
                    }
                }

                paidEntrants.Add(p);
            }

            if (paidEntrants.Count == 0) return;

            // Build entries for bracket sim
            // 1) Direct main entries are seeded from academy entrants by strength
            var academyEntries = paidEntrants
                .Select(p => new TournamentBracketSim.PlayerEntry
                {
                    playerId = p.Id,
                    player = p,
                    strength = ComputeAcademyPlayerStrength(p),
                    isAcademyPlayer = true
                })
                .OrderByDescending(e => e.strength)
                .ToList();

            // decide how many direct acceptances vs qualifying
            int qualSlots = Mathf.Clamp(lvl.qualSlotsToMain, 0, lvl.mainDrawSize);
            int directSlots = lvl.mainDrawSize - qualSlots;

            var directMain = academyEntries.Take(Mathf.Min(directSlots, academyEntries.Count)).ToList();
            var qualPool = academyEntries.Skip(directMain.Count).ToList();

            // Simulate qual+main, pad with NPCs inside sim
            var draw = TournamentBracketSim.RunTournament(
                lvl,
                directMain,
                qualPool,
                qualSlots,
                rng
            );

            // Apply points + prizes to academy players
            ApplyTournamentRewards(tw, draw);

            OnTournamentCompleted?.Invoke(lvl.name);
            Debug.Log($"[Tournament] Completed: {lvl.name} | Champion: {draw.championId}");
        }

        private void ApplyTournamentRewards(TournamentWeekInstance tw, TournamentBracketSim.DrawResult draw)
        {
            var lvl = tw.Level;

            foreach (var kv in draw.achievedRound)
            {
                string pid = kv.Key;
                Round round = kv.Value;

                // only award points to academy players in this tournament instance
                if (!tw.TryGetAcademyPlayer(pid, out var player))
                    continue;

                // Determine draw type for points (qualifying/main)
                // If playerId in qualifiers list but not in main achieved rounds properly, still award qual points if table has it.
                // Simplify: if they never made main cut and level has qualifying, treat as qualifying.
                DrawType drawType = DrawType.Main;

                if (lvl.hasQualifying && tw.WasInQualifying(pid))
                {
                    // If they did not appear in main deep rounds, award qualifying points based on achieved round mapping.
                    // Our sim maps qualifying losers to qualifying rounds naturally.
                    if (round.ToString().StartsWith("Qual_")) drawType = DrawType.Qualifying;
                }

                int pts = tw.Points.GetPoints(drawType, round);

                // Category key
                string catKey = lvl.circuit == TennisCircuit.AITA_Juniors
                    ? AgeKey(lvl.ageGroup)
                    : (lvl.circuit == TennisCircuit.ITF_Juniors ? "ITFJ" : "PRO");

                ranking.AddResult(new RankingService.ResultRecord
                {
                    playerId = pid,
                    year = calendar.Year,
                    week = calendar.Week,
                    circuit = lvl.circuit,
                    categoryKey = catKey,
                    level = lvl,
                    points = pts
                });

                // Prize money (Pro only)
                if (lvl.circuit == TennisCircuit.Pro)
                {
                    int prize = round switch
                    {
                        Round.W => lvl.prizeWinner,
                        Round.F => lvl.prizeFinalist,
                        Round.SF => lvl.prizeSF,
                        Round.QF => lvl.prizeQF,
                        _ => 0
                    };

                    if (prize > 0)
                    {
                        economy.EarnNow(prize, "Prize money");
                        economy.AddLedgerEntry(LedgerEntryType.Income, LedgerCategory.PrizeShare, prize, $"Prize money: {lvl.name}");
                    }
                }
            }
        }

        private float ComputeAcademyPlayerStrength(PlayerInstance p)
        {
            if (p == null) return 50f;

            float baseStrength = p.Stats.Overall;

            // Apply health performance multiplier (fatigue/injury effect)
            if (healthGame?.Health != null && p.Health != null)
            {
                float perf = healthGame.Health.GetPerformanceMultiplier(p.Health);
                baseStrength *= perf;
            }

            // small trait nudges (optional)
            if ((p.Traits & PlayerTraitFlags.HighEndurance) != 0) baseStrength += 2f;
            if ((p.Traits & PlayerTraitFlags.Durable) != 0) baseStrength += 1f;

            // injury prone / overtrainer not directly buffed

            return Mathf.Clamp(baseStrength, 5f, 100f);
        }

        private int GetFederationTrustProxy()
        {
            // You don’t have FederationTrust system wired yet.
            // Proxy with global reputation for now (0..100 expected).
            // If your ReputationService uses different scale, adjust here.
            // (Keeping minimal coupling.)
            return Mathf.Clamp(reputation.GlobalReputation, 0, 100);
        }

        private static string AgeKey(JuniorAgeGroup g)
        {
            return g switch
            {
                JuniorAgeGroup.U10 => "U10",
                JuniorAgeGroup.U12 => "U12",
                JuniorAgeGroup.U14 => "U14",
                JuniorAgeGroup.U16 => "U16",
                _ => "U18"
            };
        }
    }

    public sealed class TournamentWeekInstance
    {
        public TournamentLevelSO Level { get; }
        public PointsTableSO Points { get; }

        private readonly List<PlayerInstance> academyEntrants = new();
        private readonly HashSet<string> qualifyingParticipants = new();

        public IReadOnlyList<PlayerInstance> AcademyEntrants => academyEntrants;

        public TournamentWeekInstance(TournamentLevelSO level, PointsTableSO points)
        {
            Level = level;
            Points = points;
        }

        public void ClearEntries()
        {
            academyEntrants.Clear();
            qualifyingParticipants.Clear();
        }

        public void AddAcademyEntrant(PlayerInstance p)
        {
            if (p == null) return;
            if (academyEntrants.Contains(p)) return;
            academyEntrants.Add(p);
        }

        public bool TryGetAcademyPlayer(string id, out PlayerInstance p)
        {
            p = academyEntrants.FirstOrDefault(x => x.Id == id);
            return p != null;
        }

        public bool WasInQualifying(string playerId) => qualifyingParticipants.Contains(playerId);

        // Used by TournamentService when it splits direct vs qual
        public void MarkQualifying(string playerId)
        {
            if (!string.IsNullOrEmpty(playerId))
                qualifyingParticipants.Add(playerId);
        }
    }
}
