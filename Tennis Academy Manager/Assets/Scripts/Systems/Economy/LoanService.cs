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

        private int missedEmiCount;


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

        public void OnMonthPassed(EconomyService economy, ReputationService reputation)
        {
            monthsElapsed++;

            if (!IsEMIActive) return;

            int emi = Mathf.Min(MonthlyEMI, Remaining);

            // MVP rule: EMI must be payable from current cash
            if (economy.Cash >= emi)
            {
                Remaining -= emi;

                economy.AddLedgerEntry(
                    LedgerEntryType.Expense,
                    LedgerCategory.EMI,
                    emi,
                    "Monthly loan EMI"
                );

                missedEmiCount = 0;

                // Discipline reward (small, conservative)
                reputation.Add(ReputationComponent.Discipline, +1, "EMI paid on time");
            }
            else
            {
                missedEmiCount++;

                // Discipline penalty
                reputation.Add(ReputationComponent.Discipline, -8, "EMI missed");

                // Optional: add penalty fee (small) to ledger (bank penalty)
                economy.AddLedgerEntry(
                    LedgerEntryType.Expense,
                    LedgerCategory.EMI,
                    2000,
                    "Late payment penalty"
                );

                // Default escalation (MVP hook)
                if (missedEmiCount >= 2)
                {
                    reputation.Add(ReputationComponent.Discipline, -12, "Loan default risk escalated");
                    Debug.LogWarning("[Loan] Multiple missed EMIs → default escalation triggered (hook for downsizing later).");
                }
            }
        }

    }
}
