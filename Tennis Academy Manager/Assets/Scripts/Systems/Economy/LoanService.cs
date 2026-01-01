using TennisAcademyManager.Core;
using UnityEngine;

namespace TennisAcademyManager.Systems
{
    public class LoanService : IGameService
    {
        public int Principal { get; private set; } = 500_000;
        public int Remaining { get; private set; }
        public int MonthlyEMI { get; private set; } = 15_000;
        public int GraceMonths { get; private set; } = 3;

        private int monthsElapsed;

        public void Initialize()
        {
            Remaining = Principal;
            Debug.Log("[LoanService] Initialized");
        }

        // ✅ Explicit disbursement step (called by GameRoot)
        public void DisburseInitialLoan(EconomyService economy)
        {
            economy.AddLedgerEntry(
                LedgerEntryType.Income,
                LedgerCategory.LoanDisbursement,
                Principal,
                "Initial bank loan disbursement"
            );

            Debug.Log($"[LoanService] Loan disbursed: ₹{Principal}");
        }

        public bool IsEMIActive => monthsElapsed >= GraceMonths && Remaining > 0;

        public void OnMonthPassed(EconomyService economy)
        {
            monthsElapsed++;

            if (!IsEMIActive) return;

            int emi = Mathf.Min(MonthlyEMI, Remaining);
            Remaining -= emi;

            economy.AddLedgerEntry(
                LedgerEntryType.Expense,
                LedgerCategory.EMI,
                emi,
                "Monthly loan EMI"
            );
        }
    }
}
