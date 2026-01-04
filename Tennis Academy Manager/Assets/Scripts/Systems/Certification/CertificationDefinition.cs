using UnityEngine;

namespace TennisAcademyManager.Systems
{
    [CreateAssetMenu(menuName = "TAM/Coach/Certification", fileName = "Certification_")]
    public class CertificationDefinition : ScriptableObject
    {
        public CoachCertification Certification;

        [Header("Requirements")]
        public CoachCertification? Prerequisite;

        [Header("Cost & Time")]
        public int Cost;
        public int DurationMonths;

        [Header("ROI")]
        public float PricingToleranceBonus; // +% to pricing cap
        public float InjuryRiskReduction;   // hook for later
        public int ReputationBonus;         // coaching rep

        [Header("Display")]
        public string DisplayName;
        [TextArea] public string Description;
    }
}
