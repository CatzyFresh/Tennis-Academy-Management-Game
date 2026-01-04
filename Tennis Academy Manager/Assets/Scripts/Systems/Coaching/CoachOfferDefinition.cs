using UnityEngine;

namespace TennisAcademyManager.Systems
{
    [CreateAssetMenu(menuName = "TAM/Coach/Offer", fileName = "CoachOffer_")]
    public class CoachOfferDefinition : ScriptableObject
    {
        [Header("Profile")]
        public string CoachName;
        public CoachRole Role;

        [Header("Starting Certification Level (optional)")]
        public CoachCertification? StartingCertification; // can be null

        [Header("Hiring + Salary")]
        public int HiringCost;
        public int ExpectedMonthlySalary;

        [Header("Capacity")]
        public int Capacity;
    }
}
