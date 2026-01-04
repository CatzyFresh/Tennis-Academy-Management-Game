using UnityEngine;

namespace TennisAcademyManager.Systems.Tournaments
{
    [CreateAssetMenu(menuName = "TAM/Tournaments/Tournament Template", fileName = "TournamentTemplate")]
    public class TournamentTemplateSO : ScriptableObject
    {
        public TournamentLevelSO level;
        public PointsTableSO pointsTable;

        [Header("Scheduling")]
        public bool appearsEveryWeek = true;

        [Tooltip("Optional: restrict by season")]
        public bool restrictBySeason = false;
        public SeasonPhase onlySeason = SeasonPhase.Competition;
    }
}
