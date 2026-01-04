using UnityEngine;

namespace TennisAcademyManager.Systems.Tournaments
{
    [CreateAssetMenu(menuName = "TAM/Tournaments/Tournament Level", fileName = "TournamentLevel")]
    public class TournamentLevelSO : ScriptableObject
    {
        public TennisCircuit circuit;

        [Header("AITA / ITF / PRO tier")]
        public AitaSeries aitaSeries;
        public ItfJuniorGrade itfGrade;
        public ProLevel proLevel;

        [Header("Category (AITA only)")]
        public JuniorAgeGroup ageGroup = JuniorAgeGroup.U14;

        [Header("Draw")]
        public bool hasQualifying = true;
        [Min(2)] public int mainDrawSize = 32;
        [Min(0)] public int qualDrawSize = 32;

        [Tooltip("How many qualifying spots feed into main draw (typical 4/8/16).")]
        [Min(0)] public int qualSlotsToMain = 8;

        [Header("Entry Gates")]
        [Tooltip("Lower rank number is better. 999999 means 'no gate'.")]
        public int minAitaRank = 999999;

        [Range(0, 100)] public int minFederationTrust = 0;

        [Header("Economy (paid NOW for punishing early career)")]
        public int baseTravelCost = 6000;
        public int baseTournamentExpenses = 2500;

        [Header("Prize Money (pro only)")]
        public int prizeWinner = 0;
        public int prizeFinalist = 0;
        public int prizeSF = 0;
        public int prizeQF = 0;

        [Header("Fatigue")]
        [Range(0.5f, 2f)] public float fatigueMultiplier = 1.0f;
    }
}
