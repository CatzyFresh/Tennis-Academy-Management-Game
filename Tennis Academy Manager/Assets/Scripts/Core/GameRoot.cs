using UnityEngine;
using TennisAcademyManager.Systems;

namespace TennisAcademyManager.Core
{
    public class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance { get; private set; }
        private GameStateMachine stateMachine;

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

            // 2️⃣ Initialize services (self-contained only)
            timeService.Initialize();
            calendarService.Initialize();
            economyService.Initialize();
            loanService.Initialize();
            saveService.Initialize();

            // 3️⃣ Register services (global access starts here)
            ServiceLocator.Register(timeService);
            ServiceLocator.Register(calendarService);
            ServiceLocator.Register(economyService);
            ServiceLocator.Register(loanService);
            ServiceLocator.Register(saveService);

            // 4️⃣ One-time startup actions (orchestration)
            DisburseStartingLoan(economyService, loanService);

            // 5️⃣ Wire event-driven system dependencies
            WireCalendarEvents(calendarService, economyService, loanService);

            // 6️⃣ Initialize game state flow LAST
            InitializeStateMachine();
        }

        private void DisburseStartingLoan(EconomyService economy,LoanService loan)
        {
            loan.DisburseInitialLoan(economy);

            // Apply immediately so starting cash is correct
            economy.RunMonthlyClose();
        }

        private void WireCalendarEvents(CalendarService calendar,EconomyService economy,LoanService loan)
        {
            calendar.OnMonthPassed += () =>
            {
                loan.OnMonthPassed(economy);
                economy.RunMonthlyClose();
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
