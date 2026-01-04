using UnityEngine;

namespace TennisAcademyManager.Systems
{
    [CreateAssetMenu(menuName = "TAM/Courts/Court Definition", fileName = "CourtDefinition_")]
    public class CourtDefinition : ScriptableObject
    {
        public CourtType Type;

        [Header("Economy")]
        public int BuildCost;
        public int MonthlyMaintenance;

        [Header("Gameplay (later)")]
        [Range(0f, 1f)] public float InjuryRisk;   // used later
        public int Capacity;                      // used later (sessions / players)

        [Header("Meta")]
        public string DisplayName;
        [TextArea] public string Description;
    }
}
