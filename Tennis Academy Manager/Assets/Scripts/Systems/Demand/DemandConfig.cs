using UnityEngine;

namespace TennisAcademyManager.Systems
{
    [CreateAssetMenu(menuName = "TAM/Demand/Demand Config", fileName = "DemandConfig")]
    public class DemandConfig : ScriptableObject
    {
        [Header("Base inquiries per day (Tier 2 baseline)")]
        public float baseInquiriesPerDay = 4f;

        [Header("Segment distribution (must sum ~ 1.0)")]
        [Range(0, 1)] public float hobbyKidsShare = 0.40f;
        [Range(0, 1)] public float competitiveJuniorsShare = 0.30f;
        [Range(0, 1)] public float eliteProspectsShare = 0.05f;
        [Range(0, 1)] public float adultsShare = 0.25f;

        [Header("Conversion rates (baseline)")]
        [Range(0, 1)] public float inquiryToTrial = 0.25f;
        [Range(0, 1)] public float trialToEnroll = 0.30f;

        [Header("Monthly retention baseline (churn = 1-retention)")]
        [Range(0, 1)] public float monthlyRetention = 0.92f;

        [Header("Monthly fees (MVP fixed, pricing system later)")]
        public int feeHobbyKids = 2500;
        public int feeCompetitiveJuniors = 4000;
        public int feeEliteProspects = 8000;
        public int feeAdults = 3000;

        [Header("Season demand multipliers")]
        public float preSeasonMult = 1.10f;    // Jan–Feb
        public float competitionMult = 1.00f;  // Mar–Jun
        public float monsoonMult = 0.75f;      // Jul–Sep
        public float peakMult = 1.20f;         // Oct–Nov
        public float offSeasonMult = 0.85f;    // Dec
    }
}
