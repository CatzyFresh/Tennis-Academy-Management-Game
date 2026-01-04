using System;
using System.Collections.Generic;
using System.Linq;
using TennisAcademyManager.Core;
using TennisAcademyManager.Systems.Health;
using TennisAcademyManager.Systems;

namespace TennisAcademyManager.Systems.Players
{
    public sealed class PlayerService : IGameService
    {
        public event Action OnRosterUpdated;

        private PlayerRepository repo;
        private PlayerGenerator generator;

        private HealthService healthService;

        public IReadOnlyDictionary<string, PlayerInstance> AllPlayers => repo.All;

        public void Initialize()
        {
            repo = new PlayerRepository();
            generator = new PlayerGenerator(new System.Random()); // later: seed from save

            // HealthService must be registered before PlayerService uses it (we will do this in GameRoot)
            healthService = ServiceLocator.Get<HealthGameService>()?.Health;

            if (healthService == null)
                throw new Exception("[PlayerService] HealthGameService not registered. Register it before PlayerService.");

            UnityEngine.Debug.Log("[PlayerService] Initialized");
        }

        // Called by DemandService: trials->enroll created N new active players.
        public void EnrollGeneratedPlayers(PlayerSegment segment, int count)
        {
            if (count <= 0) return;

            for (int i = 0; i < count; i++)
            {
                var p = generator.Generate(segment);
                p.SetStatus(PlayerStatus.Active);
                repo.Add(p);
            }

            OnRosterUpdated?.Invoke();
        }

        // Called by DemandService: churn N active players in segment.
        public void ReleaseActivePlayers(PlayerSegment segment, int count)
        {
            if (count <= 0) return;

            var candidates = repo.ActiveBySegment(segment).Take(count).ToList();
            foreach (var p in candidates)
                p.SetStatus(PlayerStatus.Released);

            if (candidates.Count > 0)
                OnRosterUpdated?.Invoke();
        }

        // Daily health tick (called from Calendar OnDayPassed)
        public void TickDailyAllPlayers(Func<PlayerInstance, RecoveryPlan> planResolver,
                                        Func<PlayerInstance, HealthModifiers> modifiersResolver)
        {
            foreach (var p in repo.ActivePlayers())
            {
                var plan = planResolver?.Invoke(p) ?? RecoveryPlan.FullRest;
                var mods = modifiersResolver?.Invoke(p) ?? HealthModifiers.Default;

                healthService.TickDaily(p.Id, p.Health, new RecoveryContext(plan), mods);
            }

            OnRosterUpdated?.Invoke();
        }

        // Training/Match system can call this
        public void ApplyActivity(string playerId, ActivityType type, HealthModifiers mods)
        {
            if (!repo.TryGet(playerId, out var p)) return;
            if (p.Status != PlayerStatus.Active) return;

            var ctx = new ActivityContext(type, isPlayingWhileInjured: p.Health.Injury.IsInjured);
            healthService.ApplyActivity(p.Id, p.Health, ctx, mods);

            OnRosterUpdated?.Invoke();
        }
    }
}
