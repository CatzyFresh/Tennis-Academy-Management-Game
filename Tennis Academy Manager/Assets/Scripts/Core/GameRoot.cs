using UnityEngine;
using TennisAcademyManager.Systems;
using TennisAcademyManager.Settings;
using System;
using TennisAcademyManager.Systems.Players;
using TennisAcademyManager.Systems.Health;
using TennisAcademyManager.Systems.Tournaments;
using System.Collections.Generic;

namespace TennisAcademyManager.Core
{
    public class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance { get; private set; }

        private GameStateMachine stateMachine;

        [SerializeField] private GameConfig gameConfig;

        public static event Action OnServicesReady;
        public static bool ServicesReady { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSystems();
        }

        private void InitializeSystems()
        {
            Debug.Log("[GameRoot] Initializing systems...");

            // 1️⃣ Create services (NO logic here)
            var timeService = new TimeService();
            var calendarService = new CalendarService();
            var economyService = new EconomyService();
            var loanService = new LoanService();
            var saveService = new SaveService();
            var reputationService = new ReputationService();
            var constructionService = new ConstructionService();

            var configService = new GameConfigService();
            var pricingService = new PricingService();
            var demandService = new DemandService();

            var coachService = new CoachService();
            var certificationService = new CertificationService();

            // Health + Players
            HealthGameService healthGameService = null;
            var playerService = new PlayerService();

            // Tournaments + Rankings
            var rankingService = new RankingService();
            var tournamentService = new TournamentService();

            // 2️⃣ Initialize services (self-contained only)
            timeService.Initialize();
            calendarService.Initialize();
            economyService.Initialize();
            loanService.Initialize();
            saveService.Initialize();
            reputationService.Initialize();
            constructionService.Initialize();

            configService.Initialize();
            pricingService.Initialize();
            demandService.Initialize();

            coachService.Initialize();
            certificationService.Initialize();

            // 3️⃣ Register services (global access starts here)
            ServiceLocator.Register(timeService);
            ServiceLocator.Register(calendarService);
            ServiceLocator.Register(economyService);
            ServiceLocator.Register(loanService);
            ServiceLocator.Register(saveService);
            ServiceLocator.Register(reputationService);
            ServiceLocator.Register(constructionService);

            ServiceLocator.Register(configService);
            ServiceLocator.Register(pricingService);
            ServiceLocator.Register(demandService);

            ServiceLocator.Register(coachService);
            ServiceLocator.Register(certificationService);

            // 4️⃣ Wire config dependencies (Demand + Health + Tournaments list)
            if (!WireConfigs(configService, demandService, pricingService, out var healthTuning, out var configTournamentTemplates))
            {
                Debug.LogError("[GameRoot] Config wiring failed. Aborting initialization.");
                return;
            }

            // 5️⃣ Create + Init + Register HealthGameService AFTER config validation
            healthGameService = new HealthGameService(healthTuning);
            healthGameService.Initialize();
            ServiceLocator.Register(healthGameService);

            // 6️⃣ Register + Initialize PlayerService AFTER HealthGameService is registered
            ServiceLocator.Register(playerService);
            playerService.Initialize();

            // 7️⃣ Register + Initialize RankingService BEFORE TournamentService (TournamentService depends on it)
            rankingService.Initialize();
            ServiceLocator.Register(rankingService);

            // 8️⃣ Register + Initialize TournamentService AFTER all its dependencies are registered
            tournamentService.Initialize();
            tournamentService.SetTemplates(configTournamentTemplates);
            ServiceLocator.Register(tournamentService);

            // 9️⃣ One-time startup actions
            DisburseStartingLoan(economyService, loanService);

            // 🔟 Wire events
            WireCalendarEvents(
                calendarService,
                economyService,
                loanService,
                reputationService,
                constructionService,
                pricingService,
                demandService,
                coachService,
                certificationService,
                playerService,
                tournamentService
            );

            // 1️⃣1️⃣ State machine LAST
            InitializeStateMachine();

            ServicesReady = true;
            Debug.Log("[GameRoot] All services ready");
            OnServicesReady?.Invoke();
        }

        private void DisburseStartingLoan(EconomyService economy, LoanService loan)
        {
            loan.DisburseInitialLoan(economy);
            economy.RunMonthlyClose();
        }

        private bool WireConfigs(
            GameConfigService configService,
            DemandService demandService,
            PricingService pricingService,
            out HealthTuningSO healthTuning,
            out List<TournamentTemplateSO> tournamentTemplates)
        {
            healthTuning = null;
            tournamentTemplates = null;

            configService.SetConfig(gameConfig);

            if (configService.Config == null)
            {
                Debug.LogError("[GameRoot] GameConfig is not assigned in Inspector!");
                return false;
            }

            var demandConfig = configService.Config.DemandConfig;
            if (demandConfig == null)
            {
                Debug.LogError("[GameRoot] DemandConfig missing inside GameConfig!");
                return false;
            }

            healthTuning = configService.Config.HealthTuning;
            if (healthTuning == null)
            {
                Debug.LogError("[GameRoot] HealthTuning missing inside GameConfig!");
                return false;
            }

            tournamentTemplates = configService.Config.TournamentTemplates;
            if (tournamentTemplates == null)
                tournamentTemplates = new List<TournamentTemplateSO>();

            demandService.SetConfig(demandConfig);
            pricingService.SetBaseFeesFromDemandConfig(demandConfig);

            Debug.Log("[GameRoot] Config wiring complete");
            return true;
        }

        private void WireCalendarEvents(
            CalendarService calendar,
            EconomyService economy,
            LoanService loan,
            ReputationService reputation,
            ConstructionService construction,
            PricingService pricing,
            DemandService demand,
            CoachService coach,
            CertificationService certification,
            PlayerService players,
            TournamentService tournaments)
        {
            calendar.OnDayPassed += () =>
            {
                demand.OnDayPassed(calendar, reputation, construction);

                players.TickDailyAllPlayers(
                    planResolver: _ => RecoveryPlan.FullRest,
                    modifiersResolver: _ => HealthModifiers.Default
                );
            };

            calendar.OnWeekPassed += () =>
            {
                tournaments.RunWeeklyCycle();
            };

            calendar.OnMonthPassed += () =>
            {
                pricing.RecalculateRecommendations(reputation, construction);

                int carePenalty = 0;
                foreach (PlayerSegment s in System.Enum.GetValues(typeof(PlayerSegment)))
                    carePenalty += pricing.GetCarePenalty(s);

                if (carePenalty > 0)
                    reputation.Add(
                        ReputationComponent.Care,
                        -Mathf.Min(carePenalty, 5),
                        "Overpricing reduced player care perception"
                    );

                construction.ApplyMonthlyMaintenance(economy);
                coach.ApplyMonthlySalaries(economy);
                loan.OnMonthPassed(economy, reputation);
                demand.OnMonthPassed(economy, reputation);

                foreach (var coachItem in coach.Coaches)
                {
                    certification.OnMonthPassed(
                        coachItem,
                        reputation,
                        pricing
                    );
                }

                economy.RunMonthlyClose();

                coach.RecalculateLoad(
                    demand.TotalActive(),
                    pricing
                );
            };
        }

        private void InitializeStateMachine()
        {
            stateMachine = new GameStateMachine();

            stateMachine.RegisterState(new BootState());
            stateMachine.RegisterState(new MainMenuState());
            stateMachine.RegisterState(new AcademyHubState());

            stateMachine.ChangeState<BootState>();
        }

        public void ChangeState<T>() where T : IGameState
        {
            stateMachine.ChangeState<T>();
        }

        public T GetService<T>() where T : class, IGameService
        {
            return ServiceLocator.Get<T>();
        }
    }
}
